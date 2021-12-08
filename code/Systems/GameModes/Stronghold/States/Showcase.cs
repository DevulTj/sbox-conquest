
using Sandbox;
using System;
using System.Collections.Generic;

namespace Conquest.Stronghold;

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

	public int FocusPointLength = 2;


	private Vector3 TargetPos;
	private Vector3 CurrentPos;
	private Angles CurrentAngles;

	private TimeSince SinceSwitched = 0;
	private int Index = 0;

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

	private void FocusNextPoint()
	{
		Index++;

		if ( Index > FocusPoints.Count - 1 )
			return;

		TargetPos = FocusPoints[Index];
		SinceSwitched = 0;
	}
}

public partial class ShowcaseGameState : GameState
{
	public override string Identifier => "Showcase";

	public override int TimeLimit => 10;

	public override void OnStart( Conquest.GameState oldGameState = null )
	{
		base.OnStart( oldGameState );

		Camera ??= new ShowcaseCamera();

		Log.Info( $"{(Host.IsServer ? "[Server]" : "[Client]")} Creating showcase camera" );
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
