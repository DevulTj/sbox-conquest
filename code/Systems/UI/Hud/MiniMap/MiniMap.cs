
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
		public IMiniMapEntity Entity { get; set; }

		public Label Text { get; set; }

		public MiniMapDot( IMiniMapEntity entity )
		{
			Entity = entity;

			Text = AddChild<Label>();
			Text.Text = "";
		}

		public void Apply( MiniMapDotBuilder info )
		{
			Text.Text = info.Text;

			foreach( var kv in info.Classes )
			{
				SetClass( kv.Key, kv.Value );
			}
		}
	}

	public class MiniMapDotBuilder
	{
		public Dictionary<string, bool> Classes { get; set; } = new();
		public Vector3 Position { get; set; } = new();
		public Rotation Rotation { get; set; } = new();
		public string Text { get; set; } = "";
	}

	[UseTemplate( "systems/ui/hud/minimap/minimap.html" )]
	public class MiniMap : Panel
	{
		public Panel CapturePointsPanel { get; set; }
		public Panel MiniMapPanel { get; set; }
		// Used to contain all active dots
		public Panel DotAnchor { get; set; }

		protected List<MiniMapDot> Dots { get; set; } = new();

		public float Range => 2000f;

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

		// @TODO: Replace with MiniMapDot system
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

		protected void ValidateEntity( IMiniMapEntity entity )
		{
			var info = new MiniMapDotBuilder();
			var styleClass = entity.GetMainClass();
			var currentDot = Dots.Where( x => x.Entity == entity ).FirstOrDefault();

			if ( !entity.Update( ref info ) )
			{
				ClearDot( currentDot );
				return;
			}

			if ( currentDot is null )
			{
				currentDot = new MiniMapDot( entity )
				{
					Parent = DotAnchor
				};

				// Add style class
				currentDot.AddClass( styleClass );

				Dots.Add( currentDot );
			}

			var diff = info.Position - CurrentView.Position;

			var x = MiniMapSize.x / Range * diff.x * 0.5f;
			var y = MiniMapSize.y / Range * diff.y * 0.5f;
			var ang = MathF.PI / 180 * (CurrentView.Rotation.Yaw() - 90f);
			var cos = MathF.Cos( ang );
			var sin = MathF.Sin( ang );

			var translatedX = x * cos + y * sin;
			var translatedY = y * cos - x * sin;

			currentDot.Style.Left = (MiniMapSize.x / 2f) + translatedX;
			currentDot.Style.Top = (MiniMapSize.y / 2f) - translatedY;
			currentDot.Apply( info );
		}

		protected void UpdateMiniMapDots()
		{
			var existingDots = Dots.Select( x => x.Entity ).ToList();

			Entity.All.OfType<IMiniMapEntity>()
							.Concat( existingDots )
							.ToList()
							.ForEach( x => ValidateEntity( x ) );
		}

		public override void Tick()
		{
			base.Tick();

			UpdateMiniMapDots();
			UpdateCapturePoints();
		}
	}
}
