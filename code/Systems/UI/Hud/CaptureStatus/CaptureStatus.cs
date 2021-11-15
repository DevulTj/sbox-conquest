
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
		public Label FriendlyTeamName { get; set; }
		public Panel EnemyTeamBar { get; set; }
		public Label EnemyTeamName { get; set; }
		public Panel Point { get; set; }
		public Panel Contest { get; set; }
		public Label PointName { get; set; }
		// -

		protected void SetCaptureClass( string className, bool active )
		{
			Point.SetClass( className, active );
			Contest.SetClass( className, active );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not Player localPlayer )
				return;

			var capturePoint = localPlayer.CapturePoint;

			if ( capturePoint is null )
			{
				Style.Opacity = 0;
				return;
			}

			PointName.Text = capturePoint.Identity;

			var friendlyTeamCount = capturePoint.GetCount( localPlayer.Team );
			var enemyTeamCount = capturePoint.GetCount( TeamSystem.GetEnemyTeam( localPlayer.Team ) );

			var total = friendlyTeamCount + enemyTeamCount;

			var topOffsetForPoint = enemyTeamCount == 0 ? 64 : 128;

			Point.Style.Top = Length.Pixels( topOffsetForPoint );

			if ( enemyTeamCount == 0 )
				Contest.Style.Opacity = 0;
			else
				Contest.Style.Opacity = 1;

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

			Style.Opacity = 1;

			SetCaptureClass( "friendly", capturePoint.Team == localPlayer.Team );
			SetCaptureClass( "enemy", capturePoint.Team == TeamSystem.GetEnemyTeam( localPlayer.Team ) );

			var percent = MathF.Round( capturePoint.Captured * 100f );
			Point.Style.Set( "background", $"conic-gradient(transparent {percent}%, rgba(50, 50, 50, 0.8) {percent}%);" );
		}
	}
}
