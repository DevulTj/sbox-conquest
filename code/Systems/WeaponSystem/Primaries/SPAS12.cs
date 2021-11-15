using Sandbox;
using System;

namespace Conquest
{
	[Library( "conquest_spas12", Title = "SPAS-12" )]
	[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
	partial class SPAS12 : BaseWeapon
	{
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/swb/rifles/fal/w_fal.vmdl" );
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
			PlaySound( "fal.fire" );

			//
			// Shoot the bullets
			//
			Rand.SetSeed( Time.Tick );
			ShootBullet( 0f, 1.5f, 15f, 3.0f );

		}

		public override void AttackSecondary()
		{
			// Grenade lob
		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/swb/muzzle/flash_medium.vpcf", EffectEntity, "muzzle" );
			Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

			if ( Owner == Local.Pawn )
			{
				new Sandbox.ScreenShake.Perlin( 0.1f, 1f, 0.5f, 0.8f );
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
