using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conquest;

public class FootCamera : CameraMode
{
	Vector3 lastPos;
	public float LeanAmount { get; set; } = 0;

	public override void Activated()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		Position = pawn.EyePosition;
		Rotation = pawn.EyeRotation;

		lastPos = Position;
	}

	Angles reloadAng = new Angles();

	public override void Update()
	{
		var pawn = Local.Pawn as Player;
		if ( pawn == null ) return;

		var eyePos = pawn.EyePosition;
		if ( eyePos.Distance( lastPos ) < 300 ) // TODO: Tweak this, or add a way to invalidate lastpos when teleporting
		{
			Position = Vector3.Lerp( eyePos.WithZ( lastPos.z ), eyePos, 20.0f * Time.Delta );
		}
		else
		{
			Position = eyePos;
		}

		Rotation = pawn.EyeRotation;

		var sliding = (pawn.Controller as WalkController).Slide.IsActive;
		if ( sliding || LeanAmount != 0f )
		{
			LeanAmount = LeanAmount.LerpTo( sliding ? pawn.Velocity.Dot( Rotation.Right ) * 0.03f : 0, Time.Delta * 15.0f );

			var appliedLean = LeanAmount;

			Rotation *= Rotation.From( 0, 0, appliedLean );
		}

		FieldOfView = 80;

		if ( pawn.IsAiming )
			FieldOfView -= 10;

		if ( pawn.IsBurstSprinting )
		{
			FieldOfView += 5;
		}

		var weapon = pawn.ActiveChild as BaseWeapon;

		if ( weapon.IsValid() )
		{
			if ( weapon.IsReloading )
			{
				var transform = pawn.GetAttachment( "hat", false ) ?? new Transform( pawn.EyePosition, pawn.EyeRotation, 1 );
				var attachmentAng = transform.Rotation.Angles() * -1f;

				reloadAng = Angles.Lerp( reloadAng, attachmentAng * 0.02f, Time.Delta * 10f );

				Rotation *= Rotation.From( reloadAng );
			}
			else
			{
				reloadAng = Angles.Lerp( reloadAng, Angles.Zero, Time.Delta * 10f );
				Rotation *= Rotation.From( reloadAng );
			}
		}

		Viewer = pawn;
		lastPos = Position;
	}


	public override void BuildInput( InputBuilder input )
	{
		var pawn = Local.Pawn as Player;
		if ( !pawn.IsValid() ) return;

		var weapon = pawn.ActiveChild as BaseWeapon;

		if ( weapon.IsValid() )
		{
			var oldPitch = input.ViewAngles.pitch;
			var oldYaw = input.ViewAngles.yaw;
			input.ViewAngles.pitch -= weapon.CurrentRecoilAmount.y * Time.Delta;
			input.ViewAngles.yaw -= weapon.CurrentRecoilAmount.x * Time.Delta;
			weapon.CurrentRecoilAmount -= weapon.CurrentRecoilAmount.WithY( (oldPitch - input.ViewAngles.pitch) * weapon.RecoilRecoveryScaleFactor * 1f ).WithX( (oldYaw - input.ViewAngles.yaw) * weapon.RecoilRecoveryScaleFactor * 1f );
		}

		base.BuildInput( input );
	}
}
