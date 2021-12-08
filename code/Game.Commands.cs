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

	[ServerCmd( "conquest_deploy" )]
	public static void DeployCommand()
	{
		Deploy( ConsoleSystem.Caller );

		Current.ClientDeploy( To.Single( ConsoleSystem.Caller ) );
	}

	[ClientRpc]
	protected void ClientDeploy()
	{
		RespawnScreen.State = TransitionState.FromOverview;
	}
}
