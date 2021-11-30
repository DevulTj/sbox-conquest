
using Sandbox;
using System.Collections.Generic;

namespace Conquest.Stronghold;

public partial class GameMode : Conquest.GameMode
{
	public override string ToString() => "GameMode[Stronghold]";
	// @Server
	public override List<Conquest.GameState> OrderedGameStates { get; set; } = new()
	{
		new WaitingForPlayersGameState(),
		new StartCountdownGameState(),
		new GameplayGameState(),
		new WinnerDecidedGameState(),
		new ShowcaseGameState()
	};
}
