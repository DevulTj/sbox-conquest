using Sandbox;
using Sandbox.UI;

namespace Conquest
{
	[UseTemplate]
	public class SpectatorHud : BaseHud
	{
		// @ref
		public RespawnScreen RespawnScreen { get; set; }
		public SpectatorHud()
		{

		}

		public override void Tick()
		{
			base.Tick();
			
			// This is shit.
			if ( Local.Pawn is Player && !IsDeleting )
			{
				Delete();
			}
		}
	}
}
