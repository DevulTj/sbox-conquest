using Sandbox;

namespace Conquest;

public partial class SpectatorPlayer : BasePlayer
{
	public SpectatorPlayer()
	{
	}

	public SpectatorPlayer( Client cl ) : this()
	{
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( cl.IsBot )
		{
			Game.Deploy( cl );
		}
	}

	protected override void MakeHud()
	{
		Hud = new SpectatorHud();
	}
}
