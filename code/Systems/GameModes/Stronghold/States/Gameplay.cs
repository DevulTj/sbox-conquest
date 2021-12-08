
using Sandbox;

namespace Conquest.Stronghold;

public partial class GameplayGameState : GameState
{
	[ConVar.Replicated( "conquest_stronghold_gamelength" )]
	public static int GameLength { get; set; } = 10;

	public override string Identifier => "Gameplay";
	public override int TimeLimit => GameLength;
	public override string DisplayText => FormattedTimeRemaining;

	// When scores hit zero, set the winner.
	public override void OnScoreHitZero( Team winner )
	{
		var winnerState = new WinnerDecidedGameState();
		winnerState.WinningTeam = winner;

		GameMode.SetGameState( winnerState );
	}

	protected override void OnTimeLimitReached()
	{
		base.OnTimeLimitReached();

		// @TODO: Move scoring system into gamemode
		var winner = Game.Current?.Scores.GetHighestTeam() ?? Team.Unassigned;

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
