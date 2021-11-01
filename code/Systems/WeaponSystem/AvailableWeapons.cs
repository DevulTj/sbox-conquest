

namespace Conquest
{
	public static class AvailableWeapons
	{
		public static class Primary
		{
			public static string AK47 = "conquest_ak47";
			public static string M4A1 = "conquest_m4a1";
		}

		public static class Secondary
		{
			public static string MR96 = "conquest_mr96";
		}

		public static string[] Primaries = new string[]
		{
			Primary.M4A1,
			Primary.AK47
		};

		public static string[] Secondaries = new string[]
		{
			Secondary.MR96
		};
	}
}
