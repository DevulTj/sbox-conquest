
using Sandbox;
using System.Linq;

namespace Conquest.Stronghold;

public partial class WaitingForPlayersGameState : GameState
{
	[ConVar.Replicated( "conquest_stronghold_minplayers" )]
	public static int MinPlayers { get; set; } = 2;

	public override bool CanDeploy => false;
	public override string Identifier => "WaitingForPlayers";

	public override string DisplayText => $"Waiting for Players ({Client.All.Count}/{MinPlayers})";

	public override void OnStart( Conquest.GameState oldGameState = null )
	{
		base.OnStart( oldGameState );

		Reset();

		if ( Host.IsClient )
		{
			RespawnScreen.State = TransitionState.ToOverview;
		}
	}

	public override void Tick( float delta )
	{
		base.Tick( delta );

		if ( Client.All.Count >= MinPlayers )
		{
			GameMode.SetGameState( new StartCountdownGameState() );
		}
	}
}
