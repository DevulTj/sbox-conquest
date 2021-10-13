using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace Conquest
{
	[UseTemplate("systems/ui/hud/playerhud.html")]
	public class PlayerHud : BaseHud
	{
		public Panel Root { get; set; }
		public Panel Vitals { get; set; }
		public Label Health { get; set; }
		public Panel HealthBar { get; set; }
		public NameTags Tags { get; set; }

		// weapon info
		public Panel GunVitals { get; set; }
		public Label GunAmmo { get; set; }
		public Label GunReserve { get; set; }

		public PlayerHud() { }

		public Panel PrimaryWeapon { get; set; }
		public Panel SecondaryWeapon { get; set; }
		public Panel FirstGadget { get; set; }
		public Panel SecondGadget { get; set; }
		public Panel ThirdGadget { get; set; }


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
			HealthBar.Style.Dirty();

			var weapon = Local.Pawn.ActiveChild as BaseWeapon;
			if ( weapon is not null && weapon.ShowAmmoCount )
			{
				GunAmmo.Text = $"{weapon.AmmoClip}";
				GunReserve.Text = $"{weapon.AvailableAmmo()}";
				GunVitals.Style.Opacity = 1;
			}
			else
			{
				GunVitals.Style.Opacity = 0;
			}

			GunVitals.Style.Dirty();
		}
	}
}
