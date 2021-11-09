
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Conquest
{
	[Library("CriticalInformationEntry")]
	public class CriticalInformationEntry : Panel
	{
		public TimeSince CreationTime { get; set; } = 0;
		public float TimeToShow { get; set; } = 5f;

		public Label Text { get; set; }

		public CriticalInformationEntry()
		{
			Text = Add.Label( "..." );
		}

		public override void Tick()
		{
			base.Tick();

			if ( CreationTime > TimeToShow && !IsDeleting )
			{
				Delete();
			}
		}
	}

	[UseTemplate]
	public partial class CritPanel : Panel
	{
		public static CritPanel Current;

		public CriticalInformationEntry ActiveEntry { get; set; }

		public CritPanel()
		{
			Current = this;
		}

		[ServerCmd( "conquest_notifycritical" )]
		public static void AddInformation( string message )
		{
			SendMessage( To.Everyone, message );
		}

		[ClientCmd( "conquest_sendcritical", CanBeCalledFromServer = true )]
		public static void SendMessage( string message )
		{
			Current?.AddEntry( message );
		}

		public virtual Panel AddEntry( string text )
		{
			if ( ActiveEntry is not null )
			{
				ActiveEntry.Delete();
			}

			var e = Current.AddChild<CriticalInformationEntry>();

			e.Text.Text = $"{text}";

			return e;
		}
	}
}
