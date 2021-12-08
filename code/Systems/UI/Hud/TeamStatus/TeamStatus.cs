
using Sandbox.UI;

namespace Conquest;

[UseTemplate( "systems/ui/hud/teamstatus/teamstatus.html" )]
public class TeamStatus : Panel
{
	// @ref
	public Panel FriendlyTeamBar { get; set; }
	public Label FriendlyTeamName { get; set; }
	public Panel EnemyTeamBar { get; set; }
	public Label EnemyTeamName { get; set; }
	public Label FriendlyState { get; set; }
	public Label FriendlyScore { get; set; }
	public Label EnemyScore { get; set; }
	public Label EnemyState { get; set; }
	// -@ref

	public string TimeLeft { get { return GameState.Current.FormattedTimeRemaining; } }

	public TeamStatus()
	{
	}

	protected float MaxScore => TeamScores.MaximumScore;

	[GameEvent.Shared.OnScoreChanged, GameEvent.Client.OnGameStateChanged]
	protected void OnScoreChanged()
	{
		UpdateScore( Team.BLUFOR );
		UpdateScore( Team.OPFOR );
	}

	protected void UpdateScore( Team team )
	{
		var isMyTeam = team == TeamSystem.MyTeam;
			
		var bar = isMyTeam ? FriendlyTeamBar : EnemyTeamBar;
		//var label = isMyTeam ? FriendlyTeamName : EnemyTeamName;
		var scoreLabel = isMyTeam ? FriendlyScore : EnemyScore;
		var state = isMyTeam ? FriendlyState : EnemyState;

		var teamName = TeamSystem.GetTeamName( team );
			
		var oldScore = Game.Current.Scores.GetOldScore( team );
		var score = Game.Current.Scores.GetScore( team );

		float percent = ( score / MaxScore ) * 100;
		bar.Style.Width = Length.Percent( percent );

		if ( !isMyTeam )
			bar.Style.Left = Length.Percent( 100 - percent );

		//label.Text = $"{teamName}";
		scoreLabel.Text = $"{score:f0}";

		if ( oldScore is not null && oldScore > score )
		{
			_ = state.AddTimedClass( "show", 5 );
		}
	}
}
