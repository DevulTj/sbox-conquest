
namespace Conquest.Stronghold;

public partial class GameMode : Conquest.GameMode
{
	public override string ToString() => "GameMode[Stronghold]";
	// @Server
	public override Conquest.GameState DefaultGameState => new WaitingForPlayersGameState();

	public override BaseHud Hud => new PlayerHud();
}
