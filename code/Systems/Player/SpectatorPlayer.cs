using Sandbox;
using System;
using System.Linq;

namespace Conquest
{
	public partial class SpectatorPlayer : BasePlayer
	{
		public SpectatorPlayer()
		{

		}

		public SpectatorPlayer( Client cl ) : this()
		{

		}

		protected override void MakeHud()
		{
			Hud = new SpectatorHud();
		}
	}
}
