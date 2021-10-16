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
			var localPlayer = Local.Pawn as BasePlayer;

			bool isHidden = false;

			if ( localPlayer is Player actualPlayer )
			{
				isHidden = actualPlayer.CapturePoint == CapturePoint;
			}

			SetMarkerClass( "hidden", isHidden );

			// No need to do anything else if we're hidden.
			if ( isHidden )
				return;

			var friendState = TeamSystem.GetFriendState( localPlayer.Team, CapturePoint.Team );
			string name = CapturePoint.Identity;
			bool flipflop = ((float)CapturePoint.TimeSinceStateChanged).FloorToInt() % 1 == 0;

			SetMarkerClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
			SetMarkerClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );
			SetMarkerClass( "contested", CapturePoint.CurrentState == CapturePointEntity.State.Contested );
			SetMarkerClass( "contestedFlash", CapturePoint.CurrentState == CapturePointEntity.State.Contested && flipflop );
			SetMarkerClass( "capturing", CapturePoint.CurrentState == CapturePointEntity.State.Capturing );
			SetMarkerClass( "capturingFlash", CapturePoint.CurrentState == CapturePointEntity.State.Capturing && flipflop );
			SetMarkerClass( "friendlyCapturing", CapturePoint.CurrentState == CapturePointEntity.State.Capturing && TeamSystem.IsFriendly( localPlayer.Team, CapturePoint.HighestTeam ) );

			OccupantsLabel.Text = string.Join( $" / ", CapturePoint.OccupantCounts.ToList().Select( x => x.ToString() ) );
			MarkerName = name;
		}
	}
}
