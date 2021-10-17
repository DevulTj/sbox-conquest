using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace Conquest
{
	[UseTemplate("systems/ui/hud/playerhud.html")]
	public class PlayerHud : BaseHud
	{
		public static PlayerHud Current { get; set; } 

		public Panel Root { get; set; }
		public Panel Vitals { get; set; }
		public Label Health { get; set; }
		public Panel HealthBar { get; set; }
		public NameTags Tags { get; set; }

		// weapon info
		public Panel GunVitals { get; set; }
		public Label GunAmmo { get; set; }
		public Label GunReserve { get; set; }

		public Label GunName { get; set; }

		public Label PlayerName { get; set; }

		public Panel Inventory { get; set; }

		public List<InventoryItem> Items { get; set; } = new();

		public Panel Main { get; set; }

		public PlayerHud()
		{
			Current = this;

			for (int i = 0; i < 5; i++ )
			{
				var item = Inventory.AddChild<InventoryItem>( i < 2 ? "large" : "small" );
				item.SlotIndex = i;
				Items.Add( item );
			}
		}

		public override void Tick()
		{
			base.Tick();

			var player = Local.Pawn as Player;
			if ( player is null )
				return;

			if ( Health == null )
				return;
		
			Health.Text = $"{player?.Health:n0}";
			var healthPercent = ( player.Health / 100f ) * 100f;

			// Danger if at 20% hp
			Health.SetClass( "danger", healthPercent < 0.2 );

			HealthBar.Style.Width = Length.Percent( healthPercent );

			var weapon = Local.Pawn.ActiveChild as BaseWeapon;
			if ( weapon is not null && weapon.ShowAmmoCount )
			{
				GunName.Text = $"{weapon.ClassInfo.Title.ToUpper()}";
				GunAmmo.Text = $"{weapon.AmmoClip.ToString("D3")}";
				GunReserve.Text = $"{weapon.AvailableAmmo().ToString("D3")}";
				GunVitals.Style.Opacity = 1;
			}
			else
			{
				GunVitals.Style.Opacity = 0;
			}

			PlayerName.Text = $"{player.Client.Name.ToUpper()}";
		}
	}
}
