
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest
{
	public class SquadManager : BaseNetworkable
	{
		public List<string> SquadNames = new()
		{
			"Alpha",
			"Bravo",
			"Charlie",
			"Delta",
			"Echo",
			"Foxtrot",
			"Golf",
			"Hotel",
			"Indigo",
			"Juliet",
			"Kilo",
			"Lima",
			"Mike",
			"November",
			"Oscar",
			"Papa",
			"Quebec",
			"Romeo",
			"Sierra",
			"Tango",
			"Uniform",
			"Whiskey",
			"Xray",
			"Yankee",
			"Zulu"
		};

		public SquadManager()
		{
			Current = this;
		}

		public static SquadManager Current;

		public  static Squad GetSquad( Client cl )
		{
			return cl.Components.Get<SquadMemberComponent>().SquadRef;
		}

		public static Squad MySquad => Local.Client.Components.Get<SquadMemberComponent>()?.SquadRef;

		public Dictionary<Team, List<Squad>> Squads { get; set; } = new() { { Team.BLUFOR, new List<Squad>() }, { Team.OPFOR, new List<Squad>() } };

		public Squad New( Team team )
		{
			var newSquad = new Squad();
			Squads[team].Add( newSquad );

			newSquad.Identity = SquadNames[ Squads[team].Count - 1 ];

			Log.Info( $"[Conquest] Squad created. It's called \"{newSquad.Identity}\"" );

			return newSquad;
		}

		public Squad GetOrCreateSquad( Team team )
		{
			var squad = Squads[team].LastOrDefault();
			if ( squad is null || squad.IsFull )
			{
				squad = New( team );
			}

			return squad;
		}

		/// <summary>
		/// Assigns a player to a squad, stack-behavior
		/// </summary>
		/// <param name="client"></param>
		public void Assign( Client client )
		{
			var squadComponent = client.Components.GetOrCreate<SquadMemberComponent>();
			// Already in a squad
			if ( squadComponent.SquadRef is not null )
				return;

			var squadRef = GetOrCreateSquad( TeamSystem.GetTeam( client ) );

			squadComponent.SquadRef = squadRef;
			squadRef.Add( client );

			Log.Info( $"[Conquest] Client {client.Name} was added to squad: {squadRef.Identity}" );
		}

		public void Clear( Client client )
		{
			var squadComponent = client.Components.GetOrCreate<SquadMemberComponent>();
			squadComponent.SquadRef?.Remove( client );

			Log.Info( $"[Conquest] Client {client.Name} was removed from squad: {(squadComponent.SquadRef?.Identity ?? "Unknown")}" );
		}
	}
}
