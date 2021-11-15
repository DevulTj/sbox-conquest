using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Conquest
{
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
		public virtual WeaponSlot Slot => WeaponInfo.Slot;

		public virtual bool ShowAmmoCount => true;

		public virtual int Bucket => 1;
		public virtual int BucketWeight => 100;

		public virtual float PrimaryRate => 5.0f;
		public virtual float SecondaryRate => 15.0f;

		public WeaponInfoAsset WeaponInfo
		{
			get
			{
				WeaponInfoAsset.Registry.TryGetValue( this.ClassInfo.Name, out var asset );
				return asset;
			}
		}

		public override void Spawn()
		{
			base.Spawn();

			CollisionGroup = CollisionGroup.Weapon; // so players touch it as a trigger but not as a solid
			SetInteractsAs( CollisionLayer.Debris ); // so player movement doesn't walk into it

			SetModel( WeaponInfo.WorldModel );
		}

		[Net, Predicted]
		public TimeSince TimeSincePrimaryAttack { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceSecondaryAttack { get; set; }


		[Net, Predicted]
		bool WishToShoot { get; set; } = false;
		public void SetWantsToShoot( bool want )
		{
			WishToShoot = want;
		}

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

			if ( !Owner.Client.IsBot )
				SetWantsToShoot( Input.Down( InputButton.Attack1 ) );

			if ( CanPrimaryAttack() )
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary();
			}

			SetWantsToShoot( false );

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

		protected float ConvertRPM( int rpm )
		{
			return 60f / rpm;
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
			if ( !Owner.IsValid() ) return false;
			if ( !WishToShoot ) return false;

			if ( (Owner as Player).SinceSprintStopped < 0.2f ) return false;

			var rate = ConvertRPM( WeaponInfo.RPM );
			if ( rate <= 0 ) return true;

			return TimeSincePrimaryAttack > rate;
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
					.UseLagCompensation()
					.UseHitboxes()
					.HitLayer( CollisionLayer.Water, !InWater )
					.HitLayer( CollisionLayer.Debris )
					.Ignore( Owner )
					.Ignore( this )
					.Size( radius )
					.Run();

			yield return tr;
		}

		public BaseViewModel HandsModel;

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( WeaponInfo.ViewModel ) )
				return;

			var viewmodel = new ViewModel
			{
				Position = Position,
				Owner = Owner,
				EnableViewmodelRendering = true,
				WeaponInfo = WeaponInfo
			};

			ViewModelEntity = viewmodel;
			ViewModelEntity.SetModel( WeaponInfo.ViewModel );

			// Bonemerge hands
			if ( WeaponInfo.UseCustomHands && !string.IsNullOrEmpty( WeaponInfo.HandsAsset ) )
			{
				HandsModel = new BaseViewModel();
				HandsModel.Owner = Owner;
				HandsModel.EnableViewmodelRendering = true;
				HandsModel.SetModel( WeaponInfo.HandsAsset );
				HandsModel.SetParent( ViewModelEntity, true );
			}
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
		public virtual Vector3 RecoilOnShot => new Vector3( Rand.Float(-7f, 7f ), 15f, 0 );
		public virtual float RecoilRecoveryScaleFactor => 10f;

		// @Client
		public Vector3 CurrentRecoilAmount { get; set; } = Vector3.Zero;

		[Net, Predicted] public int AmmoClip { get; set; }
		[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
		[Net, Predicted] public bool IsReloading { get; set; }
		[Net, Predicted] public TimeSince TimeSinceDeployed { get; set; }
		[Net, Predicted] protected int BurstCount { get; set; } = 0;

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
			return owner.AmmoCount( WeaponInfo.AmmoType );
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

			AmmoClip = WeaponInfo.ClipSize;
		}

		public override void Reload()
		{
			if ( IsReloading )
				return;

			if ( AmmoClip >= WeaponInfo.ClipSize )
				return;

			TimeSinceReload = 0;

			if ( Owner is Player player )
			{
				if ( player.AmmoCount( WeaponInfo.AmmoType ) <= 0 )
					return;
			}

			IsReloading = true;

			( Owner as AnimEntity ).SetAnimBool( "b_reload", true );

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

			if ( IsReloading && TimeSinceReload > WeaponInfo.ReloadTime )
			{
				OnReloadFinish();
			}
		}

		public virtual void OnReloadFinish()
		{
			IsReloading = false;

			if ( Owner is Player player )
			{
				var ammo = player.TakeAmmo( WeaponInfo.AmmoType, WeaponInfo.ClipSize - AmmoClip );
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

		protected bool CanPrimaryAttackSemi()
		{
			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
		}

		protected bool CanPrimaryAttackBurst()
		{
			if ( !Input.Down( InputButton.Attack1 ) )
			{
				BurstCount = 0;
			}

			if ( BurstCount >= WeaponInfo.BurstAmount )
				return false;

			return base.CanPrimaryAttack();
		}

		public override bool CanPrimaryAttack()
		{
			if ( Owner is Player player )
			{
				if ( player.IsSprinting )
					return false;
			}

			var fireMode = WeaponInfo.DefaultFireMode;
			if ( fireMode == FireMode.Semi )
			{
				return CanPrimaryAttackSemi();
			}
			else if ( fireMode == FireMode.Burst )
			{
				return CanPrimaryAttackBurst();
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

		protected virtual float GetBulletSpread()
		{
			return WeaponInfo.BulletSpread;
		}

		protected virtual float GetBulletForce()
		{
			return 1f;
		}

		protected virtual float GetBulletDamage()
		{
			return WeaponInfo.BulletBaseDamage;
		}

		protected virtual float GetBulletRadius()
		{
			return WeaponInfo.BulletRadius;
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			//
			// If we have no ammo, play a sound
			//
			if ( !TakeAmmo( 1 ) )
			{
				DryFire();
				return;
			}

			//
			// Play the fire sound
			//
			PlaySound( WeaponInfo.FireSound );

			//
			// Tell the clients to play the shoot effects
			//
			ShootEffects();

			//
			// Do recoil
			//
			PerformRecoil();

			//
			// Shoot the bullet
			//
			ShootBullet( GetBulletSpread(), GetBulletForce(), GetBulletDamage(), GetBulletRadius() );

			//
			// Set animation property
			//
			( Owner as AnimEntity ).SetAnimBool( "b_attack", true );

			if ( WeaponInfo.DefaultFireMode == FireMode.Burst )
				BurstCount++;
		}

		[ClientRpc]
		protected virtual void PerformRecoil()
		{
			CurrentRecoilAmount += RecoilOnShot;
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

			if ( IsLocalPawn )
			{
				new Sandbox.ScreenShake.Perlin();
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
			PlaySound( WeaponInfo.DryFireSound );
		}

		public override void CreateHudElements()
		{
			Log.Info( "[Conquest] Crosshair created." );

			CrosshairPanel = new Crosshair();
			CrosshairPanel.Parent = Local.Hud;
			CrosshairPanel.AddClass( ClassInfo.Name );
		}

		public override bool IsUsable()
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
