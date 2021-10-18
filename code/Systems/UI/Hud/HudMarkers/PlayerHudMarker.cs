using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Conquest.UI
{
	public partial class PlayerHudMarker : HudMarker
	{
		public Player Player { get; set; }

		public float DistanceUntilHidden => 6000;
		public float DistanceUntilShownEnemy => 1000;

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
			if ( Player is null || ( Player is not null && Player.LifeState != LifeState.Alive ) )
			{
				SetMarkerClass( "hidden", true );
				return;
			}

			var localPlayer = Local.Pawn as Player;
			var isFarAway = localPlayer.EyePos.Distance( Player.Position ) > DistanceUntilHidden;
			var isCloseEnoughEnemy = localPlayer.EyePos.Distance( Player.Position ) < DistanceUntilShownEnemy;


			var tr = Trace.Ray( localPlayer.EyePos, Player.EyePos )
				.Ignore( localPlayer )
				.Run();

			var isHidden = isFarAway;
			var farAndNotLooking = isFarAway && (tr.Hit && tr.Entity != Player);

			if ( IsFocused && isHidden && !farAndNotLooking )
				isHidden = false;

			// Test against the local player and the player in question.
			var friendState = TeamSystem.GetFriendState( localPlayer.Team, Player.Team );

			var isEnemy = friendState == TeamSystem.FriendlyStatus.Hostile;

			if ( isEnemy && ( !IsFocused || !isCloseEnoughEnemy ) )
				isHidden = true;

			SetMarkerClass( "hidden", isHidden );

			// No need to do anything else if we're hidden.
			if ( isHidden )
				return;

				

			SetMarkerClass( "friendly", friendState == TeamSystem.FriendlyStatus.Friendly );
			SetMarkerClass( "enemy", isEnemy );

			MarkerName = Player.Client.Name;
		}
	}
}
