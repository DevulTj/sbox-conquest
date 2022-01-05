using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest;

public enum AIState
{
	GoingToPoint,
	EngagingContact,
	Shooting,
	Wandering,
	Idle
}

public partial class AIPlayer : Player
{
	public AIPlayer() : base()
	{
	}

	public AIPlayer( Client cl ) : base( cl )
	{
	}

	[ConVar.Replicated( "conquest_ai_zombie" )]
	public static bool Zombie { get; set; } = false;

	Vector3 InputVelocity;
	Vector3 LookDir;

	public NavSteer Steer;
	public AIState State { get; set; } = AIState.Idle;
	public float SeekRadius { get; set; } = 2048;
	public Player TargetPlayer { get; set; }

	// This is all shit, but testing.
	public AIState DecideState()
	{
		AIState state = AIState.Wandering;

		List<Player> foundPlayers = new();

		foreach ( var ent in Physics.GetEntitiesInSphere( Position, SeekRadius ) )
		{
			if ( ent is Player player && player.LifeState == LifeState.Alive )
			{
				if ( TeamSystem.IsHostile( Team, player.Team ) )
				{
					foundPlayers.Add( player );
					break;
				}
			}
		}

		if ( foundPlayers.Count > 0 )
		{
			state = AIState.EngagingContact;
			TargetPlayer = foundPlayers.OrderBy( x => x.Position.Distance( Position ) ).First();

			LookDir = Vector3.Lerp( LookDir, InputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );

			var tr = Trace.Ray( EyePos, TargetPlayer.Position ).WithoutTags( "flyby" ).Ignore( this ).Run();

			if ( Position.Distance( TargetPlayer.Position ) < 1024f && tr.Entity == TargetPlayer )
			{
				// We are shooting the player
				// @TODO: Actually shoot the player
				state = AIState.Shooting;

				Steer = null;

				var weapon = ActiveChild as Carriable;
				if ( weapon is not null )
					weapon.SetWantsToShoot( true );

				var animHelper = new CitizenAnimationHelper( this );

				animHelper.WithLookAt( TargetPlayer.Position );
				animHelper.WithVelocity( Velocity );
				animHelper.WithWishVelocity( InputVelocity );

				var targetDelta = (TargetPlayer.Position - Position);
				var targetDistance = targetDelta.Length;
				var targetDirection = targetDelta.Normal;

				// should be a helper func
				EyeRot = Rotation.From( new Angles(
					((float)Math.Asin( targetDirection.z )).RadianToDegree() * -1.0f,
					((float)Math.Atan2( targetDirection.y, targetDirection.x )).RadianToDegree(),
					0.0f ) );
			}
			else
			{
				TickEngageContact();
			}
		}
		else
		{
			TargetPlayer = null;

			var capturePoint = All.OfType<CapturePointEntity>()
				.Where( x => TeamSystem.GetFriendState( Team, x.Team ) != TeamSystem.FriendlyStatus.Friendly )
				.OrderBy( x => x.Position.Distance( Position ) )
				.FirstOrDefault();

			if ( capturePoint is not null )
			{
				state = AIState.GoingToPoint;

				Steer = new NavSteer();
				Steer.Target = capturePoint.Position;

				var animHelper = new CitizenAnimationHelper( this );

				animHelper.WithLookAt( EyePos + LookDir );
				animHelper.WithVelocity( Velocity );
				animHelper.WithWishVelocity( InputVelocity );
			}
		}

		return state;
	}

	protected void TickEngageContact()
	{
		Steer = new NavSteer();
		Steer.Target = TargetPlayer.Position;

		var animHelper = new CitizenAnimationHelper( this );

		animHelper.WithLookAt( TargetPlayer.Position );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( InputVelocity );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		InputVelocity = 0;

		if ( Zombie )
			return;

		DecideState();

		if ( Steer != null )
		{
			Steer.Tick( Position );

			if ( !Steer.Output.Finished )
			{
				InputVelocity = Steer.Output.Direction.Normal;

				var controller = Controller as WalkController;
				if ( controller != null )
					Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 500, controller.GetWishSpeed() );
			}
		}

		Move( Time.Delta );
			
		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 0.5f )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
		}
	}

	protected virtual void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 64, 4 );
		//DebugOverlay.Box( Position, bbox.Mins, bbox.Maxs, Color.Green );

		MoveHelper move = new( Position, Velocity );
		move.MaxStandableAngle = 50;
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
				move.TryUnstuck();
				move.TryMoveWithStep( timeDelta, 30 );
		}

		var tr = move.TraceDirection( Vector3.Down * 10.0f );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
			{
				move.Position = tr.EndPos;
			}

			if ( InputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity = move.Velocity - movement * InputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * InputVelocity.Normal;

			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
			}
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
			Sandbox.Debug.Draw.Once.WithColor( Color.Red ).Circle( Position, Vector3.Up, 10.0f );
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}
}
