using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class Game 
	{
		public static void Deploy( Client cl )
		{
			cl.Pawn?.Delete();

			var player = new Player( cl );
			cl.Pawn = player;
			player.Respawn();
		}

		[ServerCmd( "conquest_deploy" )]
		public static void DeployCommand()
		{
			Deploy( ConsoleSystem.Caller );
		}
	}
}
