using Sandbox;
using System;
using System.Linq;

namespace Conquest
{
	public abstract partial class BasePlayer : Sandbox.Player
	{
		[Net] public TeamSystem.Team Team { get; set; } = TeamSystem.Team.Unassigned;

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
			var spawnpoint = All.OfType<SpawnPoint>()
				.OrderBy( x => Guid.NewGuid() )
				.FirstOrDefault();

			if ( spawnpoint == null )
			{
				Log.Warning( $"Couldn't find spawnpoint for {pawn}!" );
				return;
			}

			pawn.Transform = spawnpoint.Transform;
		}

		public override string ToString()
		{
			return $"Player: {Client?.SteamId}/{Client?.Name}";
		}
	}
}
