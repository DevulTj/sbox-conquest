using Sandbox;

namespace Conquest
{
	public partial class Game : Sandbox.Game
	{
		public Game()
		{
			Current = this;
			Transmit = TransmitType.Always;
		}

		public override void ClientJoined( Client cl )
		{
			BasePlayer player = new Player( cl );
			cl.Pawn = player;

			player.Respawn();
		}
	}
}
