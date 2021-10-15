using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Conquest.UI
{
	public partial class PlayerHudMarker : HudMarker
	{
		public Player Player { get; set; }

		public float DistanceUntilHidden => 6000;

		public PlayerHudMarker( Player player ) : base()
		{
			Player = player;
			Entity = Player;

			PositionOffset = new Vector3( 0, 0, 80f );
			StayOnScreen = true;

			AddClass( "player" );
		}

		public override void Refresh()
		{
			var localPlayer = Local.Pawn as Player;
			var isFarAway = localPlayer.EyePos.Distance( Player.Position ) > DistanceUntilHidden;

			var tr = Trace.Ray( localPlayer.EyePos, Player.EyePos )
				.Ignore( localPlayer )
				.Run();

			var isHidden = isFarAway;
			var farAndNotLooking = isFarAway && (tr.Hit && tr.Entity != Player);

			if ( IsFocused && isHidden && !farAndNotLooking )
				isHidden = false;

			SetMarkerClass( "hidden", isHidden );

			// No need to do anything else if we're hidden.
			if ( isHidden )
				return;

			// Test against the local player and the player in question.
			var friendState = TeamSystem.GetFriendState( localPlayer.Team, Player.Team );

			SetMarkerClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
			SetMarkerClass( "enemy", friendState == TeamSystem.FriendlyStatus.Hostile );

			MarkerName = Player.Client.Name;
		}
	}
}
