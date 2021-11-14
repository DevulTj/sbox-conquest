
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	[Library( "KillFeedEntryPanel" )]
	public class KillFeedEntryPanel : Panel
	{
		public TimeSince CreationTime { get; set; } = 0;
		public float TimeToShow { get; set; } = 5f;

		public Label Left { get; set; }
		public Label Method { get; set; }
		public Label Right { get; set; }

		public KillFeedEntryPanel()
		{
			Left = Add.Label( "DevulTj", "friendly" );
			Method = Add.Label( "[AK-47]", "method" );
			Right = Add.Label( "Bot", "enemy" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( CreationTime > TimeToShow && !IsDeleting )
			{
				Delete();
			}
		}
	}

	[Library( "KillFeedMessagePanel" )]
	public class KillFeedMessagePanel : Panel
	{
		public TimeSince CreationTime { get; set; } = 0;
		public float TimeToShow { get; set; } = 5f;

		public Label Left { get; set; }

		public KillFeedMessagePanel()
		{
			Left = Add.Label( "hey!", "message" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( CreationTime > TimeToShow && !IsDeleting )
			{
				Delete();
			}
		}
	}

	[UseTemplate]
	public class KillFeedPanel : Panel
	{
		public static KillFeedPanel Current;

		public KillFeedPanel()
		{
			Current = this;
		}

		public static Client FromSteamId( long steamId )
		{
			return Client.All.Where( x => x.PlayerId == steamId ).FirstOrDefault();
		}

		public virtual Panel AddKill( long lsteamid, string left, long rsteamid, string right, string method )
		{
			var e = Current.AddChild<KillFeedEntryPanel>();

			var myTeam = TeamSystem.MyTeam;
			var leftTeam = TeamSystem.GetTeam( FromSteamId( lsteamid ) );
			var rightTeam = TeamSystem.GetTeam( FromSteamId( rsteamid ) );

			e.Left.Text = left;
			e.Left.SetClass( "friendly", TeamSystem.IsFriendly( myTeam, leftTeam ) );
			e.Left.SetClass( "enemy", TeamSystem.IsHostile( myTeam, leftTeam ) );

			e.Left.SetClass( "me", lsteamid == (Local.Client?.PlayerId) );

			e.Method.Text = $"[{method}]";

			e.Right.Text = right;
			e.Right.SetClass( "friendly", TeamSystem.IsFriendly( myTeam, rightTeam ) );
			e.Right.SetClass( "enemy", TeamSystem.IsHostile( myTeam, rightTeam ) );
			e.Right.SetClass( "me", rsteamid == (Local.Client?.PlayerId) );

			return e;
		}

		public virtual Panel AddMessage( string message )
		{
			var e = Current.AddChild<KillFeedMessagePanel>();
			e.Left.Text = message;

			return e;
		}
	}
}
