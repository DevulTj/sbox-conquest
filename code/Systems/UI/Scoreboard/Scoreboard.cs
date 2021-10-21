using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Conquest
{
	public class Scoreboard : Panel
	{
		public struct TeamSection
		{
			public Label TeamName;
			public Panel TeamIcon;
			public Panel TeamContainer;
			public Panel Header;
			public Panel TeamHeader;
			public Label TeamTickets;
			public Panel Canvas;
		}

		public Dictionary<Client, ScoreboardEntry> Rows = new();
		public Dictionary<TeamSystem.Team, TeamSection> TeamSections = new();

		public Scoreboard()
		{
			StyleSheet.Load( "systems/ui/scoreboard/scoreboard.scss" );

			AddClass( "scoreboard" );

			AddTeamHeader( TeamSystem.Team.BLUFOR );
			AddTeamHeader( TeamSystem.Team.OPFOR );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Input.Down( InputButton.Score ) );

			if ( !IsVisible )
				return;

			foreach ( var client in Client.All.Except( Rows.Keys ) )
			{
				var entry = AddClient( client );
				Rows[client] = entry;
			}

			foreach ( var client in Rows.Keys.Except( Client.All ) )
			{
				if ( Rows.TryGetValue( client, out var row ) )
				{
					row?.Delete();
					Rows.Remove( client );
				}
			}

			foreach ( var kv in Rows )
			{
				CheckTeamIndex( kv.Value );
			}

			foreach ( var kv in TeamSections )
			{
				kv.Value.TeamTickets.Text = $"{Game.Current.Scores.GetScore( kv.Key )}";
			}
		}

		protected void AddTeamHeader( TeamSystem.Team team )
		{
			var section = new TeamSection
			{

			};

			section.TeamContainer = Add.Panel( "team" );
			section.TeamHeader = section.TeamContainer.Add.Panel( "team-header" );
			section.Header = section.TeamContainer.Add.Panel( "table-header" );
			section.Canvas = section.TeamContainer.Add.Panel( "canvas" );

			section.TeamIcon = section.TeamHeader.Add.Panel( "teamIcon" );
			section.TeamName = section.TeamHeader.Add.Label( TeamSystem.GetTeamName( team ), "teamName" );
			section.TeamTickets = section.TeamHeader.Add.Label( $"{Game.Current.Scores.GetScore( team )}", "teamTickets" );

			var hudClass = TeamSystem.GetTeamName( team );

			section.TeamIcon.AddClass( hudClass );

			section.Header.Add.Label( "NAME", "name" );
			section.Header.Add.Label( "CAPTURES", "captures" );
			section.Header.Add.Label( "KILLS", "kills" );
			section.Header.Add.Label( "DEATHS", "deaths" );
			section.Header.Add.Label( "PING", "ping" );

			section.Canvas.AddClass( hudClass );
			section.Header.AddClass( hudClass );
			section.TeamHeader.AddClass( hudClass );

			TeamSections[team] = section;
		}

		protected virtual ScoreboardEntry AddClient( Client entry )
		{
			var teamIndex = entry.Components.Get<TeamComponent>()?.Team ?? TeamSystem.Team.Unassigned;

			if ( !TeamSections.TryGetValue( teamIndex, out var section ) )
			{
				section = TeamSections[0];
			}

			var p = section.Canvas.AddChild<ScoreboardEntry>();
			p.Client = entry;
			return p;
		}

		private void CheckTeamIndex( ScoreboardEntry entry )
		{
			TeamSystem.Team currentTeamIndex = TeamSystem.Team.Unassigned;
			var teamIndex = entry.Client.Components.Get<TeamComponent>()?.Team ?? TeamSystem.Team.Unassigned;

			foreach ( var kv in TeamSections )
			{
				if ( kv.Value.Canvas == entry.Parent )
				{
					currentTeamIndex = kv.Key;
				}
			}

			if ( currentTeamIndex != teamIndex )
			{
				entry.Parent = TeamSections[teamIndex].Canvas;
			}
		}
	}

	public class ScoreboardEntry : Panel
	{
		public Client Client { get; set; }
		public Label PlayerName { get; set; }
		public Label Captures { get; set; }
		public Label Kills { get; set; }
		public Label Deaths { get; set; }
		public Label Ping { get; set; }

		private RealTimeSince TimeSinceUpdate { get; set; }

		public ScoreboardEntry()
		{
			AddClass( "entry" );

			PlayerName = Add.Label( "PlayerName", "name" );
			Captures = Add.Label( "", "captures" );
			Kills = Add.Label( "", "kills" );
			Deaths = Add.Label( "", "deaths" );
			Ping = Add.Label( "", "ping" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( !Client.IsValid() )
				return;

			if ( TimeSinceUpdate < 1f )
				return;

			TimeSinceUpdate = 0;
			UpdateData();
		}

		public virtual void UpdateData()
		{
			PlayerName.Text = Client.Name;
			Captures.Text = Client.GetInt( "captures" ).ToString();
			Kills.Text = Client.GetInt( "kills" ).ToString();
			Deaths.Text = Client.GetInt( "deaths" ).ToString();
			Ping.Text = Client.Ping.ToString();
			SetClass( "me", Client == Local.Client );
		}

		public virtual void UpdateFrom( Client client )
		{
			Client = client;
			UpdateData();
		}
	}
}
