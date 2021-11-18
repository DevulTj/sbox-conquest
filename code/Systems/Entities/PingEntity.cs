using Sandbox;
using System.Threading.Tasks;

namespace Conquest;

[Library( "conquest_ping" )]
public partial class PingEntity : ModelEntity, IHudMarkerEntity, IMiniMapEntity, IGameStateAddressable
{
	public async Task DeferredDeletion()
	{
		await GameTask.DelayRealtimeSeconds( 10f );

		Delete();
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		_ = DeferredDeletion();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	public string GetMainClass() => "ping";


	bool IHudMarkerEntity.Update( ref HudMarkerBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		info.Position = Position;

		return true;	
	}

	bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		info.Position = Position + CollisionBounds.Center;

		return true;
	}

	void IGameStateAddressable.ResetState()
	{
		Delete();
	}
}
