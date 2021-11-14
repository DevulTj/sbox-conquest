
using Sandbox;
using System.Collections.Generic;

namespace Conquest
{
	public partial class Squad : BaseNetworkable
	{
		public int MaxMembers => 4;
		public int CurrentMembers => Members.Count;

		public bool IsFull => CurrentMembers >= MaxMembers;

		[Net] public string Identity { get; set; } = "";
		[Net] public IList<Client> Members { get; set; }

		public bool Add( Client cl )
		{
			if ( IsFull )
			{
				return false;
			}

			Members.Add( cl );

			return true;
		}

		public bool Remove( Client cl )
		{
			return Members.Remove( cl );
		}
	}
}
