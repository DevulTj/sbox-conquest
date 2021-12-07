
using Sandbox;
using Sandbox.UI;
using System;

namespace Conquest.UI;

[Library( "ConquestPanel", Alias = new string[] { "cdiv" } ) ]
public partial class ConquestPanel : Panel
{

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		switch ( name )
		{
			case "gamestate":
				{
					BindGameState( value );

					break;
				};
			case "notgamestate":
				{
					BindNotGameState( value );

					break;
				}
		}
	}

	private void BindGameState( string identifier )
	{
		BindClass( "visible", () => GameState.Current.Identifier == identifier );
	}

	private void BindNotGameState( string identifier )
	{
		BindClass( "visible", () => GameState.Current.Identifier != identifier );
	}
}
