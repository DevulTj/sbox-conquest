using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest
{
	public class PlayerAward : LibraryMethod
	{
		public int PointsGiven { get; set; }
		public Texture IconTexture { get; set; }

		public PlayerAward( string name = "Award", int pointsGiven = 0, string description = "", string texturePath = "" )
		{
			Title = name;
			Description = description;
			PointsGiven = pointsGiven;

			if ( !string.IsNullOrEmpty( texturePath ) )
			IconTexture = Texture.Load( texturePath );
		}
	}

	public static class PlayerAwards
	{

		[PlayerAward( name: "Kill", pointsGiven: 15, description: "PLAYER KILLED", texturePath: "ui/Awards/Skull.png" )]
		public static void KillAwardGiven( Player player )
		{
		}

		[PlayerAward( name: "Capture", pointsGiven: 30, description: "POINT CAPTURED", texturePath: "ui/Awards/Capture.png" )]
		public static void CapturedAwardGiven( Player player )
		{
		}

		public static PlayerAward Get( string awardTitle )
		{
			var attribute = Library.GetAttributes<PlayerAward>()
				.Where( x => string.Equals( x.Title, awardTitle, StringComparison.OrdinalIgnoreCase ) )
				.FirstOrDefault();

			return attribute;
		}

		public static void Give( Player player, string awardTitle )
		{
			var award = Get( awardTitle );
			if ( award is null )
				return;

			award?.InvokeStatic( player );

			player.Points += award.PointsGiven;
			player.PromptAwardGiven( award.Title );
		}
	}

	partial class Player
	{
		[Net]
		public int Points { get; set; } = 0;

		public List<PlayerAward> AwardsGiven { get; set; } = new();

		public void GiveAward( string awardTitle ) => PlayerAwards.Give( this, awardTitle );

		[ClientRpc]
		public void PromptAwardGiven( string awardTitle )
		{
			var award = PlayerAwards.Get( awardTitle );
			if ( award is null )
				return;

			Event.Run( PlayerEvent.Client.OnAwardGiven, award );
		}
	}
}
