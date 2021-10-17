
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	[UseTemplate( "systems/ui/hud/respawnscreen/respawnscreen.html" )]
	public class RespawnScreen : Panel
	{
		public static bool Exists = false;
		public static TimeSince TimeSinceDeployed = -1;
		public static float DeployAnimTime => 1;
		public static Vector3 Position => new Vector3( -186.83f, -805.75f, 5024.03f );
		public static Angles Angles = new Angles( 90, 90, 0 );

		// @ref
		public Button DeployButton { get; set; }
		public Label GameName { get; set; }
		public Label LoadoutPanel { get; set; }
		// -

		public RespawnScreen()
		{
			Exists = true;
		}
		public override void OnDeleted()
		{
			Exists = false;

			TimeSinceDeployed = 0;

			base.OnDeleted();
		}

		public override void Tick()
		{
			base.Tick();
		}

		public void Deploy()
		{
			Host.AssertClient();

			Game.DeployCommand();
		}
	}
}
