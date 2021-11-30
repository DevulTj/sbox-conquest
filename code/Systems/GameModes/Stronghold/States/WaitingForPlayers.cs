
namespace Conquest.Stronghold;

public partial class WaitingForPlayersGameState : GameState
{
	public override bool CanDeploy => false;
	public override string ToString() => "GameState[Stronghold][WaitingForPlayers]";
}
