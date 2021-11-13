using Sandbox;
using System;

namespace Conquest
{
	[Library( "conquest_ak47", Title = "AK-47" )]
	[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
	partial class AK47 : BaseWeapon
	{
		public override WeaponSlot Slot => WeaponSlot.Primary;
		public override string ViewModelPath => "weapons/ak47/v_ak47.vmdl";

		public override float PrimaryRate => 12.0f;
		public override float SecondaryRate => 1.0f;
		public override int ClipSize => 30;
		public override float ReloadTime => 2.8f;
		public override int Bucket => 3;
		public override AmmoType AmmoType => AmmoType.Rifle;

		public override ViewModelInfo VMInfo => new SMGViewModelInfo( this );

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/ak47/ak47.vmdl" );
			AmmoClip = 30;
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			if ( !TakeAmmo( 1 ) )
			{
				DryFire();
				return;
			}

			(Owner as AnimEntity).SetAnimBool( "b_attack", true );

			//
			// Tell the clients to play the shoot effects
			//
			ShootEffects();
			PerformRecoil();
			
			PlaySound( "ak47.shoot" );

			//
			// Shoot the bullets
			//
			Rand.SetSeed( Time.Tick );
			ShootBullet( 0.1f, 1.5f, 15f, 3.0f );

		}

		public override void AttackSecondary()
		{
			// Grenade lob
		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
			Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

			if ( Owner == Local.Pawn )
			{
				new Sandbox.ScreenShake.Random( 0.5f, 0.5f, 0.5f );
				//new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
			}

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 ); // TODO this is shit
			anim.SetParam( "aimat_weight", 1.0f );
		}

	}
}
