using Conquest.UI;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Conquest;

[Library("conquest_capturepoint")]
[Hammer.Sphere( "TriggerRadius", 255, 255, 255, true )]
[Hammer.Solid]
public partial class CapturePointEntity : BaseTrigger, IMiniMapEntity, IHudMarkerEntity, IGameStateAddressable
{
	public enum State
	{
		None,
		Contested,
		Capturing
	}

	[Net, Category( "Capture Point" ), Property]
	public string Identity { get; set; }

	[Net, Category( "Capture Point" ), Property]
	public string NiceName { get; set; } = "ZONE";

	[Property]
	public float TriggerRadius { get; set; } = 386f;

	[BindComponent]
	protected TeamComponent TeamComponent { get; }

	public Team Team
	{
		get => TeamComponent.Team;
		set => TeamComponent.Team = value;
	}

	[Net, Category( "Capture Point" )]
	public Team HighestTeam { get; set; } = Team.Unassigned;

	protected static int ArraySize => Enum.GetNames( typeof( Team ) ).Length - 1;

	[Net, Category( "Capture Point" )]
	public List<int> OccupantCounts { get; set; } = new();

	[Net, Category( "Capture Point")]
	public float Captured { get; set; } = 0;

	// takes 10s to cap
	[Category( "Capture Point"), Description("Time in seconds it takes to capture a point with one player.")]
	public float CaptureTime = 10;

	[Net, Category( "Capture Point"), Change("OnStateChanged")]
	public State CurrentState { get; set; }

	protected void OnStateChanged( State then, State now )
	{
		TimeSinceStateChanged = 0;
	}

	public TimeSince TimeSinceStateChanged { get; set; } = 0;

	// @Server
	public Dictionary<Team, HashSet<Player>> Occupants { get; set; } = new();

	public CapturePointEntity()
	{
	}

	protected void Initialize()
	{
		if ( Host.IsServer )
		{
			// Create a TeamComponent
			Components.GetOrCreate<TeamComponent>();

			Team = Team.Unassigned;
			HighestTeam = Team;
			Captured = 0;
			CurrentState = State.None;
			OccupantCounts.Clear();
			Occupants.Clear();
			TimeSinceStateChanged = 0;

			for ( int i = 0; i < ArraySize; i++ )
				OccupantCounts.Add( 0 );

			// Initialize the dictionary's list values.
			foreach ( Team team in Enum.GetValues( typeof( Team ) ) )
			{
				if ( team == Team.Unassigned )
					continue;

				Occupants[team] = new();
			}
		}
	}
	public override void Spawn()
	{
		base.Spawn();

		// Set the default size
		SetTriggerSize( TriggerRadius );

		// Client doesn't need to know about htis
		Transmit = TransmitType.Always;

		Initialize();
	}

	/// <summary>
	/// Set the trigger radius. Default is 16.
	/// </summary>
	public void SetTriggerSize( float radius )
	{
		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero, radius );
		CollisionGroup = CollisionGroup.Trigger;
	}

	internal void AddPlayer( Player player )
	{
		// Already in the list!
		if ( Occupants[player.Team].Contains( player ) )
			return;

		// Already in another cap point.
		if ( player.CapturePoint is not null && player.CapturePoint != this )
			return;

		player.CapturePoint = this;

		Occupants[player.Team].Add( player );
		OccupantCounts[(int)player.Team]++;
	}

	internal void RemovePlayer( Player player )
	{
		if ( !Occupants.ContainsKey( player.Team ) )
			return;

		if ( !Occupants[player.Team].Contains( player ) )
			return;

		if ( player.CapturePoint == this )
			player.CapturePoint = null;

		Occupants[player.Team].Remove( player );
		OccupantCounts[(int)player.Team]--;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( Host.IsServer && other is Player player )
		{
			AddPlayer( player );
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( Host.IsServer && other is Player player )
		{
			RemovePlayer( player );
		}
	}

	public int GetCount( Team team )
	{
		return OccupantCounts[(int)team];
	}

	[Event.Tick.Server]
	public void Tick()
	{
		if ( Occupants is null || OccupantCounts is null )
			return;

		if ( Occupants.Count == 0 || OccupantCounts.Count == 0 )
			return;


		var lastCount = 0;
		var highest = Team.Unassigned;
		var contested = false;
		for ( int i = 0; i < OccupantCounts.Count; i++ )
		{
			var team = (Team)i;
			var count = OccupantCounts[i];

			if ( lastCount > 0 && count > 0 )
			{
				contested = true;
				break;
			}

			if ( count > 0 )
			{
				lastCount = count;
				highest = team;
			}
		}

		HighestTeam = highest;

		// nobody is fighting for this point (which shouldn't really happen)
		if ( highest == Team.Unassigned )
		{
			CurrentState = State.None;

			return;
		}

		// Don't do anythig while we're contested
		if ( contested )
		{
			CurrentState = State.Contested;
			return;
		}
		else
		{
			CurrentState = State.None;
		}

		// A team is trying to cap. Let's reverse this shit.
		if ( Team != Team.Unassigned && highest != Team )
		{
			float attackMultiplier = MathF.Sqrt( lastCount ); // Somewhat random sub-linear scale
			Captured = MathX.Clamp( Captured - Time.Delta * attackMultiplier / CaptureTime, 0, 1 );

			if ( Captured == 0f )
			{
				Team = Team.Unassigned;
			}
			else
			{
				CurrentState = State.Capturing;
			}
		}
		else
		{
			float attackMultiplier = MathF.Sqrt( lastCount ); // Somewhat random sub-linear scale


			var last = Captured;

			Captured = MathX.Clamp( Captured + Time.Delta * attackMultiplier / CaptureTime, 0, 1 );

			if ( Captured == 1f )
			{
				if ( last != Captured )
				{
					foreach( var player in Occupants[highest] )
					{
						player.GiveAward( "Capture" );
					}
				}
				Team = highest;
			}
			else
			{
				CurrentState = State.Capturing;
				Team = Team.Unassigned;
			}
		}
	}

	// @Interfaces 
	public string GetMainClass() => "capturepoint";

	bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		info.Text = Identity;
		info.Position = Position;

		var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );
		info.Classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
		info.Classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;

		return true;
	}

	void IGameStateAddressable.ResetState()
	{
		Initialize();
	}

	public Dictionary<string, bool> GetUIClasses()
	{
		var classes = new Dictionary<string, bool>();
		var friendState = TeamSystem.GetFriendState( Team, TeamSystem.MyTeam );
		bool flipflop = ((float)TimeSinceStateChanged).FloorToInt() % 1 == 0;

		// This isn't great. But it'll do.
		classes["friendly"] = friendState == TeamSystem.FriendlyStatus.Friendly;
		classes["enemy"] = friendState == TeamSystem.FriendlyStatus.Hostile;
		classes["contested"] = CurrentState == State.Contested;
		classes["contestedFlash"] = CurrentState == State.Contested && flipflop;
		classes["capturing"] = CurrentState == State.Capturing;
		classes["capturingFlash"] = CurrentState == State.Capturing && flipflop;

		return classes;
	}

	bool IHudMarkerEntity.Update( ref HudMarkerBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		if ( Local.Pawn is Player soldierPlayer && soldierPlayer.CapturePoint == this )
			return false;

		info.Text = Identity;
		info.Position = Position + Rotation.Up * 200f;
		info.StayOnScreen = true;
		info.Classes = GetUIClasses();

		return true;
	}
}
