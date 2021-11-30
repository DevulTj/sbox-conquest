
using Sandbox;

namespace Conquest.Stronghold;

public partial class WinnerDecidedGameState : GameState
{
	[Net] public Team WinningTeam { get; set; } = Team.Unassigned;

	public override string ToString() => "GameState[Stronghold][WinnerDecided]";

}
