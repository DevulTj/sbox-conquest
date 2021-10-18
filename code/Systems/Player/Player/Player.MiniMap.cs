using Conquest.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class Player 
	{
		string IMiniMapEntity.GetMainClass() => "player";

		bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
		{
			if ( !this.IsValid() )
				return false;

			info.Text = "";
			info.Position = Position;

			var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );

			info.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
			info.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;
			info.Classes["me"] = Local.Pawn == this;

			return true;
		}
	}
}
