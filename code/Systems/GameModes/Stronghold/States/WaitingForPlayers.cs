
using Sandbox;

namespace Conquest.Stronghold;

public partial class WaitingForPlayersGameState : GameState
{
	[ConVar.Replicated( "conquest_stronghold_minplayers" )]
	public static int MinPlayers { get; set; } = 2;

	public override bool CanDeploy => false;
	public override string ToString() => "GameState[Stronghold][WaitingForPlayers]";

	public override void Tick( float delta )
	{
		base.Tick( delta );

		if ( Client.All.Count >= MinPlayers )
		{
			GameMode.SetGameState( new StartCountdownGameState() );
		}
	}
}
