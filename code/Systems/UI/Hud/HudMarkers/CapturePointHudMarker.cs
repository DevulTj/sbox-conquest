using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Conquest.UI
{
	public partial class CapturePointHudMarker : HudMarker
	{
		public Label OccupantsLabel { get; set; }

		public CapturePointEntity CapturePoint => Entity as CapturePointEntity;

		public CapturePointHudMarker( Entity parent, string name = "Capture Point" ) : base()
		{
			Entity = parent;
			PositionOffset = new Vector3( 0, 0, 96f );
			MarkerName = name;
			StayOnScreen = true;

			AddClass( "capturepoint" );

			MarkerNameLabel.Text = MarkerName;

			OccupantsLabel = AddChild<Label>( "occupants" );
		}

		public override void Refresh()
		{
			var name = CapturePoint.Identity;
			var player = Local.Pawn as Player;
			var friendState = TeamSystem.GetFriendState( player.Team, CapturePoint.Team );

			SetMarkerClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
			SetMarkerClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );

			OccupantsLabel.Text = string.Join( $" / ", CapturePoint.OccupantCounts.ToList().Select( x => x.ToString() ) );

			SetMarkerClass( "contested", CapturePoint.CurrentState == CapturePointEntity.State.Contested );

			var flipflop = ((float)CapturePoint.TimeSinceStateChanged).FloorToInt() % 2 == 0;
			SetMarkerClass( "contestedFlash", CapturePoint.CurrentState == CapturePointEntity.State.Contested && flipflop );

			SetMarkerClass( "capturing", CapturePoint.CurrentState == CapturePointEntity.State.Capturing );
			SetMarkerClass( "capturingFlash", CapturePoint.CurrentState == CapturePointEntity.State.Capturing && flipflop );
			SetMarkerClass( "friendlyCapturing", CapturePoint.CurrentState == CapturePointEntity.State.Capturing && TeamSystem.IsFriendly( player.Team, CapturePoint.HighestTeam ) );

			MarkerName = name;
		}
	}
}
