using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace Conquest
{
	[UseTemplate]
	public class PlayerHud : BaseHud
	{
		public Panel Root { get; set; }
		public Panel Vitals { get; set; }
		public Label Health { get; set; }
		public Label Money { get; set; }
		public Label Job { get; set; }
		public Label Salary { get; set; }
		public NameTags Tags { get; set; }

		// weapon info
		public Panel GunVitals { get; set; }
		public Label GunName { get; set; }
		public Label GunAmmo { get; set; }
		public Label GunReserve { get; set; }

		public PlayerHud() { }

		public static int ShouldDrawHudState { get; set; } = 1;

		protected void ShouldDrawHud()
		{
			if ( ShouldDrawHudState > 0 )
				Vitals.Style.Opacity = 1;
			else
				Vitals.Style.Opacity = 0;
		}

		private void AddTimedClass( string type, float timeSeconds = 0.5f )
		{
			_ = AddTimedClass( Money, type, timeSeconds );
		}

		private async Task AddTimedClass( Panel target, string type, float timeSeconds = 0.5f )
		{
			target.AddClass( type );
			await Task.DelaySeconds( timeSeconds );
			target.RemoveClass( type );
		}

		public override void Tick()
		{
			base.Tick();

			var player = Local.Pawn as Player;
			if ( player is null )
				return;

			if ( Health == null || Money == null || Salary == null )
				return;

			ShouldDrawHud();

			Health.Text = $"{player?.Health:n0}";
			// Danger if at 20% hp
			Health.SetClass( "danger", (player.Health / 100f) < 0.2 );

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
			Job.Style.Dirty();
		}
	}
}
