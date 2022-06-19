using Sandbox;
using SandboxEditor;

namespace Conquest;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
/// 
[Solid]
[HammerEntity]
[Library( "conquest_headquarters" )]
[EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
[Title("Headquarters"), Category( "Conquest - Setup" ), Description( "Defines a headquarters where the player can (re)spawn" )]
public partial class Headquarters : Entity, IGameStateAddressable, IMiniMapEntity
{
	[Net, Property] public Team Team { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	void IGameStateAddressable.ResetState()
	{

	}

	string IMiniMapEntity.GetMainClass() => "capturepoint";
	bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		info.Text = "HQ";
		info.Position = Position;

		var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );
		info.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
		info.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;
		info.Classes["headquarters"] = true;

		return true;
	}
}
