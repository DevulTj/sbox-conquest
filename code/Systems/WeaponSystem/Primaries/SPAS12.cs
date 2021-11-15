using Sandbox;
using System;

namespace Conquest
{
	[Library( "conquest_spas12", Title = "SPAS-12" )]
	[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
	partial class SPAS12 : BaseWeapon
	{
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
