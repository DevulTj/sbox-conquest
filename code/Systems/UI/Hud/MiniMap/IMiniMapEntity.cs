namespace Conquest;

public interface IMiniMapEntity
{
	public string GetMainClass();
	public bool Update( ref MiniMapDotBuilder info );
}
