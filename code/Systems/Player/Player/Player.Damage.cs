using Sandbox;
using System;
using System.Linq;
using Sandbox.UI;

namespace Conquest;

partial class Player
{
	public float MaxHealth => 100f;
	public float RegenerateTime => 5f;
	public float RegenerateSpeed => 10f;

	protected DamageInfo LastDamage;
	protected TimeSince SinceTakenDamage;
	public override void TakeDamage( DamageInfo info )
	{
		LastDamage = info;
		SinceTakenDamage = 0;

		bool isHeadshot = info.HitboxIndex == 5;
		// hack - hitbox 0 is head
		// we should be able to get this from somewhere
		if ( isHeadshot )
		{
			info.Damage *= 2.0f;

			Sound.FromEntity( "darkrp.headshot", this );
			Sound.FromEntity( "darkrp.headshot", info.Attacker );
		}

		base.TakeDamage( info );

		if ( info.Attacker is Player attacker && attacker != this )
		{
			// Note - sending this only to the attacker!
			attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ), isHeadshot );

			TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
		}
	}

	public void Kill()
	{
		Health = 0.0f;
		OnKilled();
	}

	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv, bool isHeadshot = false )
	{
		Sound.FromScreen( "dm.ui_attacker" )
			.SetPitch( 1 + healthinv * 1 );

		HitIndicator.Current?.OnHit( pos, amount, healthinv == 1, isHeadshot );

		// If it's a kill
		if ( isHeadshot & healthinv == 1 )
		{
			Sound.FromScreen( "conquest.headshot_kill" );
		}
	}

	[ClientRpc]
	public void TookDamage( Vector3 pos )
	{
		//DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

		HurtIndicator.Current?.OnHit( pos );
	}

	[ConCmd.Admin( "conquest_sethp" )]
	public static void SetHealth( float amt )
	{
		if ( ConsoleSystem.Caller.Pawn is Player player )
		{
			player.Health = amt;
		}
	}

	protected void SimulateDamage()
	{
		if ( Health <= MaxHealth && SinceTakenDamage >= RegenerateTime )
		{
			Health += RegenerateSpeed * Time.Delta;
			Health = Health.Clamp( 0, MaxHealth );
		}
	}
}
