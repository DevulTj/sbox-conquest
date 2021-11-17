using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conquest;

public class HudMarkerBuilder
{
	public Dictionary<string, bool> Classes { get; set; } = new();
	public Vector3 Position { get; set; } = new();
	public Rotation Rotation { get; set; } = new();
	public string Text { get; set; } = "";
	public bool StayOnScreen { get; set; } = false;
}

public interface IHudMarkerEntity
{
	public string GetMainClass();
	public bool Update( ref HudMarkerBuilder info );
}
