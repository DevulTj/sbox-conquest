using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class HurtIndicator : Panel
	{
		public static HurtIndicator Current;

		public HurtIndicator()
		{
			Current = this;
			StyleSheet.Load( "Systems/UI/Hud/HurtIndicator/HurtIndicator.scss" );
		}

		public void OnHit(Vector3 pos)
		{
			var p = new HitPoint(pos);
			p.Parent = this;
		}

		public class HitPoint : Panel
		{
			public Vector3 Position;

			public HitPoint(Vector3 pos)
			{
				Position = pos;

				_ = Lifetime();
			}

			float lerpVal = 0;
			public override void Tick()
			{
				base.Tick();

				var wpos = CurrentView.Rotation.Inverse * ( Position.WithZ( 0 ) - CurrentView.Position.WithZ( 0 ) ).Normal;
				wpos = wpos.WithZ( 0 ).Normal;
				var angle = MathF.Atan2( wpos.y, -1.0f * wpos.x );

				var pt = new PanelTransform();

				pt.AddTranslateX( Length.Percent( -50.0f ) );
				pt.AddTranslateY( Length.Percent( -50.0f ) );
				pt.AddRotation( 0, 0, angle.RadianToDegree() );
				pt.AddSkew( 5f, 5f, 5f );

				Style.Transform = pt;
				Style.Dirty();
			}

			async Task Lifetime()
			{
				await Task.Delay( 500 );
				AddClass("dying");
				await Task.Delay( 100 );
				Delete();
			}
		}
	}
}
