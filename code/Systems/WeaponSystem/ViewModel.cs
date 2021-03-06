using Sandbox;
using System;

namespace Conquest;

public partial class ViewModel : BaseViewModel
{
	float walkBob = 0;
	Vector3 velocity;
	Vector3 acceleration;
	float MouseScale => .5f;
	float ReturnForce => 400f;
	float Damping => 18f;
	float AccelDamping => 0.1f;
	float PivotForce => 2000f;
	float VelocityScale => 10f;
	float RotationScale => 2.5f;
	float LookUpPitchScale => 10f;
	float LookUpSpeedScale => 10f;
	float NoiseSpeed => 0.8f;
	float NoiseScale => 20f;

	Vector3 WalkCycleOffsets => WeaponInfo.WalkCycleOffsets;
	float ForwardBobbing => WeaponInfo.ForwardBobbing;
	float SideWalkOffset => WeaponInfo.SideWalkOffset;
	public Vector3 AimOffset => WeaponInfo.AimOffset;
	Vector3 Offset => WeaponInfo.Offset;
	Vector3 CrouchOffset => WeaponInfo.CrouchOffset;
	float OffsetLerpAmount => WeaponInfo.OffsetLerpAmount;
	Vector3 SmoothedVelocity;
	float VelocityClamp => 3f;

	float noiseZ = 0;
	float noisePos = 0;
	float upDownOffset = 0;
	float avoidance = 0;

	float burstSprintLerp = 0;
	float sprintLerp = 0;
	float aimLerp = 0;

	float smoothedDelta = 0;

	float DeltaTime => smoothedDelta;

	public WeaponInfoAsset WeaponInfo { get; set; }

	public ViewModel()
	{
		noiseZ = Rand.Float( -10000, 10000 );
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );


