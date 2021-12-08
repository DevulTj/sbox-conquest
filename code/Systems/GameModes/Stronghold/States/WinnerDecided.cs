
using Sandbox;

namespace Conquest.Stronghold;

public partial class WinnerDecidedGameState : GameState
{
	[Net] public Team WinningTeam { get; set; } = Team.Unassigned;

	public override int TimeLimit => 5;
	public override string Identifier => "WinnerDecided";

	public override string DisplayText => $"{TeamSystem.GetTeamName( WinningTeam )} WINS";

	public override void OnStart( Conquest.GameState oldGameState )
	{
		base.OnStart( oldGameState );

		if ( Host.IsServer )
			CritPanel.AddInformation( $"GAME OVER. {TeamSystem.GetTeamName( WinningTeam )} WINS" );
	}

	protected override void OnTimeLimitReached()
	{
		base.OnTimeLimitReached();

		if ( WinningTeam == Team.Unassigned )
		{
			GameMode.SetGameState( new WaitingForPlayersGameState() );
			return;
		}

		var gameState = new ShowcaseGameState();
		gameState.WinningTeam = WinningTeam;

		GameMode.SetGameState( gameState );
	}
}
