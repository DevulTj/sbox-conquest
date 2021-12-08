
using Sandbox;
using System;

namespace Conquest;

public partial class GameState : BaseNetworkable
{
	/// <summary>
	/// Are we the current Game State?
	/// </summary>
	public bool IsCurrent => Current == this;

	/// <summary>
	///  Static Accessor for retrieving the current game state
	/// </summary>
	public static GameState Current => GameModeManager.Current.GameMode.CurrentGameState;

	/// <summary>
	///  Static Accessor for retrieving the last game state
	/// </summary>
	public static GameState Last => GameModeManager.Current.GameMode.LastGameState;
	public GameMode GameMode => GameModeManager.Current.GameMode;

	[Net, Predicted] public TimeSince TimeSinceStart { get; set; } = 0;
	[Net, Predicted] public TimeSince TimeSinceEnd { get; set; } = 0;
	[Net, Predicted] public bool HasTimeLimitReached { get; set; } = false;

	/// <summary>
	/// Decides whether or not players can deploy from the respawn screen.
	/// </summary>
	public virtual bool CanDeploy => true;

	/// <summary>
	/// If set above zero, this will enforce a time limit.
	/// </summary>
	public virtual int TimeLimit => 0;
	public bool HasTimeLimit => TimeLimit > 0;
	public TimeSpan TimeRemaining => TimeSpan.FromSeconds( HasTimeLimit ? TimeLimit - TimeSinceStart : 0 );
	public string FormattedTimeRemaining => TimeRemaining.ToString( @"mm\:ss" );

	/// <summary>
	/// Decides whether or not this state should reset all entities.
	/// </summary>
	public virtual bool ShouldResetEntities => false;

	public virtual string Identifier => ToString();
	/// <summary>
	/// Text to display on UI to represent the game state.
	/// </summary>
	public virtual string DisplayText => ToString();

	public override string ToString() => "GameStateBase";

	public virtual void Tick( float delta )
	{
		if ( !HasTimeLimitReached && TimeLimit != 0 && TimeSinceStart >= TimeLimit )
		{
			OnTimeLimitReached();
		}
	}

	protected virtual void OnTimeLimitReached()
	{
		HasTimeLimitReached = true;
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

	public virtual void OnScoreChanged( Team team, int score )
	{

	}

	public virtual void OnScoreHitZero( Team team )
	{

	}
}
