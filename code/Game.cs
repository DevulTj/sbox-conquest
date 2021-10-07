using Sandbox;

namespace Conquest
{
	public partial class Game : Sandbox.Game
	{
		[Net]
		public TeamScores Scores { get; set; }

		public Game()
		{
			Current = this;
			Transmit = TransmitType.Always;

			if ( Host.IsServer )
				Scores = new();
		}

		public override void ClientJoined( Client cl )
		{
			BasePlayer player = new Player( cl );
			cl.Pawn = player;

			player.Respawn();
		}
	}
}
