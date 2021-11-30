
using Sandbox;

namespace Conquest.Stronghold;

public partial class GameplayGameState : GameState
{
	public override string ToString() => "GameState[Stronghold][Gameplay]";
	
	// When scores hit zero, set the winner.
	public override void OnScoreHitZero( Team winner )
	{
		var winnerState = new WinnerDecidedGameState();
		winnerState.WinningTeam = winner;

		GameMode.SetGameState( winnerState );
	}

	public override void OnStart( Conquest.GameState oldGameState = null )
	{
		base.OnStart( oldGameState );

		if ( Host.IsClient )
			ChatBox.AddInformation( $"Stronghold has started. First team to hit zero loses." );
	}
}
