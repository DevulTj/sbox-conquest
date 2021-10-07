
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	public static class PanelExtension
	{
		public static async Task AddTimedClass( this Panel panel, string className, float time = 0.5f )
		{
			panel.AddClass( className );
			await GameTask.DelayRealtimeSeconds( time );
			panel.RemoveClass( className );
		}

		public static void PositionAtCrosshair( this Panel panel )
		{
			panel.PositionAtCrosshair( Local.Pawn );
		}

		public static void PositionAtCrosshair( this Panel panel, Entity player )
		{
			if ( !player.IsValid() ) return;

			var eyePos = player.EyePos;
			var eyeRot = player.EyeRot;

			var tr = Trace.Ray( eyePos, eyePos + eyeRot.Forward * 2000 )
				.Size( 1.0f )
				.Ignore( player )
				.UseHitboxes()
				.Run();

			panel.PositionAtWorld( tr.EndPos );
		}

		public static void PositionAtWorld( this Panel panel, Vector3 pos )
		{
			var screenpos = pos.ToScreen();

			if ( screenpos.z < 0 )
				return;

			panel.Style.Left = Length.Fraction( screenpos.x );
			panel.Style.Top = Length.Fraction( screenpos.y );
			panel.Style.Dirty();
		}
	}

	public class Crosshair : Panel
	{
		int fireCounter;

		public Crosshair()
		{
			StyleSheet.Load( "systems/ui/crosshair/crosshair.scss" );

			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}
		}

		public override void Tick()
		{
			base.Tick();
			this.PositionAtCrosshair();

			SetClass( "fire", fireCounter > 0 );

			if ( fireCounter > 0 )
				fireCounter--;
		}

		protected override void OnEvent( PanelEvent e )
		{
			if ( e.Name == "fire" )
			{
				// this is a hack until we have animation or TriggerClass support
				fireCounter += 2;
			}

			base.OnEvent( e );
		}
	}
}
