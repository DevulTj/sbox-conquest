
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	[Library( "SquadmatePanel" )]
	public class SquadmatePanel : Panel
	{

		public SquadmatePanel()
		{
			PlayerName = Add.Label( "DevulTj", "name" );
			Avatar = Add.Image( "avatarbig:76561197973858781", "avatar" );
			Bar = Add.Panel( "bar" );
			FillBar = Add.Panel( "fill" );
		}

		public void SetClient( Client cl )
		{
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

		public override void Tick()
		{
			base.Tick();
		}
	}

	[UseTemplate]
	public class SquadOverview : Panel
	{
		public SquadOverview()
		{

		}

		// @ref
		public Panel RootPanel { get; set; }

		public Panel Members { get; set; }

		public string SquadName => ( SquadManager.MySquad?.Identity ?? "Alpha" ) + " Squad";

		public void AddMember( Client cl )
		{
			var panel = Members.AddChild<SquadmatePanel>();
			panel.SetClient( cl );
		}

		public override void Tick()
		{
			Members.DeleteChildren( true );

			var squad = SquadManager.MySquad;

			if ( squad is not null )
			{
				foreach( var member in squad.Members )
				{
					AddMember( member );
				}
			}
		}
	}
}
