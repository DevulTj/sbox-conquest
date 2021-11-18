
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Conquest.Net;

public class To
{
	public static IEnumerable<Client> BLUFOR => Team( Conquest.Team.BLUFOR );
	public static IEnumerable<Client> OPFOR => Team( Conquest.Team.BLUFOR );

	public static IEnumerable<Client> Team( Team team ) =>  Client.All.Where( x => TeamSystem.GetTeam( x ) == team );
	public static IEnumerable<Client> Squad( Client client ) => SquadManager.GetSquad( client ).Members;
}
