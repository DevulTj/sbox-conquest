
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
