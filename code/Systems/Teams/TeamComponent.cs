
using Sandbox;
using System;

namespace Conquest
{
	public partial class TeamComponent : EntityComponent
	{
		[Net] public Team Team { get; set; } = Team.Unassigned;
	}
}
