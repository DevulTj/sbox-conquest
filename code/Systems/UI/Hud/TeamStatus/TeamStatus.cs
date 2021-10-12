
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{

	[UseTemplate("systems/ui/hud/teamstatus/teamstatus.html")]
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
		// @ref
		public Label FriendlyState { get; set; }
		// @ref
		public Label EnemyState { get; set; }

		public TeamStatus()
		{
			SetScore( TeamSystem.Team.BLUFOR );
			SetScore( TeamSystem.Team.OPFOR );
		}

		protected float MaxScore => 1000;

		[GameEvent.Shared.OnScoreChanged]
		protected void OnScoreChanged()
		{
			SetScore( TeamSystem.Team.BLUFOR );
			SetScore( TeamSystem.Team.OPFOR );
		}

		protected void SetScore( TeamSystem.Team team )
		{
			var isMyTeam = team == TeamSystem.MyTeam;
			
			var bar = isMyTeam ? FriendlyTeamBar : EnemyTeamBar;
			var label = isMyTeam ? FriendlyTeamName : EnemyTeamName;
			var state = isMyTeam ? FriendlyState : EnemyState;

			var teamName = TeamSystem.GetTeamName( team );
			
			var oldScore = Game.Current.Scores.GetOldScore( team );
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

			if ( oldScore is not null && oldScore > score )
			{
				_ = state.AddTimedClass( "show", 5 );
			}
		}
	}
}
