using Sandbox;

namespace Conquest
{
	public class GameEvent
	{
		public class Server
		{
			public const string OnChatMessage = "OnChatMessage";
			public class OnChatMessageAttribute : EventAttribute
			{
				public OnChatMessageAttribute() : base( OnChatMessage ) { }
			}
		}

		public class Client
		{

		}

		public class Shared
		{

		}
	}
}
