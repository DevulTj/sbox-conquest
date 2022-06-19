using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest;

public partial class Game : Sandbox.GameBase, IGameStateAddressable
{
	[Net]
	public TeamScores Scores { get; set; }

	[Net]
	public SquadManager SquadManager { get; set; }

	public Game()
	{
		Current = this;
		Transmit = TransmitType.Always;

		Global.TickRate = 20;

		if ( Host.IsServer )
		{
			Scores = new();
			SquadManager = new();
		}
		else if ( Host.IsClient )
		{
			InitPostProcess();
		}
	}

	// @TODO: Move this entire thing into its own system
	public StandardPostProcess StandardPostProcess { get; set; }

	private float distanceLerp = 0f;
	protected void PushGlobalPPSettings()
	{
		var pp = StandardPostProcess;
		pp.ChromaticAberration.Enabled = true;
		pp.ChromaticAberration.Offset = new Vector3( -0.0007f, -0.0007f, 0f );

		pp.MotionBlur.Enabled = true;
		pp.MotionBlur.Scale = 0.05f;
		pp.MotionBlur.Samples = 5;

		pp.Vignette.Enabled = true;
		pp.Vignette.Color = Color.Black;
		pp.Vignette.Roundness = 1.5f;
		pp.Vignette.Intensity = 1f;

		pp.Saturate.Enabled = true;
		pp.Saturate.Amount = 0.95f;

		pp.FilmGrain.Enabled = true;
		pp.FilmGrain.Intensity = 0.1f;

		pp.ColorOverlay.Enabled = false;
		//pp.ColorOverlay.Amount = 0.1f;
		//pp.ColorOverlay.Color = new Color( 0.1f, 0.1f, 0.2f );
		//pp.ColorOverlay.Mode = StandardPostProcess.ColorOverlaySettings.OverlayMode.Additive;
	}

	protected void InitPostProcess()
	{
		StandardPostProcess = new();

		PostProcess.Add( StandardPostProcess );
	}

	public override void ClientJoined( Client cl )
	{
		var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
		teamComponent.Team = teamComponent.Team.GetLowestCount();

		// Set up the player in a squad.
		SquadManager.Assign( cl );

		BasePlayer player = cl.IsBot ? new AIPlayer( cl ) : new SpectatorPlayer( cl );
		cl.Pawn = player;

		Log.Info( $"\"{cl.Name}\" has joined the game" );
		ChatBox.AddInformation( To.Everyone, $"{cl.Name} has joined", $"avatar:{cl.PlayerId}" );

		player.Respawn();
	}

	public static Game Current { get; protected set; }

	/// <summary>
	/// Called when the game is shutting down
	/// </summary>
	public override void Shutdown()
	{
		if ( Current == this )
			Current = null;
	}

	/// <summary>
	/// Client has disconnected from the server. Remove their entities etc.
	/// </summary>
	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		Log.Info( $"\"{cl.Name}\" has left the game ({reason})" );
		ChatBox.AddInformation( To.Everyone, $"{cl.Name} has left ({reason})", $"avatar:{cl.PlayerId}" );

		if ( cl.Pawn.IsValid() )
		{
			cl.Pawn.Delete();
			cl.Pawn = null;
		}

