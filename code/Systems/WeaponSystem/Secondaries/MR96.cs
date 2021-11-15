using Sandbox;

namespace Conquest
{
	[Library( "conquest_mr96", Title = "MR-96" )]
	[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
	partial class MR96 : BaseWeapon
	{
		public override WeaponSlot Slot => WeaponSlot.Secondary; 
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override float PrimaryRate => 2.0f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int ClipSize => 6;
		public override int Bucket => 2;

		public override Vector3 RecoilOnShot => new Vector3( Rand.Float( -40f, 40f ), 100f, 0 );
		public override float RecoilRecoveryScaleFactor => base.RecoilRecoveryScaleFactor * 2f;

		public override ViewModelInfo VMInfo => new MR96ViewModelInfo( this );

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
			AmmoClip = 6;
		}

		public override bool CanPrimaryAttack()
		{
			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
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

			//
			// Tell the clients to play the shoot effects
			//
			ShootEffects();
			PerformRecoil();
			PlaySound( "magnum.shoot" );

			//
			// Shoot the bullets
			//
			//Rand.SetSeed( Time.Tick );
			ShootBullet( 0f, 1.5f, 55f, 3.0f );
		}
	}
}
