using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Conquest.UI
{
	public partial class PlayerHudMarker : HudMarker
	{
		public Player Player { get; set; }

		public PlayerHudMarker( Player player ) : base()
		{
			Player = player;
			Entity = Player;

			PositionOffset = new Vector3( 0, 0, 96f );
			StayOnScreen = true;

			AddClass( "player" );
		}

		public override void Refresh()
		{
			var localPlayer = Local.Pawn as Player;

			// SetMarkerClass( "hidden", isHidden );

			// No need to do anything else if we're hidden.
			//if ( isHidden )
			//	return;

			// Test against the local player and the player in question.
			var friendState = TeamSystem.GetFriendState( localPlayer.Team, Player.Team );

			SetMarkerClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
			SetMarkerClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );

			MarkerName = Player.Client.Name;
		}
	}
}