		// Remove the player from squad
		SquadManager.Clear( cl );
	}

	/// <summary>
	/// Called each tick.
	/// Serverside: Called for each client every tick
	/// Clientside: Called for each tick for local client. Can be called multiple times per tick.
	/// </summary>
	public override void Simulate( Client cl )
	{
		if ( !cl.Pawn.IsValid() ) return;

		// Block Simulate from running clientside
		// if we're not predictable.
		if ( !cl.Pawn.IsAuthority ) return;

		cl.Pawn.Simulate( cl );
	}

	/// <summary>
	/// Called each frame on the client only to simulate things that need to be updated every frame. An example
	/// of this would be updating their local pawn's look rotation so it updates smoothly instead of at tick rate.
	/// </summary>
	public override void FrameSimulate( Client cl )
	{
		Host.AssertClient();

		if ( !cl.Pawn.IsValid() ) return;

		// Block Simulate from running clientside
		// if we're not predictable.
		if ( !cl.Pawn.IsAuthority ) return;

		cl.Pawn?.FrameSimulate( cl );

		PushGlobalPPSettings();
	}

	/// <summary>
	/// Should we send voice data to this player
	/// </summary>
	public override bool CanHearPlayerVoice( Client source, Client dest )
	{
		Host.AssertServer();

		var sp = source.Pawn;
		var dp = dest.Pawn;

		if ( sp == null || dp == null ) return false;
		if ( sp.Position.Distance( dp.Position ) > 1000 ) return false;

		return true;
	}

	/// <summary>
	/// Which camera should we be rendering from?
	/// </summary>
	public virtual CameraMode FindActiveCamera()
	{
		// Priority 1 - DevCam
		var devCam = Local.Client.Components.Get<DevCamera>();
		if ( devCam != null ) return devCam;

		// Priority 2 - GameState
		if ( GameState.Current.HasCamera )
			return GameState.Current.Camera;
		
		var clientCam = Local.Client.Components.Get<CameraMode>();
		if ( clientCam != null ) return clientCam;

		// Priority 4 - Pawn
		var pawnCam = Local.Pawn?.Components.Get<CameraMode>();
		if ( pawnCam != null ) return pawnCam;

		return null;
	}

	/// <summary>
	/// Player typed kill in the console. Override if you don't want players
	/// to be allowed to kill themselves.
	/// </summary>
	public virtual void DoPlayerSuicide( Client cl )
	{
		if ( cl.Pawn == null ) return;

		cl.Pawn.TakeDamage( DamageInfo.Generic( 1000 ) );
	}

	/// <summary>
	/// Player typed noclip in the console.
	/// </summary>
	public virtual void DoPlayerNoclip( Client player )
	{
		if ( !player.HasPermission( "noclip" ) )
			return;

		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				Log.Info( "Noclip Mode Off" );
				basePlayer.DevController = null;
			}
			else
			{
				Log.Info( "Noclip Mode On" );
				basePlayer.DevController = new NoclipController();
			}
		}
	}

	[Predicted]
	public CameraMode LastCamera { get; set; }

	protected float TargetFieldOfView = 90;
	public virtual float CalculateFOV( float ResultFOV )
	{
		TargetFieldOfView = TargetFieldOfView.LerpTo( ResultFOV, Time.Delta * 5f );
		return TargetFieldOfView;
	}

	/// <summary>
	/// Called to set the camera up, clientside only.
	/// </summary>
	public override CameraSetup BuildCamera( CameraSetup camSetup )
	{
		if ( RespawnScreen.State != TransitionState.None )
		{
			var camera = RespawnScreen.CameraSetup;
			camera.Position = RespawnScreen.Position;
			camera.Rotation = RespawnScreen.Rotation;
			camera.FieldOfView = CalculateFOV( camera.FieldOfView );

			return camera;
		}

		var cam = FindActiveCamera();
		if ( LastCamera != cam )
		{
			LastCamera?.Deactivated();
			LastCamera = cam as CameraMode;
			LastCamera?.Activated();
		}

		cam?.Build( ref camSetup );
		PostCameraSetup( ref camSetup );

		camSetup.FieldOfView = CalculateFOV( camSetup.FieldOfView );

		return camSetup;
	}

	/// <summary>
	/// Clientside only. Called every frame to process the input.
	/// The results of this input are encoded\ into a user command and
	/// passed to the PlayerController both clientside and serverside.
	/// This routine is mainly responsible for taking input from mouse/controller
	/// and building look angles and move direction.
	/// </summary>
	public override void BuildInput( InputBuilder input )
	{
		Event.Run( "buildinput", input );

		// the camera is the primary method here
		LastCamera?.BuildInput( input );

		Local.Pawn?.BuildInput( input );
	}

	/// <summary>
	/// Called after the camera setup logic has run. Allow the gamemode to 
	/// do stuff to the camera, or using the camera. Such as positioning entities 
	/// relative to it, like viewmodels etc.
	/// </summary>
	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		if ( Local.Pawn != null )
		{
			// VR anchor default is at the pawn's location
			VR.Anchor = Local.Pawn.Transform;

			Local.Pawn.PostCameraSetup( ref camSetup );
		}

		//
		// Position any viewmodels
		//
		BaseViewModel.UpdateAllPostCamera( ref camSetup );

		// @TODO: Camera mod system
		// CameraModifier.Apply( ref camSetup );
	}

	/// <summary>
	/// Called right after the level is loaded and all entities are spawned.
	/// </summary>
	public override void PostLevelLoaded()
	{
		_ = Scores.StartTicking();
		
		// Create gamemode manager
		_ = new GameModeManager();
	}

	/// <summary>
	/// Someone is speaking via voice chat. This might be someone in your game, 
	/// or in your party, or in your lobby.
	/// </summary>
	public override void OnVoicePlayed( long steamId, float level )
	{
		VoiceList.Current?.OnVoicePlayed( steamId, level );
	}

	[ConCmd.Server( "kill", Help = "Kills the calling player with generic damage" )]
	public static void KillCommand()
	{
		var target = ConsoleSystem.Caller;
		if ( target == null ) return;

		Current.DoPlayerSuicide( target );
	}

	protected void ResetStats( Client cl )
	{
		cl.SetInt( "score", 0 );
		cl.SetInt( "frags", 0 );
		cl.SetInt( "deaths", 0 );
		cl.SetInt( "captures", 0 );
	}

	void IGameStateAddressable.ResetState()
	{
		Client.All.ToList().ForEach( x => ResetStats( x ) );
		Entity.All.ToList().ForEach( x => x.RemoveAllDecals() );
	}
}
