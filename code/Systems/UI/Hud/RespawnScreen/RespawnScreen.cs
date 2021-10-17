
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	public enum TransitionState
	{
		ToOverview,
		FromOverview,
		None
	}

	[UseTemplate( "systems/ui/hud/respawnscreen/respawnscreen.html" )]
	public class RespawnScreen : Panel
	{
		private static TransitionState _State = TransitionState.None;
		public static TransitionState State
		{
			get
			{
				if ( TransitionProgress == 1f ) _State = TransitionState.None;

				return _State;
			}
			set
			{
				_State = value;
				TimeSinceStateChanged = 0;
			}
		}
		public static float TransitionProgress => MathX.Clamp( TimeSinceStateChanged, 0, 1 ) / TransitionTime;
		public static TimeSince TimeSinceStateChanged = -1;
		public static float TransitionTime => 1;
		public static Vector3 OverviewPosition => new Vector3( -186.83f, -805.75f, 5024.03f );
		public static Angles OverviewAngles => new Angles( 90, 90, 0 );
		public static CameraSetup CameraSetup = new();

		// @ref
		public Button DeployButton { get; set; }
		public Label GameName { get; set; }
		public Label LoadoutPanel { get; set; }
		// -

		public static Vector3 StartingCameraPosition { get; protected set; }
		public static Rotation StartingCameraRotation { get; protected set; }

		public RespawnScreen()
		{
			StartingCameraPosition = Game.LastCameraSnapshot.Pos;
			StartingCameraRotation = Game.LastCameraSnapshot.Rot;
			State = TransitionState.ToOverview;

			CameraSetup.FieldOfView = 90;
			CameraSetup.ZNear = 10;
			CameraSetup.ZFar = 80000;
		}

		public static Vector3 GetTargetPos()
		{
			switch ( State )
			{
				case TransitionState.ToOverview:
					return OverviewPosition;
				case TransitionState.FromOverview:
					return Local.Pawn.EyePos;
			}

			return Vector3.Zero;
		}

		public static Rotation GetTargetRotation()
		{
			switch ( State )
			{
				case TransitionState.ToOverview:
					return OverviewAngles.ToRotation();
				case TransitionState.FromOverview:
					return Local.Pawn.EyeRot;
			}

			return Rotation.Identity;
		}

		public static Vector3 GetStartPos()
		{
			switch ( State )
			{
				case TransitionState.ToOverview:
					return StartingCameraPosition;
				case TransitionState.FromOverview:
					return OverviewPosition;
			}

			return Vector3.Zero;
		}

		public static Rotation GetStartRotation()
		{
			switch ( State )
			{
				case TransitionState.ToOverview:
					return StartingCameraRotation;
				case TransitionState.FromOverview:
					return OverviewAngles.ToRotation();
			}

			return Rotation.Identity;
		}

		public override void OnDeleted()
		{
			State = TransitionState.FromOverview;

			base.OnDeleted();
		}

		public void Deploy()
		{
			Host.AssertClient();
			Game.DeployCommand();
		}
	}
}
