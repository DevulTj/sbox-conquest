
using Sandbox;
using System.ComponentModel;

namespace Conquest
{
	public partial class SquadMemberComponent : EntityComponent
	{
		[Net, Category( "SquadSystem" ), Property]
		public Squad SquadRef { get; set; }
	}
}
