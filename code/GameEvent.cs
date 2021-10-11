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
			public const string OnScoreChanged = "OnScoreChanged";

			public class OnScoreChangedAttribute : EventAttribute
			{
				public OnScoreChangedAttribute() : base( OnScoreChanged ) { }
			}
		}
	}
}
