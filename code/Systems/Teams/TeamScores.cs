
using Sandbox;
using System;
using System.Collections.Generic;

namespace Conquest
{
	public partial class TeamScores : BaseNetworkable, INetworkSerializer
	{
		public virtual int MinimumScore => 0;
		public virtual int MaximumScore => 1000;
		protected static int ArraySize => Enum.GetNames( typeof( TeamSystem.Team ) ).Length;
		protected int[] Scores { get; set; } = new int[ArraySize];

		public void SetScore( TeamSystem.Team team, int score )
		{
			Scores[(int)team] = Math.Clamp( score, MinimumScore, MaximumScore ); 
			WriteNetworkData();
		}

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
			Scores = new int[ ArraySize ];

			int count = read.Read<int>();
			for ( int i = 0; i < count; i++ )
				Scores[ i ] = read.Read<int>();
		}

		public void Write( NetWrite write )
		{
			write.Write( Scores.Length );

			foreach( var score in Scores )
				write.Write( score );
		}
	}
}
