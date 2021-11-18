
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Conquest.Net;

public class To
{
	public static Sandbox.To BLUFOR => Team( Conquest.Team.BLUFOR );
	public static Sandbox.To OPFOR => Team( Conquest.Team.BLUFOR );

	public static Sandbox.To Team( Team team ) => Sandbox.To.Multiple( Client.All.Where( x => TeamSystem.GetTeam( x ) == team ) );
	public static Sandbox.To Squad( Client client ) => Sandbox.To.Multiple( SquadManager.GetSquad( client ).Members );
}