		AddCameraEffects( ref camSetup );
	}

	private void SmoothDeltaTime()
	{
		var delta = (Time.Delta - smoothedDelta) * Time.Delta;
		var clamped = MathF.Min( MathF.Abs( delta ), 1 / 60f );

		smoothedDelta += clamped * MathF.Sign( delta );
	}

	protected float MouseDeltaLerpX;
	protected float MouseDeltaLerpY;
	private void AddCameraEffects( ref CameraSetup camSetup )
	{
		SmoothDeltaTime();

		if ( !Owner.IsValid() )
		{
			Delete();

			return;
		}

		SmoothedVelocity += (Owner.Velocity - SmoothedVelocity) * 5f * DeltaTime;

		var speed = Owner.Velocity.Length.LerpInverse( 0, 1000 );
		var bobSpeed = SmoothedVelocity.Length.LerpInverse( -100, 500 );
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;
		var forward = camSetup.Rotation.Forward;
		var owner = Owner as Player;
		var walkController = owner.Controller as WalkController;
		var avoidanceTrace = Trace.Ray( camSetup.Position, camSetup.Position + forward * 50f )
						.UseHitboxes()
						.Ignore( Owner )
						.Ignore( this )
						.Run();

		var sprint = owner.IsSprinting;
		var burstSprint = owner.IsBurstSprinting;
		var aim = owner.IsAiming;

		LerpTowards( ref avoidance, avoidanceTrace.Hit ? (1f - avoidanceTrace.Fraction) : 0, 10f );
		LerpTowards( ref sprintLerp, sprint && !burstSprint ? 1 : 0, 10f );
		LerpTowards( ref burstSprintLerp, burstSprint ? 1 : 0, 8f );

		LerpTowards( ref aimLerp, aim ? 1 : 0, 7f );
		//LerpTowards( ref upDownOffset, speed * -LookUpSpeedScale + camSetup.Rotation.Forward.z * -LookUpPitchScale, LookUpPitchScale );

		camSetup.FieldOfView = 70f * (1 - aimLerp) + 50f * aimLerp;
		camSetup.FieldOfView -= burstSprintLerp * 10f;

		bobSpeed *= (1 - sprintLerp * 0.25f);
		bobSpeed *= (1 - burstSprintLerp * 0.15f);

		var controller = owner.Controller as WalkController;

		if ( Owner.GroundEntity != null && controller is not null && !controller.Slide.IsActive )
		{
			walkBob += Time.Delta * 30.0f * bobSpeed;
		}

		if ( Owner.Velocity.Length < 60 )
		{
			var step = MathF.Round( walkBob / 90 );

			walkBob += (step * 90 - walkBob) * 10f * Time.Delta;
		}

		if ( walkController?.Duck?.IsActive == true )
		{
			acceleration += CrouchOffset * DeltaTime * (1 - aimLerp);
		}

		walkBob %= 360;

		noisePos += DeltaTime * NoiseSpeed;

		var mouseDeltaX = -Input.MouseDelta.x * DeltaTime * MouseScale;
		var mouseDeltaY = -Input.MouseDelta.y * DeltaTime * MouseScale;

		acceleration += Vector3.Left * mouseDeltaX * 0.5f * (1f - aimLerp * 2f);
		acceleration += Vector3.Up * mouseDeltaY * (1f - aimLerp * 2f);
		acceleration += -velocity * ReturnForce * DeltaTime;

		// Apply horizontal offsets based on walking direction
		var horizontalForwardBob = WalkCycle( 0.5f, 3f ) * speed * WalkCycleOffsets.x * DeltaTime;

		acceleration += forward.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Forward * ForwardBobbing * horizontalForwardBob;

		// Apply left bobbing and up/down bobbing
		acceleration += Vector3.Left * WalkCycle( 0.5f, 2f ) * speed * WalkCycleOffsets.y * (1 + sprintLerp) * DeltaTime;
		acceleration += Vector3.Up * WalkCycle( 0.5f, 2f, true ) * speed * WalkCycleOffsets.z * DeltaTime;

		acceleration += left.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Left * speed * SideWalkOffset * DeltaTime * (1 - aimLerp * 0.5f);

		velocity += acceleration * DeltaTime;

		ApplyDamping( ref acceleration, AccelDamping );

		ApplyDamping( ref velocity, Damping * (1 + aimLerp) );

		//acceleration += new Vector3(
		//	Noise.Perlin( noisePos, 0f, noiseZ ),
		//	Noise.Perlin( noisePos, 1f, noiseZ ),
		//	Noise.Perlin( noisePos, 0f, noiseZ )
		//) * NoiseScale * Time.Delta * (1 - aimLerp * 0.9f);

		velocity = velocity.Normal * Math.Clamp( velocity.Length, 0, VelocityClamp );

		Rotation desiredRotation = Local.Pawn.EyeRotation;
		desiredRotation *= Rotation.FromAxis( Vector3.Up, velocity.y * RotationScale * (1 - aimLerp) );
		desiredRotation *= Rotation.FromAxis( Vector3.Forward, -velocity.y * RotationScale * (1 - aimLerp * 0.0f) - 0f * (1 - aimLerp) );
		desiredRotation *= Rotation.FromAxis( Vector3.Right, velocity.z * RotationScale * (1 - aimLerp) );

		Rotation = desiredRotation;

		var desiredOffset = Vector3.Lerp( Offset, AimOffset, aimLerp );

		Position += forward * (velocity.x * VelocityScale + desiredOffset.x);
		Position += left * (velocity.y * VelocityScale + desiredOffset.y);
		Position += up * (velocity.z * VelocityScale + desiredOffset.z + upDownOffset * (1 - aimLerp));

		Position += (desiredRotation.Forward - Owner.EyeRotation.Forward) * -PivotForce;

		// Apply sprinting / avoidance offsets
		var offsetLerp = MathF.Max( sprintLerp, burstSprintLerp );

		Rotation *= Rotation.FromAxis( Vector3.Up, ( velocity.y * ( (sprintLerp * 40f) + (burstSprintLerp * 40f) ) + offsetLerp * OffsetLerpAmount ) * (1 - aimLerp) );
		Rotation *= Rotation.FromAxis( Vector3.Right,(sprintLerp * WeaponInfo.SprintRightRotation) + ( burstSprintLerp * WeaponInfo.BurstSprintRightRotation ) * (1 - aimLerp) );
		Rotation *= Rotation.FromAxis( Vector3.Up, ( ( sprintLerp * WeaponInfo.SprintUpRotation ) + ( burstSprintLerp * WeaponInfo.BurstSprintUpRotation ) ) * (1 - aimLerp) );

		Rotation *= Rotation.FromAxis( Vector3.Forward, -1f );
		Rotation *= Rotation.FromAxis( Vector3.Up, -1f );

		Rotation *= Rotation.From( WeaponInfo.AimAngleOffset * aimLerp );

		Position += forward * avoidance;

		Position += left * (velocity.y * ( ( sprintLerp * WeaponInfo.SprintLeftOffset ) + ( burstSprintLerp * WeaponInfo.BurstSprintLeftOffset ) ) + offsetLerp * -10f * (1 - aimLerp));
		Position += left * ( (WeaponInfo.PostSprintLeftOffset * sprintLerp) + (WeaponInfo.BurstPostSprintLeftOffset * burstSprintLerp ) );

		Position += up * (offsetLerp * -0f + avoidance * -10 * (1 - aimLerp));


		var uitx = new Sandbox.UI.PanelTransform();
		uitx.AddTranslateY(MathF.Sin(walkBob * 1.0f) * speed * -8.0f);
		uitx.AddTranslateX(MathF.Sin(walkBob * 0.5f) * speed * -6.0f);

		MouseDeltaLerpX = MouseDeltaLerpX.LerpTo(mouseDeltaX, Time.Delta * 5f);
		MouseDeltaLerpY = MouseDeltaLerpY.LerpTo(mouseDeltaY, Time.Delta * 5f);

		uitx.AddTranslateX(MouseDeltaLerpX * 25);
		uitx.AddTranslateY(MouseDeltaLerpY * 25);

		uitx.AddRotation( MouseDeltaLerpY * -25, MouseDeltaLerpX * -2, 0f );

		PlayerHud.Current.LeftObjects.Style.Transform = uitx;
		PlayerHud.Current.RightObjects.Style.Transform = uitx;
	}

	private float WalkCycle( float speed, float power, bool abs = false )
	{
		var sin = MathF.Sin( walkBob * speed );
		var sign = Math.Sign( sin );

		if ( abs )
		{
			sign = 1;
		}

		return MathF.Pow( sin, power ) * sign;
	}

	public void ApplyImpulse( Vector3 impulse )
	{
		acceleration += impulse;
	}

	private void LerpTowards( ref float value, float desired, float speed )
	{
		var delta = (desired - value) * speed * DeltaTime;
		var deltaAbs = MathF.Min( MathF.Abs( delta ), MathF.Abs( desired - value ) ) * MathF.Sign( delta );

		if ( MathF.Abs( desired - value ) < 0.001f )
		{
			value = desired;

			return;
		}

		value += deltaAbs;
	}

	private void ApplyDamping( ref Vector3 value, float damping )
	{
		var magnitude = value.Length;

		if ( magnitude != 0 )
		{
			var drop = magnitude * damping * DeltaTime;
			value *= Math.Max( magnitude - drop, 0 ) / magnitude;
		}
	}
}
