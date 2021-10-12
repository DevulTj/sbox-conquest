
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	[UseTemplate( "systems/ui/hud/minimap/minimap.html" )]
	public class MiniMap : Panel
	{
		public Panel CapturePointsPanel { get; set; }
		public Panel MiniMapPanel { get; set; }
		public MiniMap()
		{

		}
	}
}
