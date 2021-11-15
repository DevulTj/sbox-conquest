using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Conquest
{
	[Library( "winfo" ), AutoGenerate]
	public class WeaponInfoAsset : Asset
	{
		public static Dictionary<string, WeaponInfoAsset> Registry { get; set; } = new();

		#region Configuration

		// Important
		[Property, Category( "Important" )] public string WeaponClass { get; set; } = "";
		[Property, Category( "Important" )] public WeaponSlot Slot { get; set; } = WeaponSlot.Primary;
		[Property, Category( "Important" )] public AmmoType AmmoType { get; set; } = AmmoType.Rifle;


		// Weapon Stats
		[Property( Title = "Rounds Per Minute" ), Category( "Stats" )] public int RPM { get; set; } = 600;
		[Property, Category( "Stats" )] public float ReloadTime { get; set; } = 2f;
		[Property, Category( "Stats" )] public int ClipSize { get; set; } = 30;
		[Property, Category( "Stats" )] public bool AllowChamberReload { get; set; } = false;

		[Property, Category( "Stats" )] public float BulletSpread { get; set; } = 0f;
		[Property, Category( "Stats" )] public float BulletBaseDamage { get; set; } = 30f;
		[Property, Category( "Stats" )] public float BulletRadius { get; set; } = 1f;

		// Hands
		[Property, Category( "Hands" )] public bool UseCustomHands { get; set; } = false;
		[Property, Category( "Hands" ), ResourceType( "vmdl" )] public string HandsAsset { get; set; } = "weapons/swb/hands/rebel/v_hands_rebel.vmdl";

		// Models
		[Property, Category( "Models" ), ResourceType( "vmdl" )] public string ViewModel { get; set; } = "";
		[Property, Category( "Models" ), ResourceType( "vmdl" )] public string WorldModel { get; set; } = "";


		// Sounds
		[Property, Category( "Sounds" )] public string FireSound { get; set; } = "";
		[Property, Category( "Sounds" )] public string DryFireSound { get; set; } = "";

		// UI
		[Property, Category( "UI" ), ResourceType( "png" )] public string LoadoutIcon { get; set; } = "";

		// ViewModel
		[Property, Category( "ViewModel" )] public Vector3 WalkCycleOffsets { get; set; } = new Vector3( 50f, 20f, 50f );
		[Property, Category( "ViewModel" )] public float ForwardBobbing { get; set; } = 4f;
		[Property, Category( "ViewModel" )] public float SideWalkOffset { get; set; } = 100f;
		[Property, Category( "ViewModel" )] public Vector3 AimOffset { get; set; } = new Vector3( 10f, 10, 1.8f );
		[Property, Category( "ViewModel" )] public Angles AimAngleOffset { get; set; } = new Angles( 0f, 0f, 0f );

		[Property, Category( "ViewModel" )] public Vector3 Offset { get; set; } = new Vector3( -6f, 5f, -5f );
		[Property, Category( "ViewModel" )] public Vector3 CrouchOffset { get; set; } = new Vector3( -10f, -50f, -0f );
		[Property, Category( "ViewModel" )] public float OffsetLerpAmount { get; set; } = 30f;

		[Property, Category( "ViewModel" )] public float SprintRightRotation { get; set; } = 20f;
		[Property, Category( "ViewModel" )] public float SprintUpRotation { get; set; } = -30f;
		[Property, Category( "ViewModel" )] public float SprintLeftOffset { get; set; } = -35f;
		[Property, Category( "ViewModel" )] public float PostSprintLeftOffset { get; set; } = 5f;

		[Property, Category( "ViewModel" )] public float BurstSprintRightRotation { get; set; } = 20f;
		[Property, Category( "ViewModel" )] public float BurstSprintUpRotation { get; set; } = -30f;
		[Property, Category( "ViewModel" )] public float BurstSprintLeftOffset { get; set; } = -35f;
		[Property, Category( "ViewModel" )] public float BurstPostSprintLeftOffset { get; set; } = 5f;
		#endregion

		protected override void PostLoad()
		{
			base.PostLoad();

			Log.Info( "[Conquest] loading weapon info" );

			if ( string.IsNullOrEmpty( WeaponClass ) )
			{
				return;
			}

			var libraryAttribute = Library.GetAttribute( WeaponClass );
			if ( libraryAttribute is not null )
			{
				Registry[WeaponClass] = this;
			}
		}
	}
}
