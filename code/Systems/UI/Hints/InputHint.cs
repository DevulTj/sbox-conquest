

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

	protected bool IsSet = false;

	public InputHint()
	{
		BindClass( "noaction", () => string.IsNullOrEmpty( Text ) );
	}

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
		IsSet = true;
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

		if ( IsSet )
		{
			Texture glyphTexture = Input.GetGlyph( Button, InputGlyphSize.Medium, GlyphStyle.Knockout.WithSolidABXY().WithNeutralColorABXY() );
			if ( glyphTexture != null )
			{
				Glyph.Texture = glyphTexture;
				Glyph.Style.Width = glyphTexture.Width / 2f;
				Glyph.Style.Height = glyphTexture.Height / 2f;
			}
			else
			{
				Glyph.Texture = Texture.Load( FileSystem.Mounted, "/ui/Input/invalid_glyph.png" );
			}
		}
	}
}
