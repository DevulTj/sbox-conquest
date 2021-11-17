
using Sandbox;
using Sandbox.UI;

namespace Conquest.UI;

public partial class HudMarker : Panel
{
	public HudMarker( IHudMarkerEntity entity )
	{
		Label = AddChild<Label>( "label" );
		Entity = entity;
	}

	public Label Label { get; set; }
	public IHudMarkerEntity Entity { get; set; }

	public bool StayOnScreen { get; set; } = false;
	public Vector2 SafetyBounds { get; set; } = new Vector2( 0.02f, 0.02f );

	public bool IsFocused { get; set; } = true;

	public Vector3 Position { get; set; } = new();

	public void Apply( HudMarkerBuilder info )
	{
		Label.Text = info.Text;
		Position = info.Position;
		StayOnScreen = info.StayOnScreen;

		foreach ( var kv in info.Classes )
			SetClass( kv.Key, kv.Value );

		PositionAtWorld();
	}

	public bool PositionAtWorld()
	{
		var screenpos = GetScreenPoint();

		var cachedX = screenpos.x;
		var cachedY = screenpos.y;

		var isFocused = cachedX.AlmostEqual( 0.5f, 0.05f ) && cachedY.AlmostEqual( 0.5f, 0.2f );

		IsFocused = isFocused;
		SetClass( "nofocus", !isFocused );

		if ( StayOnScreen )
		{
			var safetyX = SafetyBounds.x;
			var safetyY = SafetyBounds.y;

			screenpos.x = screenpos.x.Clamp( safetyX, 1 - safetyX );
			screenpos.y = screenpos.y.Clamp( safetyY, 1 - safetyY );
		}

		Style.Left = Length.Fraction( screenpos.x );
		Style.Top = Length.Fraction( screenpos.y );

		return cachedX < 0 || cachedX > 1 || cachedY < 0 || cachedY > 1;
	}

	public Vector3 GetScreenPoint()
	{
		var worldPoint = Position;
		var screenPoint = worldPoint.ToScreen();

		return screenPoint;
	}
}
