
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Conquest;

public partial class Squad : BaseNetworkable
{
	public override string ToString()
	{
		return $"Conquest.Squad[{Identity}](Leader: {SquadLeader.Name}, Members: {string.Join( $", ", Members?.Where( x => x != SquadLeader ).Select( x => x.Name ) )})";
	}
	public int MaxMembers => 4;
	public int CurrentMembers => Members.Count;

	public bool IsFull => CurrentMembers >= MaxMembers;

	[Net] public string Identity { get; set; } = "";
	[Net] public IList<Client> Members { get; set; }

	[Net] public Client SquadLeader { get; set; }

	public bool Add( Client cl )
	{
		if ( IsFull )
		{
			return false;
		}

		Members.Add( cl );

		if ( SquadLeader is null )
			SquadLeader = cl;

		return true;
	}

	public bool Remove( Client cl )
	{
		Members.Remove( cl );

		if ( SquadLeader == cl )
			SquadLeader = Members.FirstOrDefault();

		return true;
	}
}
