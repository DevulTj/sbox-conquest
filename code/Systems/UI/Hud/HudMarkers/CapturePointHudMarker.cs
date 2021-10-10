using Sandbox;
using Sandbox.UI;

namespace Conquest.UI
{
	public partial class CapturePointHudMarker : HudMarker
	{
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
			var name = (Entity as CapturePointEntity).Identity;

			MarkerName = name;
		}
	}
}
