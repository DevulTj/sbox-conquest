
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Conquest;

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

	// @TODO: Move to ExtensionMethods or something
	protected static float EaseOutCirc( float x ) => MathF.Sqrt( 1 - MathF.Pow( x - 1, 2 ) );

	public static float TransitionProgress => MathX.Clamp( TimeSinceStateChanged, 0, TransitionTime ) / TransitionTime;
	public static float EasedTransitionProgress => EaseOutCirc( TransitionProgress );
	public static float TransitionTime => 0.6f; // Takes x seconds for transition

	private static Vector3 FallbackOverviewPos = new Vector3( -186.83f, -1305.75f, 5024.03f );
	private static Angles FallbackOverviewAngles = new Angles( 90, 90, 0 );

	public static Vector3 OverviewPosition { get; set; }
	public static Rotation OverviewRotation { get; set; }

	public static CameraSetup CameraSetup = new();
	public static TimeSince TimeSinceStateChanged = -1;

	// @ref
	public Button DeployButton { get; set; }
	public Label GameName { get; set; }
	public Panel Footer { get; set; }
	public Panel Points { get; set; }
	public string MapName { get { return Global.MapName; } }
	public Label LoadoutPanel { get; set; }
	// -
	public string GSDisplayText { get { return GameState.Current.DisplayText.ToUpper(); } }

	public static Vector3 StartingCameraPosition { get; protected set; }
	public static Rotation StartingCameraRotation { get; protected set; }

	public RespawnScreen()
	{
		StartingCameraPosition = CurrentView.Position;
		StartingCameraRotation = CurrentView.Rotation;
		State = TransitionState.ToOverview;

		CameraSetup.FieldOfView = 90;
		CameraSetup.ZNear = 10;
		CameraSetup.ZFar = 80000;

		var worldOverviewEntity = Entity.All.OfType<WorldOverview>().FirstOrDefault();

		if ( worldOverviewEntity is not null )
		{
			OverviewPosition = worldOverviewEntity.Position;
			OverviewRotation = worldOverviewEntity.Rotation;
		}
		else
		{
			OverviewPosition = FallbackOverviewPos;
			OverviewRotation = FallbackOverviewAngles.ToRotation();
		}

		foreach ( var capturePoint in Entity.All.OfType<CapturePointEntity>().OrderBy( x => x.Identity ) )
		{
			var panel = Points.AddChild<CapturePointPanel>();
			panel.CapturePoint = capturePoint;
			panel.SetShowName();
		}

		var primary = Footer.AddChild<LoadoutWeaponPanel>();
		primary.IsSelector = true;
		primary.Slot = WeaponSlot.Primary;
		primary.Fetch();

		var secondary = Footer.AddChild<LoadoutWeaponPanel>();
		secondary.IsSelector = true;
		secondary.Slot = WeaponSlot.Secondary;
		secondary.Fetch();
	}

	public static Vector3 Position => GetStartPos().LerpTo( GetTargetPos(), TransitionProgress );
	public static Rotation Rotation => Rotation.Lerp( GetStartRotation(), GetTargetRotation(), TransitionProgress );

	protected static Vector3 GetTargetPos()
	{
		return State switch
		{
			TransitionState.ToOverview => OverviewPosition,
			TransitionState.FromOverview => Local.Pawn.EyePosition,
			_ => Vector3.Zero
		};
	}

	protected static Rotation GetTargetRotation()
	{
		return State switch
		{
			TransitionState.ToOverview => OverviewRotation,
			TransitionState.FromOverview => Local.Pawn.EyeRotation,
			_ => Rotation.Identity
		};
	}

	protected static Vector3 GetStartPos()
	{
		return State switch
		{
			TransitionState.ToOverview => StartingCameraPosition,
			TransitionState.FromOverview => OverviewPosition,
			_ => Vector3.Zero
		};
	}

	protected static Rotation GetStartRotation()
	{
		return State switch
		{
			TransitionState.ToOverview => StartingCameraRotation,
			TransitionState.FromOverview => OverviewRotation,
			_ => Rotation.Identity
		};
	}

	public override void OnDeleted()
	{
		State = TransitionState.FromOverview;

		base.OnDeleted();
	}

	public override void Tick()
	{
		base.Tick();

		DeployButton.BindClass( "allowed", () => GameState.Current.CanDeploy );
	}

	public void Deploy()
	{
		Host.AssertClient();
		Game.DeployCommand();
	}
}
