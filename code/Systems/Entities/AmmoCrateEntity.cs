using Sandbox;

namespace Conquest
{
	[Library( "conquest_ammocrate" )]
	public partial class AmmoCrateEntity : Prop, IUse, IHudMarkerEntity
	{
		[AdminCmd( "conquest_debug_ammocrate" )]
		public static void CreateAmmoCrate()
		{
			var caller = ConsoleSystem.Caller.Pawn;
			var entity = new AmmoCrateEntity();

			var tr = Trace.Ray( caller.EyePos, caller.EyeRot.Forward * 1000f )
				.WorldAndEntities()
				.Ignore( caller )
				.Run();

			entity.Position = tr.EndPos;
		}

		[Net] public TimeSince LastUsedTime { get; set; }

		public int AmountToGive => 30;
		public float UseCooldown => 1;

		public override void Spawn()
		{
			base.Spawn();

			Health = 100;
			Transmit = TransmitType.Default;

			SetModel( "models/citizen_props/crate01.vmdl" );
		}

		public bool OnUse( Entity user )
		{
			if ( user is Player player )
			{
				player.GiveAll( 120 );
				ClientUsed( To.Single( player.Client ) );

				LastUsedTime = 0;
			}

			return false;
		}

		public bool IsUsable( Entity user )
		{
			return LastUsedTime >= UseCooldown;
		}

		[ClientRpc]
		protected void ClientUsed()
		{
			Log.Info( "testing" );
			KillFeedPanel.Current?.AddMessage( "Ammo replenished" );
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );
		}

		string IHudMarkerEntity.GetMainClass()
		{
			return "ammocrate";
		}

		bool CalculateVis()
		{
			var tr = Trace.Ray( CurrentView.Position, EyePos )
				.WorldAndEntities()
				.Ignore( Local.Pawn )
				.Run();

			if ( tr.Distance > 768 ) return false;

			if ( tr.Hit && tr.Entity == this )
				return true;
			else
				return false;
		}

		bool IHudMarkerEntity.Update( ref HudMarkerBuilder info )
		{
			if ( !this.IsValid() )
				return false;

			if ( LifeState != LifeState.Alive )
				return false;

			if ( !CalculateVis() )
				return false;

			info.Position = Position + Rotation.Up * 50f;

			return true;	
		}
	}
}
