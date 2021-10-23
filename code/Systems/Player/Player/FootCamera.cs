using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conquest
{
	public class FootCamera : Camera
	{
		Vector3 lastPos;
		public float LeanAmount { get; set; } = 0;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;

			lastPos = Pos;
		}

		public override void Update()
		{
			var pawn = Local.Pawn as Player;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePos;
			if ( eyePos.Distance( lastPos ) < 300 ) // TODO: Tweak this, or add a way to invalidate lastpos when teleporting
			{
				Pos = Vector3.Lerp( eyePos.WithZ( lastPos.z ), eyePos, 20.0f * Time.Delta );
			}
			else
			{
				Pos = eyePos;
			}

			Rot = pawn.EyeRot;

			var sliding = (pawn.Controller as WalkController).Slide.IsActive;
			if ( sliding || LeanAmount != 0f )
			{
				LeanAmount = LeanAmount.LerpTo( sliding ? pawn.Velocity.Dot( Rot.Right ) * 0.03f : 0, Time.Delta * 15.0f );

				var appliedLean = LeanAmount;

				Rot *= Rotation.From( 0, 0, appliedLean );
			}

			FieldOfView = 80;

			if ( pawn.IsAiming )
				FieldOfView -= 10;

			if ( pawn.IsBurstSprinting )
			{
				FieldOfView += 5;
			}

			Viewer = pawn;
			lastPos = Pos;
		}
	}
}
