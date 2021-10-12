
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	[UseTemplate( "systems/ui/hud/capturestatus/capturestatus.html" )]
	public class CaptureStatus : Panel
	{
		// @ref
		public Panel FriendlyTeamBar { get; set; }
		// @ref
		public Label FriendlyTeamName { get; set; }
		// @ref
		public Panel EnemyTeamBar { get; set; }
		// @ref
		public Label EnemyTeamName { get; set; }

		public Panel Point { get; set; }

		public Panel Contest { get; set; }

		public CaptureStatus()
		{

		}

		protected float MaxScore => 1000;

		protected void SetCaptureClass( string className, bool active )
		{
			Point.SetClass( className, active );
			Contest.SetClass( className, active );
		}

		public override void Tick()
		{
			base.Tick();

			var localPlayer = Local.Pawn as Player;
			var capturePoint = localPlayer.CapturePoint;

			if ( capturePoint is null )
			{
				Style.Opacity = 0;
				Style.Dirty();

				return;
			}

			var friendlyTeamCount = capturePoint.GetCount( localPlayer.Team );
			var enemyTeamCount = capturePoint.GetCount( TeamSystem.GetEnemyTeam( localPlayer.Team ) );

			var total = friendlyTeamCount + enemyTeamCount;

			var topOffsetForPoint = enemyTeamCount == 0 ? 64 : 128;

			Point.Style.Top = Length.Pixels( topOffsetForPoint );

			if ( enemyTeamCount == 0 )
			{
				//Contest.Style.Opacity = 0;
				//Contest.Style.Dirty();

				//return;
			}

			FriendlyTeamBar.Style.Width = Length.Fraction( (float)friendlyTeamCount / (float)total );
			EnemyTeamBar.Style.Width = Length.Fraction( (float)enemyTeamCount / (float)total );
			EnemyTeamBar.Style.Left = Length.Fraction( (float)friendlyTeamCount / (float)total );

			if ( enemyTeamCount == 0 )
			{
				EnemyTeamName.Text = "";
				EnemyTeamBar.Style.Opacity = 0;
			}
			else
			{
				EnemyTeamName.Text = $"{enemyTeamCount}";
				EnemyTeamBar.Style.Opacity = 1;
			}

			FriendlyTeamName.Text = $"{friendlyTeamCount}";

			FriendlyTeamBar.Style.Dirty();
			EnemyTeamBar.Style.Dirty();

			Style.Opacity = 1;
			Style.Dirty();

			SetCaptureClass( "friendly", capturePoint.Team == localPlayer.Team );
			SetCaptureClass( "enemy", capturePoint.Team == TeamSystem.GetEnemyTeam( localPlayer.Team ) );

			var percent = capturePoint.Captured * 100f;
			Point.Style.Set( "background", $"conic-gradient(transparent {percent}%, rgba(50, 50, 50, 0.8) {percent}%);" );
			Point.Style.Dirty();
		}
	}
}
