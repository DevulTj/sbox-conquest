using Sandbox;
using System;
using System.Collections.Generic;

namespace Conquest
{
	public class ViewModelInfo
	{
		public ViewModelInfo( Carriable weaponRef )
		{
			Weapon = weaponRef;
		}

		public virtual Carriable Weapon { get; set; }

		public virtual Vector3 WalkCycleOffsets => new Vector3( 50f, 20f, 50f );
		public virtual float ForwardBobbing => 4f;
		public virtual float SideWalkOffset => 100f;
		public virtual Vector3 AimOffset => new Vector3( 10f, 10, 1.8f );
		public virtual Vector3 Offset => new Vector3( -6f, 5f, -5f );
		public virtual Vector3 CrouchOffset => new Vector3( -10f, -50f, -0f );
		public virtual float OffsetLerpAmount => 30f;

		public virtual float SprintRightRotation => 20f;
		public virtual float SprintUpRotation => -30f;
		public virtual float SprintLeftOffset => -35f;
		public virtual float PostSprintLeftOffset => 5f;

		public virtual float BurstSprintRightRotation => 20f;
		public virtual float BurstSprintUpRotation => -30f;
		public virtual float BurstSprintLeftOffset => -35f;
		public virtual float BurstPostSprintLeftOffset => 5f;
	}

	public class PistolViewModelInfo : ViewModelInfo
	{
		public PistolViewModelInfo( Carriable weaponRef ) : base( weaponRef )
		{

		}

		public override float BurstSprintRightRotation => 2f;
		public override float BurstSprintUpRotation => 2f;
		public override float BurstSprintLeftOffset => -35f;
		public override float BurstPostSprintLeftOffset => 5f;
	}

	public class SMGViewModelInfo : ViewModelInfo
	{
		public SMGViewModelInfo( Carriable weaponRef ) : base( weaponRef )
		{

		}

		public override float SprintRightRotation => 2f;
		public override float SprintUpRotation => 2f;

		public override float BurstSprintRightRotation => 20f;
		public override float BurstSprintUpRotation => -30f;
		public override float BurstSprintLeftOffset => -35f;
		public override float BurstPostSprintLeftOffset => 5f;
	}

	public interface ICarriable
	{
		public Entity Entity => this as Entity;

		public virtual int Bucket => 1;
		public virtual int BucketWeight => 100;
		public bool IsUsable();
	}

	public enum WeaponSlot
	{
		Primary,
		Secondary,
		Gadget
	}

	public partial class Carriable : BaseCarriable, IUse, ICarriable
	{
		public virtual WeaponSlot Slot => WeaponSlot.Primary;

		public virtual bool ShowAmmoCount => true;

		public virtual int Bucket => 1;
		public virtual int BucketWeight => 100;

		public virtual float PrimaryRate => 5.0f;
		public virtual float SecondaryRate => 15.0f;

		public virtual ViewModelInfo VMInfo => new ViewModelInfo( this );

		public override void Spawn()
		{
			base.Spawn();

			CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
			SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it
		}

		[Net, Predicted]
		public TimeSince TimeSincePrimaryAttack { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceSecondaryAttack { get; set; }

		public override void Simulate( Client player )
		{
			if ( CanReload() )
			{
				Reload();
			}

			//
			// Reload could have changed our owner
			//
			if ( !Owner.IsValid() )
				return;

			if ( CanPrimaryAttack() )
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary();
			}

			//
			// AttackPrimary could have changed our owner
			//
			if ( !Owner.IsValid() )
				return;

			if ( CanSecondaryAttack() )
			{
				TimeSinceSecondaryAttack = 0;
				AttackSecondary();
			}
		}

		public virtual bool CanReload()
		{
			if ( !Owner.IsValid() ) return false;

			return true;
		}

		public virtual void Reload()
		{

		}

		public virtual bool CanPrimaryAttack()
		{
			if ( !Owner.IsValid() || !Input.Down( InputButton.Attack1 ) ) return false;

			if ( (Owner as Player).SinceSprintStopped < 0.2f ) return false;

			var rate = PrimaryRate;
			if ( rate <= 0 ) return true;

			return TimeSincePrimaryAttack > (1 / rate);
		}

		public virtual void AttackPrimary()
		{

		}

		public virtual bool CanSecondaryAttack()
		{
			if ( !Owner.IsValid() || !Input.Down( InputButton.Attack2 ) ) return false;

			var rate = SecondaryRate;
			if ( rate <= 0 ) return true;

			return TimeSinceSecondaryAttack > (1 / rate);
		}

		public virtual void AttackSecondary()
		{

		}

		/// <summary>
		/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
		/// hits, like if you're going through layers or ricocet'ing or something.
		/// </summary>
		public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
		{
			bool InWater = Physics.TestPointContents( start, CollisionLayer.Water );

			var tr = Trace.Ray( start, end )
					.UseHitboxes()
					.HitLayer( CollisionLayer.Water, !InWater )
					.Ignore( Owner )
					.Ignore( this )
					.Size( radius )
					.Run();

			yield return tr;

			//
			// Another trace, bullet going through thin material, penetrating water surface?
			//
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			var viewmodel = new ViewModel
			{
				Position = Position,
				Owner = Owner,
				EnableViewmodelRendering = true,
				VMInfo = VMInfo
			};

			ViewModelEntity = viewmodel;
			ViewModelEntity.SetModel( ViewModelPath );
		}

