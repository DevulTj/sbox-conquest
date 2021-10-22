
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Conquest
{
	[Library("AwardImagePanel")]
	public class AwardImagePanel : Panel
	{
		public TimeSince CreationTime { get; set; } = 0;
		public float TimeToShow { get; set; } = 5f;

		public override void Tick()
		{
			base.Tick();
			
			if ( CreationTime > TimeToShow && !IsDeleting )
			{
				Delete();
			}
		}
	}

	public class AwardDescriptionPanel : Panel
	{
		public AwardDescriptionPanel()
		{
			Label = Add.Label( "PLAYER KILLED	15 XP" );
		}

		public TimeSince CreationTime { get; set; } = 0;
		public float TimeToShow { get; set; } = 5f;

		public Label Label { get; set; }

		public override void Tick()
		{
			base.Tick();

			if ( CreationTime > TimeToShow && !IsDeleting )
			{
				Delete();
			}
		}
	}

	[UseTemplate]
	public class AwardsPanel : Panel
	{
		// @ref
		public Panel RootPanel { get; set; }

		public Panel Images { get; set; }
		public Panel Awards { get; set; }

		public override void Tick()
		{
			base.Tick();
		}


		[PlayerEvent.Client.OnAwardGiven]
		protected void OnAwardGiven( PlayerAward award )
		{
			var awardCoin = Images.AddChild<AwardImagePanel>( award.Title.ToLower() );
			awardCoin.Style.SetBackgroundImage( award.IconTexture );

			var awardText = Awards.AddChild<AwardDescriptionPanel>( award.Title.ToLower() );
			awardText.Label.SetText( $"{award.Description}	{award.PointsGiven} XP" );
		}
	}
}
