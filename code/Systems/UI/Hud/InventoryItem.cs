
using Sandbox;
using Sandbox.UI;

namespace Conquest;

[UseTemplate( "systems/ui/hud/inventoryitem.html" )]
public class InventoryItem : Panel
{
	// @ref
	public Image Icon { get; set; }
	public Label SlotNumber { get; set; }
	public Label Ammo { get; set; }
	// -@ref

	public Carriable Weapon { get; set; }
	public int SlotIndex { get; set; } = -1;

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "type" )
			AddClass( value );
	}

	public override void Tick()
	{
		var player = Local.Pawn as Player;
		var active = player?.ActiveChild as Carriable;

		var weaponFromSlot = player.Inventory?.GetSlot( SlotIndex ) as Carriable;

		if ( !weaponFromSlot.IsValid() )
		{
			Icon.SetTexture( "" );
			Weapon = null;
			Ammo.Text = "";
			SlotNumber.Text = "";
			SetClass( "empty", true );

			return;
		}

		SetClass( "empty", false );


		var showInfinity = false;

		if ( Weapon is null && weaponFromSlot is not null )
		{
			Weapon = weaponFromSlot;

			Icon.SetTexture( Weapon.WeaponInfo?.LoadoutIcon );
		}


		if ( weaponFromSlot is BaseGadget gadget || weaponFromSlot.Slot == WeaponSlot.Melee )
		{
			Ammo.Text = $"∞";
			showInfinity = true;
		}
		else if ( weaponFromSlot is BaseWeapon weapon )
		{
			Ammo.Text = $"{player.AmmoCount( weapon.WeaponInfo.AmmoType ) + weapon.AmmoClip}";
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
