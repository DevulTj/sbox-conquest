
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class TeamScores : BaseNetworkable, INetworkSerializer
	{
		public TeamScores()
		{
			Scores = new int[ ArraySize ];

			// Set initializing scores.
			SetScore( TeamSystem.Team.BLUFOR, MaximumScore );
			SetScore( TeamSystem.Team.OPFOR, MaximumScore );
		}

		public virtual int TickInterval => 10;
		public virtual int MinimumScore => 0;

		[ConVar.Replicated( "conquest_maxscore" )]
		public static int MaximumScore { get; set; } = 250;

		protected static int ArraySize => Enum.GetNames( typeof( TeamSystem.Team ) ).Length;
		protected int[] Scores { get; set; }

		protected int[] OldScores { get; set; }

		// Initialized by Game
		public async Task StartTicking()
		{
			while ( true )
			{
				await GameTask.DelaySeconds( TickInterval );

				Tick();
			}
		}

		// Fucking hate this. @TODO: Do better.
		protected virtual TeamSystem.Team GetOpposingTeam( TeamSystem.Team team )
		{
			if ( team == TeamSystem.Team.BLUFOR )
				return TeamSystem.Team.OPFOR;

			if ( team == TeamSystem.Team.OPFOR )
				return TeamSystem.Team.BLUFOR;

			return TeamSystem.Team.Unassigned;
		}

		protected virtual void Tick()
		{
			foreach( var capturePoint in Entity.All.OfType<CapturePointEntity>() )
			{
				var otherTeam = GetOpposingTeam( capturePoint.Team );

				if ( otherTeam == TeamSystem.Team.Unassigned )
					continue;

				RemoveScore( otherTeam, 1 );
			}
		}

		public void SetScore( TeamSystem.Team team, int score )
		{
			Scores[(int)team] = Math.Clamp( score, MinimumScore, MaximumScore ); 
			WriteNetworkData();
		}

		public int? GetOldScore( TeamSystem.Team team ) => OldScores?[(int)team];

		public int GetScore( TeamSystem.Team team )
		{
			return Scores[(int)team];
		}

		public void AddScore( TeamSystem.Team team, int score )
		{
			SetScore( team, GetScore( team ) + score );
		}

		public void RemoveScore( TeamSystem.Team team, int score )
		{
			SetScore( team, GetScore( team ) - score );
		}

		public void Read( NetRead read )
		{
			OldScores = Scores?.ToArray();

			Scores = new int[ ArraySize ];

			int count = read.Read<int>();
			for ( int i = 0; i < count; i++ )
				Scores[ i ] = read.Read<int>();

			Event.Run( GameEvent.Shared.OnScoreChanged );
		}

		public void Write( NetWrite write )
		{
			write.Write( Scores.Length );

			foreach( var score in Scores )
				write.Write( score );
		}
	}
}
