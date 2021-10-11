using Sandbox;

namespace Conquest.UI
{
	public partial class CapturePointHudMarker : HudMarker
	{
		public CapturePointEntity CapturePoint => Entity as CapturePointEntity;

		public CapturePointHudMarker( Entity parent, string name = "Capture Point" ) : base()
		{
			Entity = parent;
			PositionOffset = new Vector3( 0, 0, 96f );
			MarkerName = name;
			StayOnScreen = true;

			AddClass( "capturepoint" );

			MarkerNameLabel.Text = MarkerName;
		}



		public override void Refresh()
		{
			var name = CapturePoint.Identity;
			var player = Local.Pawn as Player;
			var friendState = TeamSystem.GetFriendState( player.Team, CapturePoint.Team );

			SetMarkerClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
			SetMarkerClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );

			MarkerName = name;
		}
	}
}
