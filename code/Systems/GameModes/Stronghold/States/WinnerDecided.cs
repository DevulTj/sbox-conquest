
using Sandbox;

namespace Conquest.Stronghold;

public partial class WinnerDecidedGameState : GameState
{
	[Net] public Team WinningTeam { get; set; } = Team.Unassigned;

	public override int TimeLimit => 20;

	public override string ToString() => "GameState[Stronghold][WinnerDecided]";

	public override void OnStart( Conquest.GameState oldGameState )
	{
		base.OnStart( oldGameState );

		if ( Host.IsServer )
			CritPanel.AddInformation( $"GAME OVER. {TeamSystem.GetTeamName( WinningTeam )} WINS" );
	}

	protected override void OnTimeLimitReached()
	{
		base.OnTimeLimitReached();

		GameMode.SetGameState( new WaitingForPlayersGameState() );
	}
}
