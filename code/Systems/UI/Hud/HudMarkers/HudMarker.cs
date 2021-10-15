
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

		public bool StayOnScreen { get; set; } = false;
		public Vector2 SafetyBounds { get; set; } = new Vector2( 0.02f, 0.02f );

		public bool IsFocused { get; set; } = true;

		public void SetMarkerClass( string className, bool state )
		{
			SetClass( className, state );
			MarkerNameLabel.SetClass( className, state );
		}

		public void AddMarkerClass( string className )
		{
			AddClass( className );
			MarkerNameLabel.AddClass( className );
		}

		public void RemoveMarkerClass( string className )
		{
			RemoveClass( className );
			MarkerNameLabel.RemoveClass( className );
		}

		public bool PositionAtWorld()
		{
			var screenpos = GetScreenPoint();

			if ( screenpos.z < 0 )
				return false;

			var cachedX = screenpos.x;
			var cachedY = screenpos.y;

			var isFocused = cachedX.AlmostEqual( 0.5f, 0.05f ) && cachedY.AlmostEqual( 0.5f, 0.05f );

			IsFocused = isFocused;
			SetMarkerClass( "nofocus", !isFocused );

			if ( StayOnScreen )
			{
				var safetyX = SafetyBounds.x;
				var safetyY = SafetyBounds.y;

				screenpos.x = screenpos.x.Clamp( safetyX, 1 - safetyX );
				screenpos.y = screenpos.y.Clamp( safetyY, 1 - safetyY );
			}

			Style.Left = Length.Fraction( screenpos.x );
			Style.Top = Length.Fraction( screenpos.y );
			Style.Dirty();

			return cachedX < 0 || cachedX > 1 || cachedY < 0 || cachedY > 1;
		}

		public Vector3 GetWorldPoint()
		{
			if ( Point.IsNearlyZero() && Entity.IsValid() )
				return Entity.Position + PositionOffset;

			return Point;
		}

		public Vector3 GetScreenPoint()
		{
			var worldPoint = GetWorldPoint();
			var screenPoint = worldPoint.ToScreen();

			return screenPoint;
		}

		public abstract void Refresh();
	}
}
