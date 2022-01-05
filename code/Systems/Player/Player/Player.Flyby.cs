using Sandbox;

namespace Conquest;

public partial class Player
{
	public ModelEntity FlybyTrigger { get; set; }

	protected void CreateFlybyTrigger()
	{
		var flyby = new ModelEntity()
		{
			Transmit = TransmitType.Never
		};

		flyby.Tags.Add( "flyby" );
		flyby.SetParent( this );
		flyby.SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( Vector3.Zero, Vector3.One * 0.1f, 100 ) );
		flyby.CollisionGroup = CollisionGroup.Debris;

		FlybyTrigger = flyby;
	}

	[ClientRpc]
	private void DoFlybySound()
	{
		Host.AssertClient();
		Sound.FromScreen( $"bullet.flyby" );
	}
}
