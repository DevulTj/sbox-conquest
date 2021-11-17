using Sandbox;

namespace Conquest;

[Library( "conquest_ammocrate" )]
public partial class AmmoCrateEntity : Prop, IUse, IHudMarkerEntity, IMiniMapEntity, IGameStateAddressable
{
	[AdminCmd( "conquest_debug_ammocrate" )]
	public static void CreateAmmoCrate()
	{
		var caller = ConsoleSystem.Caller.Pawn;
		var entity = new AmmoCrateEntity();

		var tr = Trace.Ray( caller.EyePos, caller.EyeRot.Forward * 1000f )
			.WorldAndEntities()
			.Ignore( caller )
			.Run();

		entity.Position = tr.EndPos;
	}

	[Net] public TimeSince LastUsedTime { get; set; }

	public int AmountToGive => 30;
	public float UseCooldown => 1;

	public override void Spawn()
	{
		base.Spawn();

		Health = 100;
		Transmit = TransmitType.Default;

		SetModel( "assets/ammobox/ammo_box.vmdl" );

		PlaySound( "ammobox.deploy" );
	}

	public bool OnUse( Entity user )
	{
		if ( user is Player player )
		{
			player.GiveAll( 120 );
			ClientUsed( To.Single( player.Client ) );

			PlaySound( "ammobox.replenish" );

			LastUsedTime = 0;
		}

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return LastUsedTime >= UseCooldown;
	}

	[ClientRpc]
	protected void ClientUsed()
	{
		Log.Info( "testing" );
		KillFeedPanel.Current?.AddMessage( "Ammo replenished" );
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	public string GetMainClass() => "ammocrate";

	protected bool CalculateVis()
	{
		var tr = Trace.Ray( CurrentView.Position, Position + CollisionBounds.Center	 )
			.WorldAndEntities()
			.Ignore( Local.Pawn )
			.Ignore( this )
			.Run();


		if ( tr.Distance > 768 ) return false;

		if ( tr.Hit && tr.Entity != this )
			return false;

		return true;
	}

	bool IHudMarkerEntity.Update( ref HudMarkerBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		if ( LifeState != LifeState.Alive )
			return false;

		if ( !CalculateVis() )
			return false;

		info.Position = Position + CollisionBounds.Center;

		return true;	
	}

	bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		if ( LifeState != LifeState.Alive )
			return false;

		if ( !CalculateVis() )
			return false;

		info.Position = Position + CollisionBounds.Center;

		return true;
	}

	void IGameStateAddressable.ResetState()
	{
		Delete();
	}
}
