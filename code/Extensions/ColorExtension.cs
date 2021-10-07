using Sandbox;

namespace Conquest.ExtensionMethods
{
	public static class ColorExtensions
	{
		public static Color FixColor( this Color input )
		{
			if ( input.r > 1 || input.g > 1 || input.b > 1 )
			{
				return new Color( input.r / 255f, input.g / 255f, input.b / 255f );
			}

			return input;
		}
	}
}
