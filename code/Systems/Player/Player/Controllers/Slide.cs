using Sandbox;
using System;

namespace Conquest
{
	[Library]
	public class Slide : BaseNetworkable
	{
		public BasePlayerController Controller;

		public bool IsActive;
		public bool Wish;

		public float BoostTime = 1f;

		TimeSince Activated = 0;
		public virtual float TimeUntilStop => 0.8f;

		// You can only slide once every X
		public virtual float Cooldown => 1f;
		public virtual float MinimumSpeed => 64f;
		public virtual float WishDirectionFactor => 1200f;

		public virtual float SlideIntensity => 1 - (Activated / BoostTime);

		public bool IsDown;

		public Slide( BasePlayerController controller )
		{
			Controller = controller;
		}

		public virtual void PreTick()
		{
			var downBefore = IsDown;

			IsDown = Input.Down( InputButton.Duck ) && ( !Input.Down( InputButton.Attack2 ) );

			var oldWish = Wish;
			Wish = IsDown;

			if ( Controller.Velocity.Length <= MinimumSpeed )
				StopTry();

			// No sliding while you're already in the sky
			if ( Controller.GroundEntity == null )
				StopTry();

			if ( Activated > TimeUntilStop )
				StopTry();

			if ( oldWish == Wish )
				return;

			if ( IsDown != IsActive )
			{
				if ( IsDown ) Try();
				else StopTry();
			}

			if ( IsActive )
				Controller.SetTag( "sliding" );
		}

		public Vector3 WishDirOnStart { get; set; }

		void Try()
		{
			if ( Activated < Cooldown )
				return;

			var change = IsActive != true;

			IsActive = true;

			if ( change )
			{
				Activated = 0;
				Log.Info( "change = true" );

				WishDirOnStart = Controller.WishVelocity.Normal;
			}
		}

		void StopTry()
		{
			if ( !IsActive )
				return;

			Activated = 0;
			IsActive = false;
		}

		public float GetWishSpeed()
		{
			if ( !IsActive ) return -1;
			return 64;
		}

		internal void Accelerate( ref Vector3 wishdir, ref float wishspeed, ref float speedLimit, ref float acceleration )
		{
			var hitNormal = Controller.GroundNormal;
			var speedMult = Vector3.Dot( Controller.Velocity.Normal, Vector3.Cross( Controller.Rotation.Up, hitNormal ) );

			//wishdir *= WishDirectionFactor;

			wishdir = WishDirOnStart * ( WishDirectionFactor * Time.Delta );

			if ( BoostTime > Activated )
				speedMult -= 1 - (Activated / BoostTime);

			Controller.Velocity += wishdir + (Controller.Velocity.Normal * MathF.Abs( speedMult ) * 20);
		}
	}
}
