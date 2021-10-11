using Conquest.UI;
using Sandbox;
using System.ComponentModel;

namespace Conquest
{
	public partial class CapturePointEntity : ModelEntity
	{
		[Net, Category("Capture Point")]
		public string Identity { get; set; }

		[Net, Category( "Capture Point" )]
		public TeamSystem.Team Team { get; set; } = TeamSystem.Team.Unassigned;

		public CapturePointEntity()
		{
			if ( Host.IsClient )
			{
				Marker = new CapturePointHudMarker( this );
			}
		}

		public CapturePointHudMarker Marker { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			// Set the default size
			SetTriggerSize( 128 );

			// Client doesn't need to know about htis
			Transmit = TransmitType.Always;
		}
		public override void ClientSpawn()
		{
			base.ClientSpawn();
		}

		/// <summary>
		/// Set the trigger radius. Default is 16.
		/// </summary>
		public void SetTriggerSize( float radius )
		{
			SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, radius );
			CollisionGroup = CollisionGroup.Trigger;
		}


		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is Player player )
			{
				Team = player.Team;
			}
		}
	}
}
