using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class HitIndicator : Panel
	{
		public static HitIndicator Current;

		public HitIndicator()
		{
			Current = this;
			StyleSheet.Load( "Systems/UI/Hud/HitIndicator/HitIndicator.scss" );
		}

		public override void Tick()
		{
			base.Tick();
			this.PositionAtCrosshair();
		}

		public void OnHit( Vector3 pos, float amount, bool isKill = false, bool isHeadshot = false )
		{
			new HitPoint( amount, pos, isKill, isHeadshot, this );
		}

		public class HitPoint : Panel
		{
			public HitPoint( float amount, Vector3 pos, bool isKill, bool isHeadshot, Panel parent )
			{
				Parent = parent;
				_ = Lifetime();

				if ( isKill )
				{
					AddClass( "kill" );
				}

				if ( isHeadshot )
				{
					AddClass( "headshot" );
				}
			}

			async Task Lifetime()
			{
				await Task.Delay( 200 );
				Delete();
			}
		}
	}

}
