
using Sandbox;
using System.ComponentModel;

namespace Conquest
{
	// @Server
	public partial class SquadMemberComponent : EntityComponent
	{
		public Squad SquadRef { get; set; }
	}
}
