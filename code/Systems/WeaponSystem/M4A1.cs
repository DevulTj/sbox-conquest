using Sandbox;
using System;

namespace Conquest
{
	[Library( "conquest_m4a1", Title = "M4A1" )]
	[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
	partial class M4A1 : BaseWeapon
	{
		public override WeaponSlot Slot => WeaponSlot.Primary;
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

		public override float PrimaryRate => 15.0f;
		public override float SecondaryRate => 1.0f;
		public override int ClipSize => 30;
		public override float ReloadTime => 2.8f;
		public override int Bucket => 3;
		public override AmmoType AmmoType => AmmoType.Rifle;

		public override ViewModelInfo VMInfo => new M4A1ViewModelInfo( this );

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
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
			PlaySound( "rust_smg.shoot" );

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
				new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
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
