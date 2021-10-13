
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	[UseTemplate( "systems/ui/hud/inventoryitem.html" )]
	public class InventoryItem : Panel
	{
		public Carriable Weapon { get; set; }

		public Image Icon { get; set; }

		public Label SlotNumber { get; set; }
		public Label Ammo { get; set; }
		public int SlotIndex { get; set; } = -1;

		public InventoryItem()
		{

		}

		public override void SetProperty( string name, string value )
		{
			base.SetProperty( name, value );

			if ( name == "type" )
			{
				AddClass( value );
			}
		}

		public override void Tick()
		{
			var player = Local.Pawn as Player;
			var active = player?.ActiveChild as Carriable;

			var weaponFromSlot = player.Inventory.GetSlot( SlotIndex ) as Carriable;
			var showInfinity = false;

			Log.Info( $"{SlotIndex}: {weaponFromSlot}" );

			if ( Weapon is null && weaponFromSlot is not null )
			{
				Weapon = weaponFromSlot;
				Icon?.SetTexture( $"ui/WeaponIcons/{Weapon.ClassInfo.Name}.png" );
			}


			if ( weaponFromSlot is BaseWeapon weapon )
			{
				Ammo.Text = $"{weapon.AmmoClip}";
			}
			else if ( weaponFromSlot is null )
			{
				Ammo.Text = "∞";
				showInfinity = true;
			}

			Ammo.SetClass( "infinity", showInfinity );


			SlotNumber.Text = $"{SlotIndex + 1}";

			SetClass( "active", Weapon == active );
			Ammo.SetClass( "active", Weapon == active );
		}
	}
}
