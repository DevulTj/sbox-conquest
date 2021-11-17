using Sandbox.UI;

namespace Conquest;

public class BaseHud : RootPanel
{
	public BaseHud()
	{
	}

	public override void OnDeleted()
	{
		DeleteChildren( true );
		base.OnDeleted();
	}
}
