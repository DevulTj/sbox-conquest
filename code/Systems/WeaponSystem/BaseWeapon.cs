using Conquest.ExtensionMethods;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Conquest;

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
	Melee,
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

		if ( WeaponInfo is null )
			return;
			
		
		if ( WeaponInfo.CachedWorldModel is not null && !WeaponInfo.CachedWorldModel.IsError )
			Model = WeaponInfo.CachedWorldModel;
	}

	[Net, Predicted]
	public TimeSince TimeSincePrimaryAttack { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceSecondaryAttack { get; set; }


	[Net, Predicted]
	bool WishToShoot { get; set; } = false;

	protected virtual float MaxAmtOfHits => WeaponInfo.SurfacePassthroughAmount;
	protected virtual float MaxRicochetAngle => 45f;
	protected float MaxPenetration => 20f;

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

		if ( WeaponInfo is not null )
		{
			var rate = ConvertRPM( WeaponInfo.RPM );
			if ( rate <= 0 ) return true;

			return TimeSincePrimaryAttack > rate;
		}

		return true;
	}

	public virtual void AttackPrimary()
	{
	}

	public virtual bool CanSecondaryAttack()
	{
		if ( !Owner.IsValid() ) return false;

		var rate = SecondaryRate;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public virtual void AttackSecondary()
	{
	}

	protected virtual bool ShouldContinue( TraceResult tr, float angle = 0f )
	{
		float maxAngle = MaxRicochetAngle;

		if ( angle > maxAngle )
			return false;

		return true;
	}

	protected virtual Vector3 CalculateDirection( TraceResult tr, ref float hits )
	{
		if ( tr.Entity is GlassShard )
		{
			// Allow us to do another hit
			hits--;
			return tr.Direction;
		}

		return Vector3.Reflect( tr.Direction, tr.Normal ).Normal;
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		float currentAmountOfHits = 0;
		Vector3 _start = start;
		Vector3 _end = end;
		List<TraceResult> hits = new();

		Entity lastEnt = null;
		while ( currentAmountOfHits < MaxAmtOfHits )
		{
			currentAmountOfHits++;

			bool inWater = Map.Physics.IsPointWater( _start );
			var tr = Trace.Ray( _start, _end )
			.UseHitboxes()
			.HitLayer( CollisionLayer.Water, !inWater )
			.HitLayer( CollisionLayer.Debris )
			.Ignore( this )
			.Ignore( lastEnt.IsValid() ? lastEnt : this, false )
			.Size( radius )
			.Run();

			lastEnt = tr.Entity;

			if ( tr.Hit )
				hits.Add( tr );

			if ( tr.Entity is GlassShard )
			{
				_start = tr.EndPos;
				_end = tr.EndPos + (tr.Direction * 5000);
			}
			else if ( tr.Entity.IsValid() && tr.Entity.Tags.Has( "flyby" ) )
			{
				_start = tr.EndPos;
				_end = tr.EndPos + (tr.Direction * 5000);
			}
			else
			{
				var reflectDir = CalculateDirection( tr, ref currentAmountOfHits );
				var angle = reflectDir.Angle( tr.Direction );

				_start = tr.EndPos;
				_end = tr.EndPos + (reflectDir * 5000);

				if ( !ShouldContinue( tr, angle ) )
				{
					break;
				}
			}
		}

		return hits;
	}

	public BaseViewModel HandsModel;

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( WeaponInfo is null )
			return;

		if ( WeaponInfo.CachedViewModel is null || WeaponInfo.CachedViewModel.IsError )
			return;

		var viewmodel = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true,
			WeaponInfo = WeaponInfo
		};

		ViewModelEntity = viewmodel;
		ViewModelEntity.Model = WeaponInfo.CachedViewModel;

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

	public override void DestroyViewModel()
	{
		if ( ViewModelEntity.IsValid() )
		{
			ViewModelEntity.Delete();
		}

		if ( HandsModel.IsValid() )
		{
			HandsModel.Delete();
		}
	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );

		if ( ViewModelEntity.IsValid() )
		{
			DestroyViewModel();
			DestroyHudElements();
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

public partial class BaseWeapon : Carriable, IGameStateAddressable
{
	public virtual Vector3 RecoilOnShot => new Vector3( Rand.Float(-7f, 7f ), 15f, 0 );
	public virtual float RecoilRecoveryScaleFactor => 10f;

	// @Client
	public Vector3 CurrentRecoilAmount { get; set; } = Vector3.Zero;

	public int MaxAmmoClip => WeaponInfo.ClipSize;

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

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.AutoSleep = false;

		AmmoClip = WeaponInfo.ClipSize;
	}

	public override void Reload()
	{
		if ( AmmoClip >= WeaponInfo.ClipSize )
			return;

		TimeSinceReload = 0;

		if ( Owner is Player player )
		{
			if ( player.AmmoCount( WeaponInfo.AmmoType ) <= 0 )
				return;
		}

		IsReloading = true;
		WantsToStopReloading = false;

		( Owner as AnimEntity ).SetAnimParameter( "b_reload", true );

		StartReloadEffects();
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		base.SimulateAnimator( anim );

		anim.SetAnimParameter( "holdtype", (int)WeaponInfo.HoldType );
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			base.Simulate( owner );
		}
		if ( IsReloading && ( Input.Down( InputButton.Attack1 ) || Input.Down( InputButton.Attack2 ) ) )
		{
			StopReload();
		}

		if ( IsReloading && TimeSinceReload > WeaponInfo.ReloadTime )
		{
			OnReloadFinish();
		}
	}

	[ClientRpc]
	protected void SendReloadFinished()
	{
		ViewModelEntity?.SetAnimParameter( "reload_finished", true );
	}

	bool WantsToStopReloading = false;
	protected void StopReload()
	{
		WantsToStopReloading = true;
	}

	public virtual void OnReloadFinish()
	{
		if ( Owner is Player player )
		{
			var amountToTake = WeaponInfo.ReloadSingle ? 1 : WeaponInfo.ClipSize - AmmoClip;

			var ammo = player.TakeAmmo( WeaponInfo.AmmoType, amountToTake );
			if ( ammo == 0 )
			{
				IsReloading = false;
				SendReloadFinished();
				WantsToStopReloading = false;
				return;
			}

			AmmoClip += ammo;

			if ( !WantsToStopReloading && WeaponInfo.ReloadSingle && AmmoClip < WeaponInfo.ClipSize )
			{
				Reload();
			}
			else
			{
				IsReloading = false;
				WantsToStopReloading = false;
				SendReloadFinished();
			}
		}
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
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

	public virtual float GetAddedSpread()
	{
		var radius = 0f;
		var player = Owner as Player;

		if ( !player.GroundEntity.IsValid() )
			radius = 0.3f;

		if ( !player.IsAiming )
			radius = 0.15f;

		return radius;
	}

	[ClientRpc]
	protected void SendTracer( int i, Vector3 start, Vector3 end )
	{
		var startPos = start;
		if ( Owner == Local.Pawn && i == 0 )
		{
			ModelEntity firingViewModel = ViewModelEntity;

			if ( firingViewModel.IsValid() )
			{

				var muzzleAttach = firingViewModel.GetAttachment( "muzzle" );
				startPos = muzzleAttach.GetValueOrDefault().Position;
			}
		}
	
		var tracer = Particles.Create( "particles/swb/tracer/tracer_large.vpcf" );
		tracer.SetPosition( 1, startPos );
		tracer.SetPosition( 2, end );
	}


	[ClientRpc]
	public static void PlayFlybySound( string sound )
	{
		Sound.FromScreen( sound );
	}

	public static void PlayFlybySounds( Entity attacker, Entity victim, Vector3 start, Vector3 end, float minDistance, float maxDistance, string sound = $"bullet.flyby" )
	{
		foreach ( var client in Client.All )
		{
			var pawn = client.Pawn;

			if ( !pawn.IsValid() || pawn == attacker )
				continue;

			if ( pawn.LifeState != LifeState.Alive )
				continue;

			var distance = pawn.Position.DistanceToLine( start, end, out var _ );

			if ( distance >= minDistance && distance <= maxDistance )
			{
				PlayFlybySound( To.Single( client ), sound );
			}
		}
	}

	public static void PlayFlybySounds( Entity attacker, Vector3 start, Vector3 end, float minDistance, float maxDistance, string sound = $"bullet.flyby" )
	{
		PlayFlybySounds( attacker, null, start, end, minDistance, maxDistance, sound );
	}

	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1, float bulletRange = 5000f )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Rand.SetSeed( Time.Tick );

		spread += GetAddedSpread();

		for ( int i = 0; i < bulletCount; i++ )
		{
			var forward = Owner.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//

			int count = 0;
			foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * bulletRange, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				PlayFlybySounds( Owner, tr.Entity, tr.StartPos, tr.EndPos, bulletSize * 2f, bulletSize * 50f );

				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );

				if ( WeaponInfo.Slot != WeaponSlot.Melee )
					SendTracer( count++, tr.StartPos, tr.EndPos );
			}
		}
	}

	protected virtual float GetBulletSpread()
	{
		return WeaponInfo.BulletSpread;
	}

	protected virtual float GetBulletForce()
	{
		return 10f;
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

		if ( Slot != WeaponSlot.Melee )
		{
			if ( !TakeAmmo( 1 ) )
			{
				DryFire();
				return;
			}
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
		// Shoot some bullets
		//
		ShootBullet( GetBulletSpread(), GetBulletForce(), GetBulletDamage(), GetBulletRadius(), Math.Clamp( WeaponInfo.Pellets, 1, 16 ), WeaponInfo.BulletRange );

		//
		// Set animation property
		//
		( Owner as AnimEntity ).SetAnimParameter( "b_attack", true );

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

		if ( !string.IsNullOrEmpty( WeaponInfo.MuzzleFlashParticle ) )
			Particles.Create( WeaponInfo.MuzzleFlashParticle, EffectEntity, "muzzle" );

		if ( !string.IsNullOrEmpty( WeaponInfo.EjectParticle ) )
			Particles.Create( WeaponInfo.EjectParticle, EffectEntity, "ejection_point" );

		WeaponInfo.ScreenShake.Run();

		ViewModelEntity?.SetAnimParameter( WeaponInfo.AttackAnimBool, true );
		CrosshairPanel?.CreateEvent( WeaponInfo.AttackAnimBool );
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
	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );
	}

	void IGameStateAddressable.ResetState()
	{
		if ( Parent.IsValid() )
			return;

		Delete();
	}
}
