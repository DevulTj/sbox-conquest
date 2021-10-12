
using Sandbox;
using System;

namespace Conquest
{
	public static class TeamSystem
	{
		public static T ToEnum<T>( this string enumString )
		{
			return (T) Enum.Parse( typeof( T ), enumString );
		}

		public static Team MyTeam => ( Local.Pawn as Player )?.Team ?? Team.Unassigned;

		public enum Team
		{
			BLUFOR,
			OPFOR,
			Unassigned
		}

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

			return "--";
		}


		[ServerCmd( "conquest_jointeam" )]
		public static void JoinTeam( string name )
		{
			var player = ConsoleSystem.Caller.Pawn as Player;
			var team = name.ToEnum<Team>();

			player.Team = team;
		}
	}
}
