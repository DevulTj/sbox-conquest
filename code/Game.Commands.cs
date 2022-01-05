using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace Conquest;

public partial class Game 
{
	public static void Deploy( Client cl )
	{
		if ( !GameState.Current.CanDeploy )
		{
			// cannot deploy right now
			return;
		}

		cl.Pawn?.Delete();

		var player = cl.IsBot ? new AIPlayer( cl ) : new Player( cl );
		cl.Pawn = player;
		player.Respawn();
	}

	[ServerCmd( "devcam", Help = "Enables the devcam. Input to the player will stop and you'll be able to freefly around." )]
	public static void DevcamCommand()
	{
		if ( ConsoleSystem.Caller == null ) return;

		(Current as Game)?.DoPlayerDevCam( ConsoleSystem.Caller );
	}

	[ServerCmd( "conquest_deploy" )]
	public static void DeployCommand()
	{
		Deploy( ConsoleSystem.Caller );
	}
}
