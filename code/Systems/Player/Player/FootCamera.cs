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
