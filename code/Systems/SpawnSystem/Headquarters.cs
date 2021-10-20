using Sandbox;
using System.Collections.Generic;

namespace Conquest
{
	/// <summary>
	/// This entity defines the spawn point of the player in first person shooter gamemodes.
	/// </summary>
	[Library( "conquest_headquarters" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Hammer.Solid]
	public partial class Headquarters : Entity, IGameStateAddressable
	{
		[Net, Property] public TeamSystem.Team Team { get; set; }

		void IGameStateAddressable.ResetState()
		{
			
		}
	}
}
