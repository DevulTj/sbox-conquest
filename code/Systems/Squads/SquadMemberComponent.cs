
using Sandbox;

namespace Conquest;

public partial class SquadMemberComponent : EntityComponent
{
	[Net] public Squad SquadRef { get; set; }
}
