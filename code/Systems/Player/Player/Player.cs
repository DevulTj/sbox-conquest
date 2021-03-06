using Conquest.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest;

public partial class Player : BasePlayer, IMiniMapEntity, IHudMarkerEntity, IGameStateAddressable
{
	[BindComponent] public CameraMode MainCamera { get; }
	[BindComponent] public SpectateRagdollCamera DeathCamera { get; }
	[Net, Predicted] private bool _IsSprinting { get; set; }
	[Net, Predicted] public TimeSince SinceSprintStopped { get; set; }
	[Net, Predicted] public bool IsBurstSprinting { get; protected set; }
	[Net, Predicted] public bool IsAiming { get; protected set; }
	[Net, Local] public CapturePointEntity CapturePoint { get; set; }

	public CameraMode LastCamera { get; set; }
	public TimeSince TimeSinceDeath { get; set; }
	public ClothingContainer Clothing { get; set; } = new();

	public bool IsSprinting { get => _IsSprinting; protected set { if ( _IsSprinting && !value ) SinceSprintStopped = 0; _IsSprinting = value; } }


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

		DestroySpeedLines();

		base.OnDestroy();
	}

	public Player() : base()
	{
		Inventory = new PlayerInventory( this );

		Tags.Add( "player" );
	}

	public Player( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Spawn()
	{
		Components.RemoveAny<SpectateRagdollCamera>();
		Components.GetOrCreate<FootCamera>();

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

		InitSpeedLines();

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

		var primaryAttribute = TypeLibrary.GetTypeByName( Client.GetClientData( "conquest_loadout_primary" ) );
		BaseWeapon primary = primaryAttribute != null ? TypeLibrary.Create<BaseWeapon>( primaryAttribute ) : new FAL();
		var secondaryAttribute = TypeLibrary.GetTypeByName( Client.GetClientData( "conquest_loadout_secondary" ) );
		BaseWeapon secondary = secondaryAttribute != null ? TypeLibrary.Create<BaseWeapon>( secondaryAttribute ) : new DesertEagle();

		Inventory.Add( primary, true );
		Inventory.Add( secondary );
		Inventory.Add( new AmmoCrateGadget() );
		Inventory.Add( new Knife() );

		GiveAmmo( AmmoType.Pistol, 36 );
		GiveAmmo( AmmoType.Rifle, 180 );
		GiveAmmo( AmmoType.Shotgun, 36 );

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

		Components.GetOrCreate<FootCamera>();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		base.Respawn();

		SoftRespawn();
	}

	[ClientRpc]
	protected void UpdateKillFeed( long lsteamid, string leftName, long rsteamid, string rightName, string method )
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

		UpdateKillFeed( this.Client.PlayerId, this.Client.Name, victim.Client.PlayerId, victim.Client.Name, DisplayInfo.For( damageInfo.Weapon ).Name );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		TimeSinceDeath = 0;

		Inventory.DropActive();

		Inventory.DeleteContents();

		BecomeRagdollOnClient( Velocity, LastDamage.Flags, LastDamage.Position, LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );
			
		// Remove a ticket.
		Game.Current.Scores.RemoveScore( Team, 1 );

		Controller = null;

		Components.RemoveAny<FootCamera>();
		Components.GetOrCreate<SpectateRagdollCamera>();

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

		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			child.EnableDrawing = false;
		}
	}

	public CameraMode GetActiveCamera()
	{
		return MainCamera;
	}

	[Net, Predicted]
	public bool BurstSprintBlock { get; set; } = false;

	[Net, Predicted]
	public TimeSince SinceBurstActivated { get; set; }

	public float BurstStaminaDuration => 5f;

	private TimeSince TimeSinceWeaponDropped = -1;

	protected void HandleSharedInput( Client cl )
	{
		if ( Input.Released( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped.IsValid() )
			{
				if ( dropped.PhysicsGroup != null )
					dropped.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * 300;

				TimeSinceWeaponDropped = 0;

				SwitchToBestWeapon();
			}
		}

		var isReloading = ActiveChild is BaseWeapon weapon && weapon.IsReloading;
		IsAiming = !IsSprinting && Input.Down( InputButton.SecondaryAttack );

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

		if ( Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack) )
			IsSprinting = false;

		if ( !IsSprinting )
			IsBurstSprinting = false;

		if ( Input.Pressed( InputButton.SlotPrev ) )
		{
			Inventory.SwitchActiveSlot( -1, true );
		}
		else if ( Input.Pressed( InputButton.SlotNext ) )
		{
			Inventory.SwitchActiveSlot( 1, true );
		}
	}

	public Entity GetUsableEntity()
	{
		return FindUsable();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		// @TODO: This is fucking awful
		//if ( ActiveChild is BaseCarriable weapon && weapon.CrosshairPanel is not null )
		//{
		//	if ( IsAiming && !weapon.CrosshairPanel.HasClass( "aim" ) )
		//	{
		//		weapon.CrosshairPanel.AddClass( "aim" );
		//	}
		//	else if ( !IsAiming && weapon.CrosshairPanel.HasClass( "aim" ) )
		//	{
		//		weapon.CrosshairPanel.RemoveClass( "aim" );
		//	}

		//	if ( Velocity.Length > 10 && !weapon.CrosshairPanel.HasClass( "move" ) )
		//	{
		//		weapon.CrosshairPanel.AddClass( "move" );
		//	}
		//	else if ( Velocity.Length <= 10 && weapon.CrosshairPanel.HasClass( "move" ) )
		//	{
		//		weapon.CrosshairPanel.RemoveClass( "move" );
		//	}

		//	if ( IsSprinting && !weapon.CrosshairPanel.HasClass( "movefast" ) )
		//	{
		//		weapon.CrosshairPanel.AddClass( "movefast" );
		//	}
		//	else if ( !IsSprinting && weapon.CrosshairPanel.HasClass( "movefast" ) )
		//	{
		//		weapon.CrosshairPanel.RemoveClass( "movefast" );
		//	}
		//}
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

		CameraMode = GetActiveCamera();

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		controller?.Simulate( cl, this, GetActiveAnimator() );

		TickPlayerUse();

		SimulateActiveChild( cl, ActiveChild );

		SimulateDamage();

		using ( Prediction.Off() )
		{
			if ( IsServer && Input.Released( InputButton.Flashlight ) )
				Ping();
		}
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

	public override void StartTouch( Entity other )
	{
		if ( TimeSinceWeaponDropped < 1 ) return;

		base.StartTouch( other );
	}
}
