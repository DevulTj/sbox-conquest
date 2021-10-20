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
		public string GetMainClass() => "player";

		bool CalculateVis()
		{
			var tr = Trace.Ray( CurrentView.Position, EyePos )
				.WorldAndEntities()
				.Ignore( Local.Pawn )
				.Run();

			if ( tr.Distance > 1024 ) return false;

			if ( tr.Hit && tr.Entity == this )
				return true;
			else
				return false;
		}

		bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
		{
			if ( !this.IsValid() )
				return false;

			info.Text = "";
			info.Position = Position;

			var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );

			if ( friendState == TeamSystem.FriendlyStatus.Hostile )
			{
				if ( !CalculateVis() )
					return false;
			}

			info.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
			info.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;
			info.Classes["me"] = Local.Pawn == this;


			return true;
		}

		bool IHudMarkerEntity.Update( ref HudMarkerBuilder info )
		{
			if ( !this.IsValid() )
				return false;

			if ( this == Local.Pawn )
				return false;

			var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );

			if ( friendState == TeamSystem.FriendlyStatus.Hostile )
			{
				if ( !CalculateVis() )
					return false;
			}

			info.Text = Client.Name;
			info.Position = EyePos + Rotation.Up * 15f;

			info.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
			info.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;

			return true;
		}
	}
}
