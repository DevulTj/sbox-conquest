﻿using Sandbox;
using System;

namespace Conquest
{
	public partial class Slide : BaseNetworkable
	{
		[Net, Predicted] public bool IsActive { get; set; }
		[Net, Predicted] public bool Wish { get; set; }
		[Net, Predicted] public float BoostTime { get; set; } = 1f;
		[Net, Predicted] public bool IsDown { get; set; }
		[Net, Predicted] TimeSince Activated { get; set; } = 0;

		public virtual float TimeUntilStop => 0.8f;
		// You can only slide once every X
		public virtual float Cooldown => 1f;
		public virtual float MinimumSpeed => 64f;
		public virtual float WishDirectionFactor => 1200f;
		public virtual float SlideIntensity => 1 - (Activated / BoostTime);

		public Slide()
		{
		}

		public virtual void PreTick( BasePlayerController controller )
		{
			var downBefore = IsDown;

			IsDown = Input.Down( InputButton.Duck ) && ( !Input.Down( InputButton.Attack2 ) );

			var oldWish = Wish;
			Wish = IsDown;

			if ( controller.Velocity.Length <= MinimumSpeed )
				StopTry();

			// No sliding while you're already in the sky
			if ( controller.GroundEntity == null )
				StopTry();

			if ( Activated > TimeUntilStop )
				StopTry();

			if ( oldWish == Wish )
				return;

			if ( IsDown != IsActive )
			{
				if ( IsDown ) Try( controller );
				else StopTry();
			}

			if ( IsActive )
				controller.SetTag( "sliding" );
		}

		public Vector3 WishDirOnStart { get; set; }

		void Try( BasePlayerController controller )
		{
			if ( Activated < Cooldown )
				return;

			var change = IsActive != true;

			IsActive = true;

			if ( change )
			{
				Activated = 0;
				WishDirOnStart = controller.WishVelocity.Normal;
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

		internal void Accelerate( BasePlayerController controller, ref Vector3 wishdir, ref float wishspeed, ref float speedLimit, ref float acceleration )
		{
			var hitNormal = controller.GroundNormal;
			var speedMult = Vector3.Dot( controller.Velocity.Normal, Vector3.Cross( controller.Rotation.Up, hitNormal ) );

			//wishdir *= WishDirectionFactor;

			wishdir = WishDirOnStart * ( WishDirectionFactor * Time.Delta );

			if ( BoostTime > Activated )
				speedMult -= 1 - (Activated / BoostTime);

			controller.Velocity += wishdir + (controller.Velocity.Normal * MathF.Abs( speedMult ) * 20);
		}
	}
}
