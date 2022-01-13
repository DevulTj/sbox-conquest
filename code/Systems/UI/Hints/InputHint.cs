

using Sandbox;
using Sandbox.UI;
using System;

namespace Conquest.UI;

[UseTemplate]
public partial class InputHint : Panel
{
	// @ref
	public Image Glyph { get; set; }

	public InputButton Button { get; set; } 

	public string Content { get; set; }

	public Label ActionLabel { get; set; }

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "btn" )
		{
			var buttonEnumValue = (InputButton)Enum.Parse( typeof( InputButton ), value );
			Button = buttonEnumValue;
		}
	}

	public override void SetContent( string value )
	{
		base.SetContent( value );
		Content = value;
	}

	public override void Tick()
	{
		base.Tick();

		ActionLabel.SetText( Content );
		Glyph.Texture = Input.GetGlyph( Button );

		if ( Glyph.Texture != null )
		{
			Glyph.Style.Width = Glyph.Texture.Width;
			Glyph.Style.Height = Glyph.Texture.Height;
		}
	}
}
