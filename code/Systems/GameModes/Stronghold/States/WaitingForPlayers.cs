
using Sandbox;
using System.Linq;

namespace Conquest.Stronghold;

public partial class WaitingForPlayersGameState : GameState
{
	[ConVar.Replicated( "conquest_stronghold_minplayers" )]
	public static int MinPlayers { get; set; } = 2;

	public override bool CanDeploy => false;
	public override string Identifier => "WaitingForPlayers";

	public override void OnStart( Conquest.GameState oldGameState = null )
	{
		base.OnStart( oldGameState );

		if ( Host.IsServer )
		{
			Game.Current?.Scores.Reset();

			var ents = Entity.All.OfType<IGameStateAddressable>().ToList();
			foreach ( var entity in ents )
			{
				entity.ResetState();
			}
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
