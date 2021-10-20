using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Conquest
{
	public partial class ChatEntry : Panel
	{
		public DateTime Time { get; set; } = DateTime.Now;

		public Label TimestampLabel { get; internal set; }
		public Label NameLabel { get; internal set; }
		public Label Message { get; internal set; }
		public Image Avatar { get; internal set; }

		public RealTimeSince TimeSinceBorn = 0;

		public ChatEntry()
		{
			Avatar = Add.Image();
			TimestampLabel = Add.Label( "Timestamp", "time" );
			NameLabel = Add.Label( "Name", "name" );
			Message = Add.Label( "Message", "message" );
		}

		public override void Tick() 
		{
			base.Tick();

			if ( TimeSinceBorn > 10 ) 
			{ 
				Delete();
			}
		}
	}
}
