using Conquest.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	public partial class Player 
	{
		private Particles SpeedLines { get; set; }

		private void InitSpeedLines()
		{
			if ( IsLocalPawn )
				SpeedLines = Particles.Create( "particles/player/speed_lines.vpcf" );
		}

		private void DestroySpeedLines()
		{
			if ( IsLocalPawn )
				SpeedLines?.Destroy();
		}

		[Event.Tick.Client]
		private void SpeedLinesTick()
		{
			if ( IsLocalPawn && Controller is WalkController controller )
			{
				float targetSpeed = controller.Slide.IsActive ? 500f : controller.BurstSprintSpeed * 1.7f;
				var speed = Velocity.Length.Remap( 0f, targetSpeed, 0f, 1f );
				speed = Math.Min( Easing.EaseIn( speed ) * 60f, 60f );
				SpeedLines.SetPosition( 1, new Vector3( speed, 0f, 0f ) );
			}
		}

		public string GetMainClass() => "player";

		bool CalculateVis()
		{
			var tr = Trace.Ray( CurrentView.Position, EyePos )
				.WorldAndEntities()
				.Ignore( Local.Pawn )
				.Run();

			if ( tr.Distance > 1024 ) return false;

			if ( tr.Hit && tr.Entity == this )
				return true;
			else
				return false;
		}

		bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
		{
			if ( !this.IsValid() )
				return false;

			if ( LifeState != LifeState.Alive )
				return false;

			info.Text = "";
			info.Position = Position;

			var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );

			if ( friendState == TeamSystem.FriendlyStatus.Hostile )
			{
				if ( !CalculateVis() )
					return false;
			}

			info.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
			info.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;
			info.Classes["me"] = Local.Pawn == this;


			return true;
		}

		bool IHudMarkerEntity.Update( ref HudMarkerBuilder info )
		{
			if ( !this.IsValid() )
				return false;

			if ( this == Local.Pawn )
				return false;

			if ( LifeState != LifeState.Alive )
				return false;

			var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );

			if ( friendState == TeamSystem.FriendlyStatus.Hostile )
			{
				if ( !CalculateVis() )
					return false;
			}

			info.Text = Client.Name;
			info.Position = EyePos + Rotation.Up * 15f;

			info.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
			info.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;

			return true;
		}
	}
}
