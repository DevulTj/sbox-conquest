using Sandbox;

namespace Conquest;

[Library( "conquest_worldoverview" )]
[Hammer.EditorModel( "models/editor/camera.vmdl" )]
[Hammer.EntityTool( "World Overview", "Conquest", "Used to specify the world overview position / rotation." )]
public class WorldOverview : Entity 
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
