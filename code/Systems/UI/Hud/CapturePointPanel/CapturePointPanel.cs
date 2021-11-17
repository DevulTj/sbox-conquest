using Sandbox;
using Sandbox.UI;

namespace Conquest;

[UseTemplate]
public class CapturePointPanel : Panel
{
	public CapturePointEntity CapturePoint { get; set; }

	public string NiceName { get; set; } = "POINT NAME";
	public string Identity { get; set; } = "A";

	public void SetShowName( bool shouldShowName = true )
	{
		SetClass( "withName", shouldShowName );
	}

	public override void Tick()
	{
		base.Tick();

		if ( CapturePoint.IsValid() )
		{
			NiceName = CapturePoint.NiceName.ToUpper();
			Identity = CapturePoint.Identity.ToUpper();

			foreach( var kv in CapturePoint.GetUIClasses() )
			{
				SetClass( kv.Key, kv.Value );
			}
		}
	}
}
