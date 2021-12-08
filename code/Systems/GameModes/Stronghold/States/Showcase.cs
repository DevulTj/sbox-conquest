
using Sandbox;

namespace Conquest.Stronghold;

public partial class ShowcaseCamera : Camera
{
	public virtual Vector3 StartingCameraPosition => new Vector3( 1162.81f, -216.94f, 109.65f );
	public virtual Angles StartingCameraAngles => new Angles( 11.40f, -25.65f, 0f );

	public override void Update()
	{
		Position = StartingCameraPosition;
		Rotation = StartingCameraAngles.ToRotation();
		FieldOfView = 60f;
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
