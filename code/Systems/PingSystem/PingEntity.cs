using Sandbox;
using System.Threading.Tasks;

namespace Conquest;

[Library( "conquest_ping" )]
public partial class PingEntity : ModelEntity, IHudMarkerEntity, IMiniMapEntity, IGameStateAddressable
{
	public PingType Type { get; set; } = PingType.Ping;

	public string PingMessage { get; set; }

	public string Message => string.IsNullOrEmpty( PingMessage ) ? GetPingMessage() : PingMessage;

	public async Task DeferredDeletion()
	{
		await GameTask.DelayRealtimeSeconds( 10f );

		Delete();
	}

	protected string GetPingMessage()
	{
		return Type switch
		{
			PingType.Enemy => "ENEMY",
			_ => "MARKER"
		};
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

		info.Text = Message;
		info.Position = Position + Vector3.Up * 20f;
		info.Classes[Type.ToString()] = true;

		return true;	
	}

	bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		info.Position = Position;
		info.Classes[Type.ToString()] = true;

		return true;
	}

	void IGameStateAddressable.ResetState()
	{
		Delete();
	}
}
