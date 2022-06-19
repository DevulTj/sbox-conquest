
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Conquest;

[UseTemplate]
public class LoadoutWeaponPanel : PopupButton
{
	private LoadoutAsset Selected { get; set; }

	public bool IsSelector { get; set; } = false;
	public WeaponSlot Slot { get; set; } = WeaponSlot.Primary;

	// @ref
	public Image WeaponIcon { get; set; }
	// @text
	public string WeaponName { get; set; } = "GUN";

	public LoadoutWeaponPanel()
	{
		WeaponIcon.SetTexture( "ui/WeaponIcons/conquest_ak47.png" );

		//AddEventListener( "onclick", OnClick );
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "type" && value == "selector" )
		{
			IsSelector = true;
		}
	}

	// Fetch from game data
	public void Fetch()
	{
		var primaryLoadoutStr = Player.ChosenPrimaryLoadout;
		var secondaryLoadoutStr = Player.ChosenSecondaryLoadout;
		var weaponsOfThisSlot = LoadoutAsset.Sorted[Slot];

		LoadoutAsset asset = null;
			
		if ( Slot == WeaponSlot.Primary )
			asset = weaponsOfThisSlot.Where( x => x.Class == primaryLoadoutStr ).FirstOrDefault();
		else if ( Slot == WeaponSlot.Secondary )
			asset = weaponsOfThisSlot.Where( x => x.Class == secondaryLoadoutStr ).FirstOrDefault();

		if ( asset != null )
			SetActive( asset );
	}

	public void SetActive( LoadoutAsset loadout, Type library = null )
	{
		if ( library is null )
			library = TypeLibrary.GetTypeByName<BaseWeapon>( loadout.Class );

		var info = DisplayInfo.ForType( library );

		WeaponName = info.Name;
		WeaponIcon.SetTexture( $"ui/weaponicons/{loadout.Class}.png" );

		switch ( loadout.Slot )
		{
			case WeaponSlot.Primary:
			{
				Player.ChosenPrimaryLoadout = loadout.Class;
				ConsoleSystem.Run( "conquest_loadout_primary", loadout.Class );
				break;
			}
			case WeaponSlot.Secondary:
			{
				Player.ChosenSecondaryLoadout = loadout.Class;
				ConsoleSystem.Run( "conquest_loadout_secondary", loadout.Class );

				break;
			}
			default:
				Log.Warning( "Failed to assign a loadout slot" );
				break;
		}
	}

	public override void Open()
	{
		if ( IsSelector )
		{
			Popup = new Popup( this, Popup.PositionMode.AboveLeft, 0.0f );
			Popup.AddClass( "flat-top" );
			Popup.StyleSheet.Load( "systems/ui/hud/respawnscreen/loadoutweapon/loadoutweaponpanel.scss" );

			foreach ( var option in LoadoutAsset.Sorted[Slot] )
			{
				var library = TypeLibrary.GetTypeByName<BaseWeapon>( option.Class );
				var info = DisplayInfo.ForType( library );

				var weaponName = library != null ? library.Name : option.Class;

				var o = Popup.AddOption( weaponName, () => SetActive( option, library ) );
				o.AddClass( "loadoutweapon" );
				o.StyleSheet.Load( "systems/ui/hud/respawnscreen/loadoutweapon/loadoutweaponpanel.scss" );

				if ( Selected != null && option == Selected )
				{
					o.AddClass( "active" );
				}
			}
		}
	}
}
