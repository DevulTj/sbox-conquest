
using Sandbox;

namespace Conquest;

public partial class GameState : BaseNetworkable
{
	/// <summary>
	///  Static Accessor for retrieving the current game state
	/// </summary>
	public static GameState Current => GameModeManager.Current.GameMode.CurrentGameState;

	/// <summary>
	///  Static Accessor for retrieving the last game state
	/// </summary>
	public static GameState Last => GameModeManager.Current.GameMode.LastGameState;

	public virtual bool CanDeploy => true;

	public GameMode GameMode => GameModeManager.Current.GameMode;

	[Net, Predicted] public TimeSince TimeSinceStart { get; set; } = 0;
	[Net, Predicted] public TimeSince TimeSinceEnd { get; set; } = 0;

	public override string ToString() => "GameStateBase";

	public virtual void Tick( float delta )
	{
		// 
	}

	/// <summary>
	/// Called when the GameState has been assigned by the GameMode
	/// </summary>
	/// <param name="oldGameState"></param>
	public virtual void OnStart( GameState oldGameState = null )
	{
		TimeSinceStart = 0;
	}

	/// <summary>
	/// Called when the GameState has been set to something else by the GameMode
	/// </summary>
	/// <param name="newGameState"></param>
	public virtual void OnEnd( GameState newGameState = null )
	{
		TimeSinceEnd = 0;
	}
}
