using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest.UI
{
	public partial class HudMarkers : Panel
	{
		public static HudMarkers Current;

		public List<HudMarker> Markers { get; set; } = new();

		public HudMarkers()
		{
			Current = this;

			StyleSheet.Load( "systems/ui/hud/hudmarkers/hudmarkers.scss" );

			foreach ( var capturePoint in Entity.All.OfType<CapturePointEntity>() )
			{
				AddMarker( capturePoint.Marker );
			}
		}

		public static void Create( HudMarker marker )
		{
			if ( Current is null )
				throw new Exception( "Tried to make a marker with no HudMarkers ref" );

			Current.AddMarker( marker );
		}

		public HudMarker AddMarker( HudMarker marker )
		{
			if ( Markers.Contains( marker ) )
				throw new Exception( "Marker was already added to Markers List" );

			Markers.Add( marker );

			marker.Parent = this;

			return marker;
		}

		protected void TickMarker( HudMarker marker )
		{
			marker.Refresh();
			marker.SetClass( "offscreen", marker.PositionAtWorld() );
		}

		public override void Tick()
		{
			base.Tick();

			Markers.ForEach( x => TickMarker( x ) );
		}
	}
}
