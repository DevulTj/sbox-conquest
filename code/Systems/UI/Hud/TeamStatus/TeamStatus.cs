
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{

	[UseTemplate]
	public class TeamStatus : Panel
	{
		// @ref
		public Panel FriendlyTeamBar { get; set; }
		// @ref
		public Label FriendlyTeamName { get; set; }
		// @ref
		public Panel EnemyTeamBar { get; set; }
		// @ref
		public Label EnemyTeamName { get; set; }

		public TeamStatus()
		{

		}

		protected float MaxScore => 1000;

		protected void SetScore( TeamSystem.Team team )
		{
			var isMyTeam = team == TeamSystem.MyTeam;
			var bar = isMyTeam ? FriendlyTeamBar : EnemyTeamBar;
			var label = isMyTeam ? FriendlyTeamName : EnemyTeamName;
			var teamName = TeamSystem.GetTeamName( team );

			var score = Game.Current.Scores.GetScore( team );

			float percent = ( score / MaxScore ) * 100;
			bar.Style.Width = Length.Percent( percent );

			if ( !isMyTeam )
			{
				bar.Style.Left = Length.Percent( 100 - percent );
			}

			bar.Style.Dirty();

			label.Text = $"{score:f0}";

			if ( isMyTeam )
				label.Text = $"{teamName} {label.Text}";
			else
				label.Text = $"{label.Text} {teamName}";
		}

		public override void Tick()
		{
			base.Tick();

			SetScore( TeamSystem.Team.BLUFOR );
			SetScore( TeamSystem.Team.OPFOR );
		}
	}
}
