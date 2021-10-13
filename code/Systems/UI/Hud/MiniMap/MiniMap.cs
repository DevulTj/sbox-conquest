
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	[UseTemplate( "systems/ui/hud/minimap/minimap.html" )]
	public class MiniMap : Panel
	{
		public Panel CapturePointsPanel { get; set; }
		public Panel MiniMapPanel { get; set; }

		public List<Panel> CapturePointPanels { get; set; } = new();

		public MiniMap()
		{
			foreach( var capturePoint in Entity.All.OfType<CapturePointEntity>() )
			{
				var panel = CapturePointsPanel.AddChild<Panel>( "capturepoint" );
				var label = panel.AddChild<Label>();
				label.Text = capturePoint.Identity;

				CapturePointPanels.Add( panel );
			}
		}

		public override void Tick()
		{
			base.Tick();

			var localPlayer = Local.Pawn as Player;

			int i = 0;
			foreach( var capturePoint in Entity.All.OfType<CapturePointEntity>() )
			{
				var friendState = TeamSystem.GetFriendState( localPlayer.Team, capturePoint.Team );
				CapturePointPanels[i].SetClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
				CapturePointPanels[i].SetClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );
			}
		}
	}
}
