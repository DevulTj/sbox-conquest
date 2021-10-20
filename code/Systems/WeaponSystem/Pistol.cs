using Sandbox;

namespace Conquest
{
	[Library( "conquest_pistol", Title = "MR-96" )]
	[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
	partial class Pistol : BaseWeapon
	{
		public override WeaponSlot Slot => WeaponSlot.Secondary; 
		public override string ViewModelPath => "weapons/magnum/v_magnum.vmdl";

		public override float PrimaryRate => 2.0f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int ClipSize => 6;
		public override int Bucket => 2;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/magnum/magnum.vmdl" );
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
			PlaySound( "magnum.shoot" );

			//
			// Shoot the bullets
			//
			//Rand.SetSeed( Time.Tick );
			ShootBullet( 0.2f, 1.5f, 45f, 3.0f );

		}
	}
}
