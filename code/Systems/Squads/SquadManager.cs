
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Conquest;

public partial class SquadManager : BaseNetworkable
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

	public static Squad GetSquad( Client cl )
	{
		return Current.NetworkableSquads.Where( x => x.Members.Contains( cl ) ).FirstOrDefault();
	}

	// Local
	public static bool IsSquadmate( Client cl )
	{
		return MySquad.Members.Contains( cl );
	}

	private static Squad LocalCachedSquad;
	public static Squad MySquad
	{
		get
		{
			if ( LocalCachedSquad is not null )
				return LocalCachedSquad;

			var squad = GetSquad( Local.Client );
			if ( squad is not null )
			{
				LocalCachedSquad = squad;
			}

			return LocalCachedSquad;
		}
	}

	public Dictionary<Team, List<Squad>> Squads { get; set; } = new() { { Team.BLUFOR, new List<Squad>() }, { Team.OPFOR, new List<Squad>() } };

	[Net] public IList<Squad> NetworkableSquads { get; set; }

	public void Print()
	{
		var position = Host.IsServer ? new Vector2( 100, 100 ) : new Vector2( 100, 400 );
		DebugOverlay.ScreenText( position, 0, Color.White, $"{(Host.IsServer ? "[Server]" : "[Client]" )}\n" +
			$"There are {NetworkableSquads.Count} squads.\n" +
			$"{string.Join( $"\n",NetworkableSquads?.Select( x => x.ToString()) )}", 0.05f );
	}

	public Squad New( Team team )
	{
		var newSquad = new Squad( team );
		Squads[team].Add( newSquad );

		newSquad.Identity = SquadNames[ Squads[team].Count - 1 ];
		
		Log.Info( "Conquest", $"Squad created. It's called \"{newSquad.Identity}\"" );

		NetworkableSquads.Add( newSquad );

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
		var squadRef = GetOrCreateSquad( TeamSystem.GetTeam( client ) );
		// squadComponent.SquadRef = squadRef;
		squadRef.Add( client );

		Log.Info( "Conquest", $"Client {client.Name} was added to squad: {squadRef.Identity}" );
	}

	public void Clear( Client client )
	{
		var squad = GetSquad( client );
		if ( squad is null )
			return;

		Log.Info( "Conquest", $"Client {client.Name} was removed from squad: {(squad?.Identity ?? "Unknown")}" );

		squad.Remove( client );

		if ( squad.Members.Count <= 0 )
		{
			Squads[squad.Team].Remove( squad );
			NetworkableSquads.Remove( squad );
		}
	}
}
