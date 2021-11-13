using Sandbox;
using System;
using System.Linq;

namespace Conquest
{
	public abstract partial class BasePlayer : Sandbox.Player
	{
		public Team Team
		{
			get
			{
				var cl = Client;
				if ( cl is null ) return Team.Unassigned;

				return cl.Components.GetOrCreate<TeamComponent>().Team;
			}
			set
			{
				var cl = Client;
				if ( cl is null ) return;

				cl.Components.GetOrCreate<TeamComponent>().Team = value;
			}
		}

		public BaseHud Hud { get; set; }

		protected abstract void MakeHud();

		public BasePlayer()
		{
			Tags.Add( "player" );
		}

		public override void Respawn()
		{
			base.Respawn();

			// Init hud on client owner
			InitializeLocalHud( To.Single( Client ) );
		}

		[ClientRpc]
		protected void InitializeLocalHud()
		{
			MakeHud();

			if ( Hud != null )
			{
				Local.Hud?.Delete();
				Local.Hud = Hud;

				OnHudCreated();
			}
			else
			{
				Log.Warning( "No hud found for this player type" );
			}
		}

		protected virtual void OnHudCreated() { }

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			SetupPhysics();
		}
		
		public virtual void SetupPhysics() { }

		public static void MoveToSpawnpoint( Entity pawn )
		{
			var team = TeamSystem.GetTeam( pawn.Client );

			var spawnpoint = All.OfType<Headquarters>().Where( x => x.Team == team ).FirstOrDefault() as Entity;

			if ( spawnpoint is null )
			{
				// Revert to default
				spawnpoint = All.OfType<SpawnPoint>()
					.OrderBy( x => Guid.NewGuid() )
					.FirstOrDefault();
			}

			// Seriously, still?
			if ( spawnpoint is null )
			{
				Log.Warning( $"Couldn't find spawnpoint for {pawn}!" );
				return;
			}

			var transform = spawnpoint.Transform;
			transform.Position += new Vector3( Rand.Float( 100 ), Rand.Float( 100 ), 0 );

			pawn.Transform = transform;
		}

		public override string ToString()
		{
			return $"Player: {Client?.PlayerId}/{Client?.Name}";
		}
	}
}
