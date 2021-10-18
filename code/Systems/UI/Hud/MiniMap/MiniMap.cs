
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest
{
	public class MiniMapCapturePoint : Panel
	{
		public Label Identity { get; set; }

		public MiniMapCapturePoint()
		{
			AddClass( "capturepoint" );
			Identity = AddChild<Label>();
		}
	}

	public class MiniMapDot : Panel
	{
		public Entity Entity { get; set; }

		public MiniMapDot( Entity entity )
		{
			Entity = entity;
		}
	}

	[UseTemplate( "systems/ui/hud/minimap/minimap.html" )]
	public class MiniMap : Panel
	{
		public Panel CapturePointsPanel { get; set; }
		public Panel MiniMapPanel { get; set; }
		// Used to contain all active dots
		public Panel DotAnchor { get; set; }

		protected List<MiniMapDot> Dots { get; set; } = new();

		protected MiniMapDot NewDot( Entity entity )
		{
			return new MiniMapDot( entity )
			{
				Parent = DotAnchor
			};
		}

		public float Range => 1000f;

		// @TODO: Calc this
		public Vector2 MiniMapSize => new Vector2( 300f, 236f );

		public void ClearDot( MiniMapDot Dot )
		{
			if ( Dot is null )
				return;
			if ( !Dots.Contains( Dot ) )
				return;

			Dots.Remove( Dot );
			Dot.Delete();

			return;
		}

		public void ValidatePlayer( Player player )
		{
			var currentDot = Dots.Where( x => x.Entity == player ).FirstOrDefault();

			if ( !player.IsValid() 
				 || player.LifeState != LifeState.Alive
				 || player.Team == TeamSystem.Team.Unassigned )
			{
				ClearDot( currentDot );
				return;
			}

			if ( currentDot is null )
			{
				currentDot = new MiniMapDot( player )
				{
					Parent = DotAnchor
				};

				Dots.Add( currentDot );
			}

			var diff = player.Position - CurrentView.Position;

			var x = MiniMapSize.x / Range * diff.x * 0.5f;
			var y = MiniMapSize.y / Range * diff.y * 0.5f;
			var ang = MathF.PI / 180 * ( CurrentView.Rotation.Yaw() - 90f );
			var cos = MathF.Cos( ang );
			var sin = MathF.Sin( ang );

			var translatedX = x * cos + y * sin;
			var translatedY = y * cos - x * sin;

			currentDot.Style.Left = (MiniMapSize.x / 2f) + translatedX;
			currentDot.Style.Top = (MiniMapSize.y / 2f) - translatedY;

			var localPlayer = Local.Pawn as Player;
			currentDot.SetClass( "enemy", TeamSystem.IsHostile( player.Team, localPlayer.Team ) );
			currentDot.SetClass( "me", player == localPlayer );
		}

		protected void UpdateCapturePoints()
		{
			var localPlayer = Local.Pawn as Player;

			int i = 0;
			foreach ( var capturePoint in Entity.All.OfType<CapturePointEntity>().OrderBy( x => x.Identity ) )
			{
				MiniMapCapturePoint panel;

				if ( CapturePointsPanel.ChildrenCount > i )
					panel = CapturePointsPanel.GetChild( i ) as MiniMapCapturePoint;
				else
					panel = CapturePointsPanel.AddChild<MiniMapCapturePoint>();

				i++;

				panel.Identity.Text = capturePoint.Identity;

				var flipflop = ((float)capturePoint.TimeSinceStateChanged).FloorToInt() % 1 == 0;
				var friendState = TeamSystem.GetFriendState( localPlayer.Team, capturePoint.Team );

				panel.SetClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
				panel.SetClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );
				panel.SetClass( "contested", capturePoint.CurrentState == CapturePointEntity.State.Contested );
				panel.SetClass( "contestedFlash", capturePoint.CurrentState == CapturePointEntity.State.Contested && flipflop );
				panel.SetClass( "capturing", capturePoint.CurrentState == CapturePointEntity.State.Capturing );
				panel.SetClass( "capturingFlash", capturePoint.CurrentState == CapturePointEntity.State.Capturing && flipflop );
			}
		}

		protected void UpdatePlayers()
		{
			var players = Entity.All.OfType<Player>().OrderBy( x => Vector3.DistanceBetween( x.EyePos, CurrentView.Position ) );

			foreach ( var player in players )
			{
				ValidatePlayer( player );
			}
		}

		protected void UpdateMiniMapDots()
		{
			UpdatePlayers();
		}

		public override void Tick()
		{
			base.Tick();

			UpdateMiniMapDots();
			UpdateCapturePoints();
		}
	}
}
