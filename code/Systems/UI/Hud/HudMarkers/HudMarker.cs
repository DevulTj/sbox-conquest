
using Sandbox;
using Sandbox.UI;

namespace Conquest.UI
{
	public abstract partial class HudMarker : Panel
	{
		public HudMarker()
		{
			MarkerNameLabel = AddChild<Label>( "label" );
		}

		public Label MarkerNameLabel { get; set; }
		public string MarkerName { get { return MarkerNameLabel.StringValue; } set { MarkerNameLabel.Text = value; } }
		public Entity Entity { get; set; }
		public Vector3 PositionOffset { get; set; }
		public Vector3 Point { get; set; }

		public Vector3 GetWorldPoint()
		{
			if ( Point.IsNearlyZero() )
				return Entity.Position;

			return Point;
		}

		public Vector2 GetScreenPoint()
		{
			var worldPoint = GetWorldPoint();
			var screenPoint = worldPoint.ToScreen();

			return screenPoint;
		}

		public abstract void Refresh();
	}
}
