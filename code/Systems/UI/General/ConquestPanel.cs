
using Sandbox;
using Sandbox.UI;
using System;

namespace Conquest.UI;

[Library( "ConquestPanel", Alias = new string[] { "cdiv" } ) ]
public partial class ConquestPanel : Panel
{
	public ConquestPanel()
	{
		StyleSheet.Parse( ".cdiv { opacity: 0; &.active { opacity: 1; } }" );
	}

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

	protected bool AssignVis( string identifier, bool invert = false )
	{
		if ( invert )
		{

			return GameState.Current.Identifier != identifier;
		}
		else
		{
			return GameState.Current.Identifier == identifier;
		}
	}

	private void BindGameState( string identifier )
	{
		BindClass( "active", () => AssignVis( identifier ) );
	}

	private void BindNotGameState( string identifier )
	{
		BindClass( "active", () => AssignVis( identifier, true ) );
	}
}
