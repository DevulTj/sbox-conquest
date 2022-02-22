using Sandbox;

namespace Conquest;

public partial class Player
{
	[ClientRpc]
	protected void ClientRpcPing( Vector3 position, PingType pingType, Entity parent = null, Client sender = null )
	{
		var ping = new PingEntity();
		ping.Type = pingType;
		ping.Position = position;

		if ( sender.IsValid() )
		{
			ping.PingMessage = sender.Name.ToUpperInvariant();
		}

		if ( parent.IsValid() )
			ping.SetParent( parent );
	}

	protected void Ping()
	{
		var tr = GetPingTrace();

		var pingType = PingType.Ping;

		Vector3 position = tr.EndPos;
		Entity taggedEntity = null;

		if ( tr.Entity is Player enemy )
		{
			if ( TeamSystem.IsHostile( Team, enemy.Team ) )
			{
				pingType = PingType.Enemy;
				//taggedEntity = tr.Entity;
				position = tr.Entity.EyePosition;
			}
		}

		if ( tr.Hit )
			ClientRpcPing( Net.To.Squad( Client ), position, pingType, taggedEntity, Client );
	}

	protected TraceResult GetPingTrace()
	{
		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 10000f ).WorldAndEntities().Ignore( this ).Radius( 1f ).Run();

		return tr;
	}
}
