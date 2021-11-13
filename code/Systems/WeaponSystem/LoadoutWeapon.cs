
using Sandbox;
using System.Collections.Generic;

namespace Conquest
{
	[Library( "loadout" ), AutoGenerate]
	public class LoadoutAsset : Asset
	{
		public override string ToString() => $"Conquest.Asset.Loadout(Name={Name})";
		public static List<LoadoutAsset> All = new();

		public static Dictionary<WeaponSlot, List<LoadoutAsset>> Sorted = new()
		{
			{ WeaponSlot.Primary, new List<LoadoutAsset>() },
			{ WeaponSlot.Secondary, new List<LoadoutAsset>() },
			{ WeaponSlot.Gadget, new List<LoadoutAsset>() }
		};

		[Property]
		public string Class { get; set; }
		[Property]
		public WeaponSlot Slot { get; set; }

		protected override void PostLoad()
		{
			base.PostLoad();

			if ( !All.Contains( this ) )
			{
				All.Add( this );
				Sorted[Slot].Add( this );

				Log.Info( "[Conquest] Registered Loadout: " + Name );
			}
		}
	}
}
