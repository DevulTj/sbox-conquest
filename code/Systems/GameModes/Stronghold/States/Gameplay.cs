
namespace Conquest.Stronghold;

public partial class GameplayGameState : GameState
{
	public override string ToString() => "GameState[Stronghold][Gameplay]";
	
	// When scores hit zero, set the winner.
	[GameEvent.Server.ScoreHitZero]
	protected void ScoreHitZero( Team winner )
	{
		// Events don't care about the game state, so make sure this is the current game state.
		if ( !IsCurrent )
			return;

		var winnerState = new WinnerDecidedGameState();
		winnerState.WinningTeam = winner;

		GameMode.SetGameState( winnerState );
	}
}
