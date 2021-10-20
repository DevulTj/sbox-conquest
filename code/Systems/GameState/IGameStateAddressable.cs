namespace Conquest
{
	/// <summary>
	/// Marked on entities that react to game state changes
	/// </summary>
	public interface IGameStateAddressable
	{
		/// <summary>
		/// On reset, such as the game restarting
		/// </summary>
		public void ResetState();
	}
}
