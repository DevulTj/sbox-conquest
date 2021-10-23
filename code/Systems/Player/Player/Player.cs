using Conquest.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class Player : BasePlayer, IMiniMapEntity, IHudMarkerEntity, IGameStateAddressable
	{
		/// <summary>
		/// The clothing container is what dresses the citizen
		/// </summary>
		public Clothing.Container Clothing = new();

		[Net, Predicted] public ICamera MainCamera { get; set; }

		[Net, Predicted] private bool _IsSprinting { get; set; }

		[Net, Predicted] public TimeSince SinceSprintStopped { get; set; }

		public bool IsSprinting { get => _IsSprinting; protected set { if (_IsSprinting && !value) SinceSprintStopped = 0; _IsSprinting = value; } }

		[Net, Predicted] public bool IsBurstSprinting { get; protected set; }
		[Net, Predicted] public bool IsAiming { get; protected set; }
		[Net, Predicted] public bool IsFreeLooking { get; protected set; }

		[Net, Local]
		public CapturePointEntity CapturePoint { get; set; }

		public ICamera LastCamera { get; set; }

		public TimeSince TimeSinceDeath { get; set; }

		protected override void MakeHud()
		{
			Hud = new PlayerHud();
		}

		protected override void OnDestroy()
		{
			if ( CapturePoint.IsValid() )
			{
				CapturePoint.RemovePlayer( this );
			}

			base.OnDestroy();
		}

		public Player() : base()
		{
			Inventory = new PlayerInventory( this );
		}

		public Player( Client cl ) : this()
		{
			// Load clothing from client data
			Clothing.LoadFromClient( cl );
		}

		public override void Spawn()
		{
			MainCamera = new FootCamera();
			LastCamera = MainCamera;

			base.Spawn();
		}

		protected async Task TryMakeMarker()
		{
			while ( HudMarkers.Current is null )
			{
				await GameTask.NextPhysicsFrame();
				return;
			}
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			_ = TryMakeMarker();
		}

		protected virtual void StripWeapons()
		{
			ClearAmmo();

			// Clear a player's inventory
			Inventory?.DeleteContents();
		}

		protected virtual void GiveLoadout()
		{
			Inventory.Add( new SMG(), true );
			Inventory.Add( new Pistol() );

			GiveAmmo( AmmoType.Pistol, 36 );
			GiveAmmo( AmmoType.Rifle, 180 );
		}

		protected virtual void SoftRespawn()
		{
			StripWeapons();
			GiveLoadout();
			Clothing.DressEntity( this );
			MoveToSpawnpoint( this );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();
			Animator = new PlayerAnimator();

			MainCamera = LastCamera as FootCamera;
			Camera = MainCamera;

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();

			SoftRespawn();
		}

		[ClientRpc]
		protected void UpdateKillFeed( ulong lsteamid, string leftName, ulong rsteamid, string rightName, string method )
		{
			KillFeedPanel.Current.AddKill( lsteamid, leftName, rsteamid, rightName, method );
		}

		public virtual void OnPlayerKill( Player victim, DamageInfo damageInfo )
		{
			Event.Run( PlayerEvent.Server.OnPlayerKilled, victim, damageInfo );

			if ( TeamSystem.IsFriendly( victim.Team, Team ) )
				GiveAward( "TeamKill" );
			else
				GiveAward( "Kill" );

			UpdateKillFeed( this.Client.SteamId, this.Client.Name, victim.Client.SteamId, victim.Client.Name, damageInfo.Weapon.ClassInfo.Title );
		}

		public override void OnKilled()
		{
			base.OnKilled();

			TimeSinceDeath = 0;

			Inventory.DeleteContents();

			BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );

			// Remove a ticket.
			Game.Current.Scores.RemoveScore( Team, 1 );

			Controller = null;
			Camera = new SpectateRagdollCamera();

			EnableAllCollisions = false;
			EnableDrawing = false;

			var attacker = LastAttacker;
			if ( attacker.IsValid() )
			{
				if ( attacker is Player killer )
				{
					killer.OnPlayerKill( this, LastDamage );
				}
			}
		}

		public ICamera GetActiveCamera()
		{
			return MainCamera;
		}

		[Net, Predicted]
		public bool BurstSprintBlock { get; set; } = false;

		[Net, Predicted]
		public TimeSince SinceBurstActivated { get; set; }

		public float BurstStaminaDuration => 5f;

		protected void HandleSharedInput( Client cl )
		{
			if ( Input.Pressed( InputButton.Drop ) )
			{
				var dropped = Inventory.DropActive();
				if ( dropped != null )
				{
					if ( dropped.PhysicsGroup != null )
						dropped.PhysicsGroup.Velocity = Velocity + (EyeRot.Forward + EyeRot.Up) * 300;

					SwitchToBestWeapon();
				}
			}
			var isReloading = ActiveChild is BaseWeapon weapon && weapon.IsReloading;
			IsAiming = !IsSprinting && Input.Down( InputButton.Attack2 ) && !isReloading;

			if ( IsSprinting && Input.Pressed( InputButton.Run ) )
			{
				if ( Input.Forward > 0.5f )
				{
					IsBurstSprinting = !IsBurstSprinting;

					if ( IsBurstSprinting )
						SinceBurstActivated = 0;
				}
			}

			if ( SinceBurstActivated > BurstStaminaDuration )
				IsBurstSprinting = false;

			if ( Input.Pressed( InputButton.Run ) )
			{
				if ( !IsSprinting )
					IsSprinting = true;
				else if ( IsSprinting && Input.Forward < 0.5f )
					IsSprinting = false;
			}

			if ( !IsBurstSprinting && IsSprinting && Velocity.Length < 40 || Input.Forward < 0.5f )
				IsSprinting = false;

			if ( Input.Down( InputButton.Attack1 ) || Input.Down( InputButton.Attack2 ) )
				IsSprinting = false;

			if ( !IsSprinting )
				IsBurstSprinting = false;

			IsFreeLooking = Input.Down( InputButton.Walk );
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			// @TODO: This is fucking awful
			if ( ActiveChild is BaseCarriable weapon && weapon.CrosshairPanel is not null )
			{
				if ( IsAiming && !weapon.CrosshairPanel.HasClass( "aim" ) )
				{
					weapon.CrosshairPanel.AddClass( "aim" );
				}
				else if ( !IsAiming && weapon.CrosshairPanel.HasClass( "aim" ) )
				{
					weapon.CrosshairPanel.RemoveClass( "aim" );
				}

				if ( Velocity.Length > 10 && !weapon.CrosshairPanel.HasClass( "move" ) )
				{
					weapon.CrosshairPanel.AddClass( "move" );
				}
				else if ( Velocity.Length <= 10 && weapon.CrosshairPanel.HasClass( "move" ) )
				{
					weapon.CrosshairPanel.RemoveClass( "move" );
				}

				if ( IsSprinting && !weapon.CrosshairPanel.HasClass( "movefast" ) )
				{
					weapon.CrosshairPanel.AddClass( "movefast" );
				}
				else if ( !IsSprinting && weapon.CrosshairPanel.HasClass( "movefast" ) )
				{
					weapon.CrosshairPanel.RemoveClass( "movefast" );
				}
			}
		}

		public void SwitchToBestWeapon()
		{
			var best = Children.Select( x => x as ICarriable )
				.Where( x => x is not null && x.IsUsable() )
				.OrderByDescending( x => x.BucketWeight )
				.FirstOrDefault();

			if ( best == null ) return;

			ActiveChild = best as BaseCarriable;
		}

		public void BecomeSpectator()
		{
			var cl = Client;
			var player = new SpectatorPlayer( cl );
			Client.Pawn = player;

			Delete();

			player.Respawn();
		}

		public override void Simulate( Client cl )
		{
			if ( LifeState == LifeState.Dead )
			{
				if ( TimeSinceDeath > 3 && IsServer )
				{
					BecomeSpectator();
				}

				return;
			}

			HandleSharedInput( cl );

			if ( Input.ActiveChild != null )
				ActiveChild = Input.ActiveChild;

			if ( LifeState != LifeState.Alive )
				return;

			Camera = GetActiveCamera();

			if ( Input.Released( InputButton.View ) )
			{
				if ( MainCamera is FootCamera )
					MainCamera = new ThirdPersonCamera();
				else
					MainCamera = new FootCamera();
			}

			IsFreeLooking = Input.Down( InputButton.Walk );

			var controller = GetActiveController();
			if ( controller != null )
				EnableSolidCollisions = !controller.HasTag( "noclip" );

			controller?.Simulate( cl, this, GetActiveAnimator() );

			TickPlayerUse();

			SimulateActiveChild( cl, ActiveChild );
		}

		public override PawnController GetActiveController()
		{
			return base.GetActiveController();
		}

		public virtual void Notify( string message, string hex = "#ffffff" )
		{
			// NotificationBox.AddChatEntry( To.Single( Client ), message, hex );
		}

		protected int GetSlotIndexFromInput( InputButton slot )
		{
			return slot switch
			{
				InputButton.Slot1 => 0,
				InputButton.Slot2 => 1,
				InputButton.Slot3 => 2,
				InputButton.Slot4 => 3,
				InputButton.Slot5 => 4,
				_ => -1
			};
		}

		protected void TrySlotFromInput( InputBuilder input, InputButton slot )
		{
			if ( Input.Pressed( slot ) )
			{
				input.SuppressButton( slot );

				if ( Inventory.GetSlot( GetSlotIndexFromInput( slot ) ) is Entity weapon )
					input.ActiveChild = weapon;
			}
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );

			TrySlotFromInput( input, InputButton.Slot1 );
			TrySlotFromInput( input, InputButton.Slot2 );
			TrySlotFromInput( input, InputButton.Slot3 );
			TrySlotFromInput( input, InputButton.Slot4 );
			TrySlotFromInput( input, InputButton.Slot5 );
		}

		void IGameStateAddressable.ResetState()
		{
			BecomeSpectator();
		}
	}
}
