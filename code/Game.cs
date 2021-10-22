using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class Game : Sandbox.GameBase
	{
		[Net]
		public TeamScores Scores { get; set; }

		public Game()
		{
			Current = this;
			Transmit = TransmitType.Always;

			if ( Host.IsServer )
			{
				Scores = new();

				// @Temporary
				//// Will be map controlled ents.
				//var a = new CapturePointEntity();
				//a.Position = new Vector3( -2428.1f, -3192.38f, 0.03f );
				//a.Identity = "A";

				//var b = new CapturePointEntity();
				//b.Position = new Vector3( 1004.61f, -1612.63f, -139.97f );
				//b.Identity = "B";

				//var c = new CapturePointEntity();
				//c.Position = new Vector3( -697.96f, 200.86f, 5.04f );
				//c.Identity = "C";


				//var d = new CapturePointEntity();
				//d.Position = new Vector3( -4285.88f, 117.18f, 56.03f );
				//d.Identity = "D";

				//var e = new CapturePointEntity();
				//e.Position = new Vector3( 2023.55f, 818.86f, -124.97f );
				//e.Identity = "E";


				//var bluforHQ = new Headquarters();
				//bluforHQ.Position = new Vector3( 1037.79f, -3616.86f, -139.97f );
				//bluforHQ.Team = Team.BLUFOR;

				//var opforHQ = new Headquarters();
				//opforHQ.Position = new Vector3( -3879.69f, 1349.77f, 0.04f );
				//opforHQ.Team = Team.OPFOR;
			}
		}

		public override void ClientJoined( Client cl )
		{
			var teamComponent = cl.Components.GetOrCreate<TeamComponent>();
			teamComponent.Team = teamComponent.Team.GetLowestCount();

			BasePlayer player = cl.IsBot ? new Player( cl ) : new SpectatorPlayer( cl );
			cl.Pawn = player;

			Log.Info( $"\"{cl.Name}\" has joined the game" );
			ChatBox.AddInformation( To.Everyone, $"{cl.Name} has joined", $"avatar:{cl.SteamId}" );

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
			ChatBox.AddInformation( To.Everyone, $"{cl.Name} has left ({reason})", $"avatar:{cl.SteamId}" );

			if ( cl.Pawn.IsValid() )
			{
				cl.Pawn.Delete();
				cl.Pawn = null;
			}

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
		public virtual ICamera FindActiveCamera()
		{
			if ( Local.Client.DevCamera != null ) return Local.Client.DevCamera;
			if ( Local.Client.Camera != null ) return Local.Client.Camera;
			if ( Local.Pawn != null ) return Local.Pawn.Camera;

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

		/// <summary>
		/// The player wants to enable the devcam. Probably shouldn't allow this
		/// unless you're in a sandbox mode or they're a dev.
		/// </summary>
		public virtual void DoPlayerDevCam( Client player )
		{
			Host.AssertServer();

			if ( !player.HasPermission( "devcam" ) )
				return;

			player.DevCamera = player.DevCamera == null ? new DevCamera() : null;
		}

		[Predicted]
		public Camera LastCamera { get; set; }

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
				LastCamera = cam as Camera;
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

			CameraModifier.Apply( ref camSetup );
		}

		/// <summary>
		/// Called right after the level is loaded and all entities are spawned.
		/// </summary>
		public override void PostLevelLoaded()
		{
			_ = Scores.StartTicking();

			if ( Global.MapName == "facepunch.construct" )
			{
				var a = new CapturePointEntity();
				a.Position = new Vector3( -2428.1f, -3192.38f, 0.03f );
				a.Identity = "A";

				var b = new CapturePointEntity();
				b.Position = new Vector3( 1004.61f, -1612.63f, -139.97f );
				b.Identity = "B";

				var c = new CapturePointEntity();
				c.Position = new Vector3( -697.96f, 200.86f, 5.04f );
				c.Identity = "C";


				var d = new CapturePointEntity();
				d.Position = new Vector3( -4285.88f, 117.18f, 56.03f );
				d.Identity = "D";

				var e = new CapturePointEntity();
				e.Position = new Vector3( 2023.55f, 818.86f, -124.97f );
				e.Identity = "E";


				var bluforHQ = new Headquarters();
				bluforHQ.Position = new Vector3( 1037.79f, -3616.86f, -139.97f );
				bluforHQ.Team = Team.BLUFOR;

				var opforHQ = new Headquarters();
				opforHQ.Position = new Vector3( -3879.69f, 1349.77f, 0.04f );
				opforHQ.Team = Team.OPFOR;
			}
		}

		/// <summary>
		/// Someone is speaking via voice chat. This might be someone in your game, 
		/// or in your party, or in your lobby.
		/// </summary>
		public override void OnVoicePlayed( ulong steamId, float level )
		{
			VoiceList.Current?.OnVoicePlayed( steamId, level );
		}

		[ServerCmd( "kill", Help = "Kills the calling player with generic damage" )]
		public static void KillCommand()
		{
			var target = ConsoleSystem.Caller;
			if ( target == null ) return;

			Current.DoPlayerSuicide( target );
		}


		[AdminCmd( "conquest_restartgame", Help = "Restarts the game state" )]
		public static void RestartGame()
		{
			ChatBox.AddInformation( To.Everyone, $"The game has begun First team to hit 0 tickets loses." );

			Current.Scores.Reset();

			var ents = Entity.All.OfType<IGameStateAddressable>().ToList();
			foreach ( var entity in ents )
			{
				entity.ResetState();
			}
		}

		protected async Task DelayedRestart()
		{
			await GameTask.DelayRealtimeSeconds( 10f );

			RestartGame();
		}

		[AdminCmd( "conquest_endgame", Help = "Ends the game, and restarts it after some time" )]
		public static void EndGame( Team winner = Team.Unassigned )
		{
			ChatBox.AddInformation( To.Everyone, $"The game has ended. It'll restart in 10 seconds." );

			if ( winner != Team.Unassigned )
				ChatBox.AddInformation( To.Everyone, $"The winners: " + TeamSystem.GetTeamName( winner ) );

			_ = Current.DelayedRestart();
		}

		[GameEvent.Server.ScoreHitZero]
		protected void ScoreHitZero( Team winner )
		{
			EndGame( winner );
		}
	}
}
