using System;
using Sandbox;
using Sandbox.UI;

namespace Conquest;

public class DevCamera : CameraMode
{
	Angles LookAngles;
	Vector3 MoveInput;

	Vector3 TargetPos;
	Rotation TargetRot;

	bool PivotEnabled;
	Vector3 PivotPos;
	float PivotDist;

	float MoveSpeed;
	float FovOverride = 0;

	float LerpMode = 0;

	/// <summary>
	/// On the camera becoming activated, snap to the current view position
	/// </summary>
	public override void Activated()
	{
		base.Activated();

		TargetPos = CurrentView.Position;
		TargetRot = CurrentView.Rotation;

		Position = TargetPos;
		Rotation = TargetRot;
		LookAngles = Rotation.Angles();
		FovOverride = 80;

		//
		// Set the devcamera class on the HUD. It's up to the HUD what it does with it.
		//
		Local.Hud?.SetClass( "devcamera", true );
	}

	public override void Deactivated()
	{
		base.Deactivated();

		Local.Hud?.SetClass( "devcamera", false );
	}

	public override void Update()
	{
		var player = Local.Client;
		if ( player == null ) return;

		var tr = Trace.Ray( Position, Position + Rotation.Forward * 4096 ).UseHitboxes().Run();

		// DebugOverlay.Box( tr.EndPos, Vector3.One * -1, Vector3.One, Color.Red );

		FieldOfView = FovOverride;

		Viewer = null;

		if ( PivotEnabled )
		{
			PivotMove();
		}
		else
		{
			FreeMove();
		}
	}

	public override void BuildInput( InputBuilder input )
	{

		MoveInput = input.AnalogMove;

		if ( input.Down( InputButton.Duck ) )
			MoveInput.z = -1;

		if ( input.Down( InputButton.Jump ) )
			MoveInput.z += 1;

		MoveSpeed = 0.5f;

		if ( input.Down( InputButton.Run ) ) MoveSpeed = 5;
		if ( input.Down( InputButton.Walk ) ) MoveSpeed = 0.2f;

		if ( input.Down( InputButton.Slot1 ) ) LerpMode = 0.0f;
		if ( input.Down( InputButton.Slot2 ) ) LerpMode = 0.5f;
		if ( input.Down( InputButton.Slot3 ) ) LerpMode = 0.9f;
		if ( input.Down( InputButton.Slot4 ) ) LerpMode = 0.95f;

		if ( input.Down( InputButton.Slot0 ) ) MoveSpeed = 0.2f;

		if ( input.Pressed( InputButton.Attack1 ) )
		{
			var tr = Trace.Ray( Position, Position + Rotation.Forward * 4096 ).Run();
			PivotPos = tr.EndPosition;
			PivotDist = Vector3.DistanceBetween( tr.EndPosition, Position );
		}

		if ( input.Down( InputButton.Attack2 ) )
		{
			FovOverride += input.AnalogLook.pitch * (FovOverride / 30.0f);
			FovOverride = FovOverride.Clamp( 5, 150 );
			input.AnalogLook = default;
		}

		LookAngles += input.AnalogLook * (FovOverride / 80.0f);
		LookAngles.roll = 0;

		PivotEnabled = input.Down( InputButton.Attack1 );

		input.Clear();
		input.StopProcessing = true;
	}

	void FreeMove()
	{
		var mv = MoveInput.Normal * 300 * RealTime.Delta * Rotation * MoveSpeed;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
	}

	void PivotMove()
	{
		PivotDist -= MoveInput.x * RealTime.Delta * 100 * (PivotDist / 50);
		PivotDist = PivotDist.Clamp( 1, 1000 );

		TargetRot = Rotation.From( LookAngles );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );

		TargetPos = PivotPos + Rotation.Forward * -PivotDist;
		Position = TargetPos;
	}
}
