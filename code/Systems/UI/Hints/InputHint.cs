

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
	public string Text { get; set; }
	public Label ActionLabel { get; set; }

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "btn" )
		{
			SetButton( Enum.Parse<InputButton>( value, true ) );
		}
	}

	public void SetButton( InputButton button )
	{
		Button = button;
	}

	public override void SetContent( string value )
	{
		base.SetContent( value );

		ActionLabel.SetText( value );
		Text = value;
	}

	public override void Tick()
	{
		base.Tick();

		Glyph.Texture = Input.GetGlyph( Button );

		if ( Glyph.Texture != null )
		{
			Glyph.Style.Width = Glyph.Texture.Width;
			Glyph.Style.Height = Glyph.Texture.Height;
		}
	}
}
