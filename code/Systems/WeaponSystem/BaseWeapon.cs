﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Conquest
{
	[Library("winfo"), AutoGenerate]
	public class WeaponInfoAsset : Asset
	{
		public static Dictionary<string, WeaponInfoAsset> Registry { get; set; } = new();

		[Property, Category("Important")] public string WeaponClass { get; set; } = "";


		[Property, Category( "ViewModel" )] public Vector3 WalkCycleOffsets { get; set; } = new Vector3( 50f, 20f, 50f );
		[Property, Category( "ViewModel" )] public float ForwardBobbing { get; set; } = 4f;
		[Property, Category( "ViewModel" )] public float SideWalkOffset { get; set; } = 100f;
		[Property, Category( "ViewModel" )] public Vector3 AimOffset { get; set; } = new Vector3( 10f, 10, 1.8f );
		[Property, Category( "ViewModel" )] public Angles AimAngleOffset { get; set; } = new Angles( 0f, 0f, 0f );

		[Property, Category( "ViewModel" )] public Vector3 Offset { get; set; } = new Vector3( -6f, 5f, -5f );
		[Property, Category( "ViewModel" )] public Vector3 CrouchOffset { get; set; } = new Vector3( -10f, -50f, -0f );
		[Property, Category( "ViewModel" )] public float OffsetLerpAmount { get; set; } = 30f;

		[Property, Category( "ViewModel" )] public float SprintRightRotation { get; set; } = 20f;
		[Property, Category( "ViewModel" )] public float SprintUpRotation { get; set; } = -30f;
		[Property, Category( "ViewModel" )] public float SprintLeftOffset { get; set; } = -35f;
		[Property, Category( "ViewModel" )] public float PostSprintLeftOffset { get; set; } = 5f;

		[Property, Category( "ViewModel" )] public float BurstSprintRightRotation { get; set; } = 20f;
		[Property, Category( "ViewModel" )] public float BurstSprintUpRotation { get; set; } = -30f;
		[Property, Category( "ViewModel" )] public float BurstSprintLeftOffset { get; set; } = -35f;
		[Property, Category( "ViewModel" )] public float BurstPostSprintLeftOffset { get; set; } = 5f;

		protected override void PostLoad()
		{
			base.PostLoad();

			Log.Info( "[Conquest] loading weapon info" );

			if ( string.IsNullOrEmpty( WeaponClass ) )
			{
				return;
			}

			var libraryAttribute = Library.GetAttribute( WeaponClass );
			if ( libraryAttribute is not null )
			{
				Registry[WeaponClass] = this;
			}
		}
	}

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
		public virtual Angles AimAngleOffset => new Angles( 0f, 0f, 0f );

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

	public class MR96ViewModelInfo : ViewModelInfo
	{
		public MR96ViewModelInfo( Carriable weaponRef ) : base( weaponRef )
		{
		}

		public override Vector3 Offset => new Vector3( 0f, 5f, -2f );
		public override Vector3 CrouchOffset => new Vector3( 0f, -20f, -2f );

		public override float BurstSprintRightRotation => 2f;
		public override float BurstSprintUpRotation => 2f;
		public override float BurstSprintLeftOffset => -35f;
		public override float BurstPostSprintLeftOffset => 5f;

		public override Vector3 AimOffset => new Vector3( -5f, 10.15f, 2.2f );
		public override Angles AimAngleOffset => new Angles( 1f, 1.5f, -1f );
	}

	public class AK47ViewModelInfo : ViewModelInfo
	{
		public AK47ViewModelInfo( Carriable weaponRef ) : base( weaponRef )
		{
		}

		public override float SprintRightRotation => 2f;
		public override float SprintUpRotation => 2f;

		public override float BurstSprintRightRotation => 20f;
		public override float BurstSprintUpRotation => -30f;
		public override float BurstSprintLeftOffset => -35f;
		public override float BurstPostSprintLeftOffset => 5f;

		public override Vector3 AimOffset => new Vector3( -20f, 18.48f, 2.7f );
	}

	public class M4A1ViewModelInfo : ViewModelInfo
	{
		public M4A1ViewModelInfo( Carriable weaponRef ) : base( weaponRef )
		{
		}

		public override float SprintRightRotation => 2f;
		public override float SprintUpRotation => 2f;
		public override float BurstSprintRightRotation => 20f;
		public override float BurstSprintUpRotation => -30f;
		public override float BurstSprintLeftOffset => -35f;
		public override float BurstPostSprintLeftOffset => 10f;

		public override Vector3 AimOffset => new Vector3( -5f, 19f, 2.7f );
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
		public virtual string HandsModelPath => "weapons/swb/hands/rebel/v_hands_rebel.vmdl";

		public virtual WeaponSlot Slot => WeaponSlot.Primary;

		public virtual bool ShowAmmoCount => true;

		public virtual int Bucket => 1;
		public virtual int BucketWeight => 100;

		public virtual float PrimaryRate => 5.0f;
		public virtual float SecondaryRate => 15.0f;

		public virtual ViewModelInfo VMInfo => new ViewModelInfo( this );

		public WeaponInfoAsset WeaponInfo { get; set; }

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Log.Info( this.ClassInfo.Name );

			foreach( var registryAsset in WeaponInfoAsset.Registry )
			{
				Log.Info( "inlist:" );
				Log.Info( registryAsset );
			}

			Log.Info( "list over" );


			WeaponInfoAsset.Registry.TryGetValue( this.ClassInfo.Name, out var asset );

			Log.Info( asset );

			if ( asset is not null )
				WeaponInfo = asset;
			else
				WeaponInfo = new WeaponInfoAsset();
		}

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
					.UseLagCompensation()
					.UseHitboxes()
					.HitLayer( CollisionLayer.Water, !InWater )
					.HitLayer( CollisionLayer.Debris )
					.Ignore( Owner )
					.Ignore( this )
					.Size( radius )
					.Run();

			yield return tr;

			//
			// Another trace, bullet going through thin material, penetrating water surface?
			//
		}

		public BaseViewModel HandsModel;

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
				WeaponInfo = WeaponInfo
			};

			ViewModelEntity = viewmodel;
			ViewModelEntity.SetModel( ViewModelPath );

			// Bonemerge hands
			if ( !string.IsNullOrEmpty( HandsModelPath ) )
			{
				HandsModel = new BaseViewModel();
				HandsModel.Owner = Owner;
				HandsModel.EnableViewmodelRendering = true;
				HandsModel.SetModel( HandsModelPath );
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
		public virtual AmmoType AmmoType => AmmoType.Pistol;
		public virtual int ClipSize => 16;
		public virtual float ReloadTime => 3.0f;

		public virtual Vector3 RecoilOnShot => new Vector3( Rand.Float(-7f, 7f ), 15f, 0 );
		public virtual float RecoilRecoveryScaleFactor => 10f;

		// client driven
		public Vector3 CurrentRecoilAmount { get; set; } = Vector3.Zero;

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
			// Do recoil
			//
			PerformRecoil();

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
			// CLICK
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
