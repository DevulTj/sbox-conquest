
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Conquest;

[Library( "SquadmatePanel" )]
public class SquadmatePanel : Panel
{
	public SquadmatePanel()
	{
		PlayerName = Add.Label( "DevulTj", "name" );
		Avatar = Add.Image( "avatarbig:76561197973858781", "avatar" );
		Bar = Add.Panel( "bar" );
		FillBar = Bar.Add.Panel( "fill" );
	}

	public void SetClient( Client cl )
	{
		if ( !cl.IsValid() )
		{
			Client = null;

			return;
		}

		Client = cl;

		PlayerName.Text = cl.Name;
		Avatar.SetTexture( $"avatarbig:{cl.PlayerId}" );

		var squad = SquadManager.GetSquad( cl );
		if ( squad.SquadLeader == cl )
		{
			Icon = Add.Image( "ui/crown.png", "icon" );
		}
	}

	public Client Client { get; set; }

	public Image Avatar { get; set; }
	public Panel Bar { get; set; }
	public Panel FillBar { get; set; }
	public Label PlayerName { get; set; }
	public Image Icon { get; set; }

	float storedHpPercent = 1;
	public override void Tick()
	{
		base.Tick();

		SetClass( "valid", Client.IsValid() );

		if ( !Client.IsValid() )
			return;

		var player = Client.Pawn as Player;

		SetClass( "dead", !player.IsValid() || Client.Pawn.LifeState != LifeState.Alive );

		if ( !player.IsValid() ) return;

		var healthPercent = (player.Health / player.MaxHealth) * 100f;

		storedHpPercent = storedHpPercent.LerpTo( healthPercent, Time.Delta * 10f );
		FillBar.Style.Width = Length.Percent( storedHpPercent );

		FillBar.SetClass( "hurt", healthPercent < 0.4 );
		FillBar.SetClass( "dying", healthPercent < 0.2 );
	}
}

[UseTemplate]
public class SquadOverview : Panel
{
	public SquadOverview()
	{
		for ( int i = 0; i < 4; i++ )
		{
			AddMember( null );
		}
	}

	// @ref
	public Panel RootPanel { get; set; }

	public Panel Members { get; set; }

	public string SquadName => ( SquadManager.MySquad?.Identity ?? "Alpha" ) + " Squad";

	public void AddMember( Client cl )
	{
		var panel = Members.AddChild<SquadmatePanel>();
		panel.SetClient( cl );

		Panels.Add( panel );
	}

	protected List<SquadmatePanel> Panels { get; set; } = new();

	public override void Tick()
	{
		base.Tick();

		var squad = SquadManager.MySquad;

		int i = 0;
		foreach ( var panel in Panels )
		{
			if ( squad.Members.Count > i && panel.Client != squad.Members[i] )
			{
				panel.SetClient( squad.Members[i] );
			}

			i++;
		}
	}
}
