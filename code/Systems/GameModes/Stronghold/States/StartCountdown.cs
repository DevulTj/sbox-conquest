
using Sandbox;

namespace Conquest.Stronghold;

public partial class StartCountdownGameState : GameState
{
	[ConVar.Replicated( "conquest_stronghold_startcountdown" )]
	public static int StartCountdown { get; set; } = 10;

	public override string ToString() => "GameState[Stronghold][StartCountdown]";

	public override void Tick( float delta )
	{
		base.Tick( delta );

		if ( TimeSinceStart >= StartCountdown )
		{
			GameMode.SetGameState( new GameplayGameState() );
		}
	}
}
