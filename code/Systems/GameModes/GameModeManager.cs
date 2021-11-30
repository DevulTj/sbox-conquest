
using Sandbox;

namespace Conquest;

public partial class GameModeManager : Entity
{
	public static GameModeManager Current { get; set; }

	/// <summary>
	/// The current game mode
	/// </summary>
	[Net] public GameMode GameMode { get; set; }

	public GameModeManager()
	{
		Current = this;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		// @TODO: This needs to be decided by the user.
		GameMode = new Stronghold.GameMode();
		// Initialize the gamemode on game start.
		GameMode.Initialize();
	}

	[Event.Tick]
	public virtual void Tick()
	{
		GameMode?.Tick( Time.Delta );

		Print();
	}

	protected bool IsDebugging { get; set; } = true;
	protected virtual void Print()
	{
		if ( !IsDebugging )
			return;

		var position = Host.IsServer ? new Vector2( 100, 100 ) : new Vector2( 100, 400 );
		DebugOverlay.ScreenText( position, 0, Color.White, $"{(Host.IsServer ? "[Server]" : "[Client]")}\n" +
			$"Current GameMode: {GameMode}\n" +
			$"{GameMode.Print()}\n" );
	}
}
