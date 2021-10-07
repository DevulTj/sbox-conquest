using Sandbox;
using System;
using System.Linq;
using Sandbox.UI;

namespace Conquest
{
	partial class Player
	{
		protected DamageInfo LastDamage;
		public override void TakeDamage( DamageInfo info )
		{
			LastDamage = info;

			// hack - hitbox 0 is head
			// we should be able to get this from somewhere
			if ( info.HitboxIndex == 5 )
			{
				info.Damage *= 2.0f;

				Sound.FromEntity( "darkrp.headshot", this );
				Sound.FromEntity( "darkrp.headshot", info.Attacker );
			}

			base.TakeDamage( info );

			if ( info.Attacker is Player attacker && attacker != this )
			{
				// Note - sending this only to the attacker!
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ) );

				TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
			}
		}

		public void Kill()
		{
			Health = 0.0f;
			OnKilled();
		}

		[ClientRpc]
		public void DidDamage( Vector3 pos, float amount, float healthinv )
		{
			Sound.FromScreen( "dm.ui_attacker" )
				.SetPitch( 1 + healthinv * 1 );

			//	HitIndicator.Current?.OnHit( pos, amount );
		}

		[ClientRpc]
		public void TookDamage( Vector3 pos )
		{
			//DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

			// DamageIndicator.Current?.OnHit( pos );
		}
	}
}
