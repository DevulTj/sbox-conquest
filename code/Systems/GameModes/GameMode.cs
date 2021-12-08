
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest;

public partial class GameMode : BaseNetworkable
{
	public override string ToString() => "GameMode";

	[Net] public GameState LastGameState { get; private set; }
	[Net] public GameState CurrentGameState { get; private set; }

	/// <summary>
	/// Used to decide order of game states
	/// </summary>
	public virtual GameState DefaultGameState { get; set; } = null;

	public virtual void Tick( float delta )
	{
		CurrentGameState.Tick( delta );
	}

	/// <summary>
	/// Method to set the game state.
	/// </summary>
	/// <param name="newGameState"></param>
	public virtual void SetGameState( GameState newGameState )
	{
		// Let the current gamestate know it's time is over
		CurrentGameState?.OnEnd( newGameState );
		LastGameState = CurrentGameState;
		CurrentGameState = newGameState;
		// Let the new gamestate know it's time has just begun
		CurrentGameState?.OnStart( newGameState );
	}

	public virtual string Print()
	{
		return $"Current GameState: {CurrentGameState}\n" +
			$"Last GameState: {(LastGameState != null ? LastGameState : "NULL")}\n" +
			$"Current Identifier: {CurrentGameState?.Identifier}";
	}

	/// <summary>
	/// Called when the GameMode has initialized.
	/// </summary>
	public virtual void Initialize()
	{
		// Set the Game State to the first one in our list.
		SetGameState( DefaultGameState );
	}

	public virtual void OnScoreChanged( Team team, int score )
	{
		CurrentGameState?.OnScoreChanged( team, score );

		if ( score == 0 )
		{
			OnScoreHitZero( TeamScores.GetOpposingTeam( team ) );
		}
	}

	public virtual void OnScoreHitZero( Team team )
	{
		CurrentGameState?.OnScoreHitZero( team );
	}
}
