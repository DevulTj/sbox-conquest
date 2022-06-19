﻿
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest;

public enum Team
{
	BLUFOR,
	OPFOR,
	Unassigned
}

public static class TeamExtensions
{
	public static To NetClients( this Team team )
	{
		return To.Multiple( team.AllClients().Select( x => x ) );
	}

	public static int Count( this Team team )
	{
		return AllClients( team ).Count();
	}

	public static IEnumerable<Client> AllClients( this Team team )
	{
		return Client.All.Where( x => TeamSystem.GetTeam( x ) == team );
	}

	public static IEnumerable<Player> AllPlayers( this Team team )
	{
		return AllClients( team ).Select( x => x.Pawn as Player );
	}
	public static Team GetLowestCount( this Team team )
	{
		var bluforCount = Count( Team.BLUFOR );
		var opforCount = Count( Team.OPFOR );

		if ( opforCount < bluforCount )
			return Team.OPFOR;

		return Team.BLUFOR;
	}
}

public static class TeamSystem
{
	public static T ToEnum<T>( this string enumString )
	{
		return (T) Enum.Parse( typeof( T ), enumString );
	}

	public static Team MyTeam => Local.Client.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;

	public enum FriendlyStatus
	{
		Friendly,
		Hostile,
		Neutral
	}

	public static FriendlyStatus GetFriendState( Team one, Team two )
	{
		if ( one == Team.Unassigned || two == Team.Unassigned )
			return FriendlyStatus.Neutral;

		if ( one != two )
			return FriendlyStatus.Hostile;

		return FriendlyStatus.Friendly;
	}

	public static bool IsFriendly( Team one, Team two )
	{
		return GetFriendState( one, two ) == FriendlyStatus.Friendly;
	}

	public static bool IsHostile( Team one, Team two )
	{
		return GetFriendState( one, two ) == FriendlyStatus.Hostile;
	}

	public static Team GetEnemyTeam( Team team )
	{
		switch( team )
		{
			case Team.BLUFOR:
				return Team.OPFOR;
			case Team.OPFOR:
				return Team.BLUFOR;
		}

		return Team.Unassigned;
	}

	public static string GetTeamName( Team team )
	{
		switch( team )
		{
			case Team.BLUFOR:
				return "US";
			case Team.OPFOR:
				return "RU";
		}

		return "NOBODY";
	}

	public static Team GetTeam( Client cl )
	{
		return cl.Components.Get<TeamComponent>()?.Team ?? Team.Unassigned;
	}

	[ConCmd.Server( "conquest_jointeam" )]
	public static void JoinTeam( string name )
	{
		var player = ConsoleSystem.Caller.Pawn as Player;
		var team = name.ToEnum<Team>();

		player.Team = team;
	}
}
