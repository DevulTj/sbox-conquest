using Conquest.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class Player
	{

		[ConVar.ClientData( "conquest_loadout_primary", Saved = true )]
		public static string ChosenPrimaryLoadout { get; set; } = "conquest_fal";

		[ConVar.ClientData( "conquest_loadout_secondary", Saved = true )]
		public static string ChosenSecondaryLoadout { get; set; } = "conquest_deserteagle";
	}
}
