
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest
{
	public interface IMiniMapEntity
	{
		public string GetMainClass();
		public bool Update( ref MiniMapDotBuilder info );
	}
}
