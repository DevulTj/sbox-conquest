
using Sandbox;

namespace Conquest.Stronghold;

public partial class StartCountdownGameState : GameState
{
	[ConVar.Replicated( "conquest_stronghold_startcountdown" )]
	public static int StartCountdown { get; set; } = 10;

	public override bool CanDeploy => false;
	public override int TimeLimit => StartCountdown;
	public override string ToString() => "GameState[Stronghold][StartCountdown]";

	protected override void OnTimeLimitReached()
	{
		GameMode.SetGameState( new GameplayGameState() );
	}

	public override void OnStart( Conquest.GameState oldGameState = null )
	{
		base.OnStart( oldGameState );

		CritPanel.AddInformation( $"The game will start in {TimeLimit} seconds." );
	}
}
