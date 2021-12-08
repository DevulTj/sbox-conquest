
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest.Stronghold;

public struct ShowcasePlayerData
{
	public string Name;
	public long PlayerId;
	public int Score;

	public ShowcasePlayerData( string name, long playerid, int score )
	{
		Name = name;
		PlayerId = playerid;
		Score = score;
	}
}

public partial class ShowcaseCamera : Camera
{
	public ShowcaseCamera()
	{
		CurrentPos = StartingCameraPosition;
		TargetPos = StartingCameraPosition;
	}

	public virtual Vector3 StartingCameraPosition => new Vector3( 1162.81f, -216.94f, 109.65f );
	public virtual Angles StartingCameraAngles => new Angles( 11.40f, -25.65f, 0f );

	public List<Vector3> FocusPoints => new List<Vector3>() { StartingCameraPosition + new Vector3( 10, 10, 0 ), StartingCameraPosition + new Vector3( 20, 20, 0 ), StartingCameraPosition + new Vector3( 30, 30, 0 ), StartingCameraPosition + new Vector3( 40, 40, 0 ) };

	public float FocusPointLength = 2;

	private Vector3 TargetPos;
	private Vector3 CurrentPos;
	private Angles CurrentAngles;

	private TimeSince SinceSwitched = 0;
	private int Index = 0;

	public List<ShowcasePlayerData> Players = new();
	public ShowcasePlayerData? CurrentPlayer = null;

	public override void Update()
	{
		CurrentPos = CurrentPos.LerpTo( TargetPos, Time.Delta * 10f );

		Position = CurrentPos;
		Rotation = StartingCameraAngles.ToRotation();

		FieldOfView = 60f;

		if ( SinceSwitched > FocusPointLength )
		{
			FocusNextPoint();
		}
	}

	public void FocusIndex( int index )
	{
		Index = index;

		TargetPos = FocusPoints[Index];
		SinceSwitched = 0;

		CurrentPlayer = Players[Index];

		Log.Info( $"Focusing on player: {CurrentPlayer.GetValueOrDefault().Name}" );
	}

	private void FocusNextPoint()
	{
		Index++;

		if ( Index > Players.Count - 1 )
			return;

		FocusIndex( Index );
	}
}

public partial class ShowcaseGameState : GameState
{
	[Net] public Team WinningTeam { get; set; } = Team.Unassigned;

	public override string Identifier => "Showcase";

	public override bool CanDeploy => false;

	public override int TimeLimit => 10;

	public override void OnStart( Conquest.GameState oldGameState = null )
	{
		base.OnStart( oldGameState );

		ShowcaseCamera cam = Camera as ShowcaseCamera;
		if ( cam == null )
		{
			cam = new();
			Camera = cam;
		}

		SetupData( cam );

		Log.Info( $"{(Host.IsServer ? "[Server]" : "[Client]")} Creating showcase camera" );
	}

	[AdminCmd( "conquest_debug_setshowcase" )]
	public static void SetShowcase()
	{
		var gs = new ShowcaseGameState();
		gs.WinningTeam = Team.BLUFOR;
		GameState.Current.GameMode.SetGameState( gs );
	}


	private void SetupData( ShowcaseCamera cam )
	{
		var orderedWinners = Client.All
			.Where( x => TeamSystem.GetTeam( x ) == WinningTeam )
			.OrderBy( x => x.GetInt( "score" ) ).ToList();

		var max = MathX.Clamp( orderedWinners.Count, 1, 5 );

		Log.Info( $"there are {max} players" );

		for ( int i = 0; i < max; i++ )
		{
			var client = orderedWinners[i];
			var score = client.GetInt( "score" );

			ShowcasePlayerData player = new( client.Name, client.PlayerId, score );

			cam.Players.Add( player );
		}

		cam.FocusPointLength = TimeLimit / max;
		cam.FocusIndex( 0 );
	}

	public override void OnEnd( Conquest.GameState newGameState = null )
	{
		base.OnEnd( newGameState );
	}

	protected override void OnTimeLimitReached()
	{
		base.OnTimeLimitReached();

		GameMode.SetGameState( new WaitingForPlayersGameState() );
	}
}
