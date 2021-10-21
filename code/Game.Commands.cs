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

			ChatBox.AddInformation( To.Everyone, $"{cl.Name} spawned.", $"avatar:{cl.SteamId}" );

			player.Respawn();
		}

		[ServerCmd( "conquest_deploy" )]
		public static void DeployCommand()
		{
			Deploy( ConsoleSystem.Caller );
		}
	}
}
