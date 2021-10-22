using Sandbox;

namespace Conquest
{
	public class PlayerEvent
	{

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
