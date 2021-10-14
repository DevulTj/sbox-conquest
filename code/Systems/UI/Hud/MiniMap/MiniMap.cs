
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Conquest
{
	public class MiniMapCapturePoint : Panel
	{
		public Label Identity { get; set; }

		public MiniMapCapturePoint()
		{
			AddClass( "capturepoint" );
			Identity = AddChild<Label>();
		}
	}

	[UseTemplate( "systems/ui/hud/minimap/minimap.html" )]
	public class MiniMap : Panel
	{
		public Panel CapturePointsPanel { get; set; }
		public Panel MiniMapPanel { get; set; }

		public override void Tick()
		{
			base.Tick();

			var localPlayer = Local.Pawn as Player;

			int i = 0;
			foreach( var capturePoint in Entity.All.OfType<CapturePointEntity>().OrderBy( x => x.Identity ) )
			{
				MiniMapCapturePoint panel;

				if ( CapturePointsPanel.ChildrenCount > i )
					panel = CapturePointsPanel.GetChild( i ) as MiniMapCapturePoint;
				else
					panel = CapturePointsPanel.AddChild<MiniMapCapturePoint>();

				i++;

				panel.Identity.Text = capturePoint.Identity;

				var flipflop = ((float)capturePoint.TimeSinceStateChanged).FloorToInt() % 1 == 0;
				var friendState = TeamSystem.GetFriendState( localPlayer.Team, capturePoint.Team );

				panel.SetClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
				panel.SetClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );
				panel.SetClass( "contested", capturePoint.CurrentState == CapturePointEntity.State.Contested );
				panel.SetClass( "contestedFlash", capturePoint.CurrentState == CapturePointEntity.State.Contested && flipflop );
				panel.SetClass( "capturing", capturePoint.CurrentState == CapturePointEntity.State.Capturing );
				panel.SetClass( "capturingFlash", capturePoint.CurrentState == CapturePointEntity.State.Capturing && flipflop );
			}
		}
	}
}
