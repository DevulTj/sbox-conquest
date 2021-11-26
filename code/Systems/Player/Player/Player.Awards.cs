using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest;

public class PlayerAward : LibraryMethod
{
	public int PointsGiven { get; set; }
	public string IconTexture { get; set; }
}

public static class PlayerAwards
{

	[PlayerAward( Title = "Kill", PointsGiven = 15, Description = "ENEMY KILLED", IconTexture = "ui/Awards/Skull.png" )]
	public static void KillAwardGiven( Player player )
	{
		player.Client.AddInt( "frags", 1 );
	}

	[PlayerAward( Title = "TeamKill", PointsGiven = -30, Description = "TEAMKILLED", IconTexture = "ui/Awards/TeamKill.png" )]
	public static void TeamKillAwardGiven( Player player )
	{
		player.Client.AddInt( "frags", -1 );
	}

	[PlayerAward( Title = "Capture", PointsGiven = 30, Description = "POINT CAPTURED", IconTexture = "ui/Awards/Capture.png" )]
	public static void CapturedAwardGiven( Player player )
	{
		player.Client.AddInt( "captures", 1 );
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
		player.PromptAwardGiven( To.Single( player.Client ), award.Title );
	}
}

partial class Player
{
	public int Points { get => Client.GetInt( "score" ); set => Client.SetInt( "score", value ); }

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
