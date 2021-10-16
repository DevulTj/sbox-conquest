
using Sandbox;
using System;

namespace Conquest
{
	public partial class TeamComponent : EntityComponent
	{
		[Net] public TeamSystem.Team Team { get; set; } = TeamSystem.Team.BLUFOR;
	}
}