		public bool OnUse( Entity user )
		{
			return false;
		}

		public virtual bool IsUsable()
		{
			return true;
		}

		public bool IsUsable( Entity user )
		{
			return Owner == null;
		}
	}

	public partial class BaseWeapon : Carriable
	{
		public virtual AmmoType AmmoType => AmmoType.Pistol;
		public virtual int ClipSize => 16;
		public virtual float ReloadTime => 3.0f;

		[Net, Predicted]
		public int AmmoClip { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceReload { get; set; }

		[Net, Predicted]
		public bool IsReloading { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceDeployed { get; set; }


		public PickupTrigger PickupTrigger { get; protected set; }


		public override bool CanReload()
		{
			if ( !Owner.IsValid() ) return false;
			if ( AmmoClip > 0 && !Input.Down( InputButton.Reload ) ) return false;

			if ( AmmoClip == 0 ) return true;

			return true;
		}


		public int AvailableAmmo()
		{
			var owner = Owner as Player;
			if ( owner == null ) return 0;
			return owner.AmmoCount( AmmoType );
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			TimeSinceDeployed = 0;

			IsReloading = false;
		}

		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override void Spawn()
		{
			base.Spawn();

			PickupTrigger = new PickupTrigger();
			PickupTrigger.Parent = this;
			PickupTrigger.Position = Position;
		}

		public override void Reload()
		{
			if ( IsReloading )
				return;

			if ( AmmoClip >= ClipSize )
				return;

			TimeSinceReload = 0;

			if ( Owner is Player player )
			{
				if ( player.AmmoCount( AmmoType ) <= 0 )
					return;
			}

			IsReloading = true;

			(Owner as AnimEntity).SetAnimBool( "b_reload", true );

			StartReloadEffects();
		}

		public override void Simulate( Client owner )
		{
			if ( TimeSinceDeployed < 0.6f )
				return;

			if ( !IsReloading )
			{
				base.Simulate( owner );
			}

			if ( IsReloading && TimeSinceReload > ReloadTime )
			{
				OnReloadFinish();
			}
		}

		public virtual void OnReloadFinish()
		{
			IsReloading = false;

			if ( Owner is Player player )
			{
				var ammo = player.TakeAmmo( AmmoType, ClipSize - AmmoClip );
				if ( ammo == 0 )
					return;

				AmmoClip += ammo;
			}
		}

		[ClientRpc]
		public virtual void StartReloadEffects()
		{
			ViewModelEntity?.SetAnimBool( "reload", true );

			// TODO - player third person model reload
		}

		public override bool CanPrimaryAttack()
		{
			if ( Owner is Player player )
			{
				if ( player.IsSprinting )
					return false;
			}
	
			return base.CanPrimaryAttack();
		}

		public virtual float GetAttackSpreadMultiplier()
		{
			var radius = 1f;
			var player = Owner as Player;

			if ( player.IsAiming )
				radius *= .5f;

			if ( player.GroundEntity is null )
				radius *= 4f;

			return radius;
		}

		protected Vector3 SeededRandom()
		{
			Rand.SetSeed( DateTime.Now.GetEpoch() );
			return Vector3.Random;
		}

		public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
		{
			spread *= GetAttackSpreadMultiplier();

			var forward = Owner.EyeRot.Forward;
			forward += SeededRandom() * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				//
				// We turn predictiuon off for this, so any exploding effects don't get culled etc
				//
				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			//
			// Tell the clients to play the shoot effects
			//
			ShootEffects();

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward * 5000 ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				//
				// We turn predictiuon off for this, so aany exploding effects don't get culled etc
				//
				using ( Prediction.Off() )
				{
					var damage = DamageInfo.FromBullet( tr.EndPos, Owner.EyeRot.Forward * 100, 15 )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damage );
				}
			}
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

			if ( IsLocalPawn )
			{
				new Conquest.ScreenShake.Perlin();
			}

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );
		}

		public bool TakeAmmo( int amount )
		{
			if ( AmmoClip < amount )
				return false;

			AmmoClip -= amount;
			return true;
		}

		[ClientRpc]
		public virtual void DryFire()
		{
			// CLICK
		}

		public override void CreateHudElements()
		{
			if ( Local.Hud == null )
			{
				Log.Warning( "no hud wagwan" );
				return;
			}

			Log.Info( "crosshair created wagwan" );

			CrosshairPanel = new Crosshair();
			CrosshairPanel.Parent = Local.Hud;
			CrosshairPanel.AddClass( ClassInfo.Name );
		}

		public bool IsUsable()
		{
			if ( AmmoClip > 0 ) return true;
			return AvailableAmmo() > 0;
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );

			if ( PickupTrigger.IsValid() )
			{
				PickupTrigger.EnableTouch = false;
			}
		}

		public override void OnCarryDrop( Entity dropper )
		{
			base.OnCarryDrop( dropper );

			if ( PickupTrigger.IsValid() )
			{
				PickupTrigger.EnableTouch = true;
			}
		}

	}
}
