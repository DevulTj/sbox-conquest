using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest.UI;

public partial class HudMarkers : Panel
{
	public static HudMarkers Current;

	public HudMarkers()
	{
		Current = this;

		StyleSheet.Load( "systems/ui/hud/hudmarkers/hudmarkers.scss" );
	}


	protected List<HudMarker> Markers { get; set; } = new();


	public void Clear( HudMarker marker )
	{
		if ( marker is null )
			return;
		if ( !Markers.Contains( marker ) )
			return;

		Markers.Remove( marker );
		marker.Delete();

		return;
	}

	protected void ValidateEntity( IHudMarkerEntity entity )
	{
		var info = new HudMarkerBuilder();
		var styleClass = entity.GetMainClass();
		var current = Markers.Where( x => x.Entity == entity ).FirstOrDefault();

		if ( !entity.Update( ref info ) )
		{
			Clear( current );

			return;
		}

		if ( current is null )
		{
			current = new HudMarker( entity )
			{
				Parent = this
			};

			// Add style class
			current.AddClass( styleClass );

			Markers.Add( current );
		}

		current.Apply( info );
	}

	protected void UpdateHudMarkers()
	{
		var existingMarkers = Markers.Select( x => x.Entity ).ToList();

		Entity.All.OfType<IHudMarkerEntity>()
						.Concat( existingMarkers )
						.ToList()
						.ForEach( x => ValidateEntity( x ) );
	}

	public override void Tick()
	{
		base.Tick();
		UpdateHudMarkers();
	}
}
