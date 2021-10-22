using Sandbox;

namespace Conquest
{
	public class PlayerEvent
	{
		public class Server
		{
			public const string OnPlayerKilled = "OnPlayerKilled";

			public class OnPlayerKilledAttribute : EventAttribute
			{
				public OnPlayerKilledAttribute() : base( OnPlayerKilled ) { }
			}
		}

		public class Client
		{
			public const string OnAwardGiven = "OnAwardGiven";

			public class OnAwardGivenAttribute : EventAttribute
			{
				public OnAwardGivenAttribute() : base( OnAwardGiven ) { }
			}
		}
	}
}
