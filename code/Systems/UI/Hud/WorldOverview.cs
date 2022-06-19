using Sandbox;
using SandboxEditor;

namespace Conquest;

[HammerEntity]
[Library( "conquest_worldoverview" )]
[EditorModel( "models/editor/camera.vmdl" )]
[Title("World Overview"), Category("Conquest - Setup")]
public class WorldOverview : Entity 
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
