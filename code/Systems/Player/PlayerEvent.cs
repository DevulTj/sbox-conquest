using Sandbox;

namespace Conquest
{
	public class PlayerEvent
	{
		public const string OnMoneyChanged = "OnMoneyChanged";
		public const string OnJobChanged = "OnJobChanged";
		public const string OnPayday = "OnPayday";

		public class OnMoneyChangedAttribute : EventAttribute
		{
			public OnMoneyChangedAttribute() : base( OnMoneyChanged ) { }
		}

		public class OnJobChangedAttribute : EventAttribute
		{
			public OnJobChangedAttribute() : base( OnJobChanged ) { }
		}
		public class OnPaydayAttribute : EventAttribute
		{
			public OnPaydayAttribute() : base( OnPayday ) { }
		}
	}
}
