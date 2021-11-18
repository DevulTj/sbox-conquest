using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Conquest;

[Flags]
public enum FireMode
{
	Automatic = 1 << 0,
	Semi = 1 << 1,
	Burst = 1 << 2
}

public enum HoldType
{
	None = 0,
	Pistol = 1,
	Rifle = 2,
	Shotgun = 3,
	Default = 4
}

public enum ScreenShakeType
{
	Perlin,
	Random
}

public struct ScreenShakeData
{
	[Property] public ScreenShakeType Type { get; set; }
	[Property] public float Length { get; set; }
	[Property] public float Speed { get; set; }
	[Property] public float Size { get; set; }
	[Property] public float Rotation { get; set; }

	public void Run()
	{
		switch ( Type )
		{
			case ScreenShakeType.Perlin:
				{
					new Sandbox.ScreenShake.Perlin( Length, Speed, Size, Rotation );
					break;
				};
			case ScreenShakeType.Random:
			default:
				{
					new Sandbox.ScreenShake.Random( Length, Speed, Size );
					break;
				};
		}
	}
}

[Library( "winfo" ), AutoGenerate]
public class WeaponInfoAsset : Asset
{
	public static Dictionary<string, WeaponInfoAsset> Registry { get; set; } = new();

	#region Configuration

	// Important
	[Property, Category( "Important" )] public string WeaponClass { get; set; } = "";
	[Property, Category( "Important" )] public WeaponSlot Slot { get; set; } = WeaponSlot.Primary;
	[Property, Category( "Important" )] public AmmoType AmmoType { get; set; } = AmmoType.Rifle;
	[Property, Category( "Important" )] public FireMode DefaultFireMode { get; set; } = FireMode.Automatic;
	[Property( "Supported Fire Modes" ), Category( "Important" ), BitFlags] public FireMode SupportedfireModes { get; set; }
	[Property, Category( "Important" )] public int BurstAmount { get; set; } = 3;
	[Property, Category( "Important" )] public HoldType HoldType { get; set; } = HoldType.Rifle;


	// Weapon Stats
	[Property( Title = "Rounds Per Minute" ), Category( "Stats" )] public int RPM { get; set; } = 600;
	[Property, Category( "Stats" )] public float ReloadTime { get; set; } = 2f;
	[Property, Category( "Stats" )] public int ClipSize { get; set; } = 30;
	[Property, Category( "Stats" )] public bool AllowChamberReload { get; set; } = false;

	[Property, Category( "Stats" )] public float BulletSpread { get; set; } = 0f;
	[Property, Category( "Stats" )] public float BulletBaseDamage { get; set; } = 30f;
	[Property, Category( "Stats" )] public float BulletRadius { get; set; } = 1f;
	[Property, Category( "Stats" )] public int Pellets { get; set; } = 1;
	[Property, Category( "Stats" )] public bool ReloadSingle { get; set; } = false;
	[Property, Category( "Stats" )] public float BulletRange { get; set; } = 5000f;
	[Property, Category( "Stats" )] public float SurfacePassthroughAmount { get; set; } = 5;

	// Hands
	[Property, Category( "Hands" )] public bool UseCustomHands { get; set; } = false;
	[Property, Category( "Hands" ), ResourceType( "vmdl" )] public string HandsAsset { get; set; } = "weapons/swb/hands/rebel/v_hands_rebel.vmdl";

	// Models
	[Property, Category( "Models" ), ResourceType( "vmdl" )] public string ViewModel { get; set; } = "";
	[Property, Category( "Models" ), ResourceType( "vmdl" )] public string WorldModel { get; set; } = "";

	// VFX
	[Property, Category( "VFX" ), ResourceType( "vpcf" )] public string EjectParticle { get; set; } = "particles/pistol_ejectbrass.vpcf";
	[Property, Category( "VFX" ), ResourceType( "vpcf" )] public string MuzzleFlashParticle { get; set; } = "particles/swb/muzzle/flash_medium.vpcf";

	// Screen shake
	[Property, Category( "Screen Shake" )] public ScreenShakeData ScreenShake { get; set; }

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

	// Animation
	[Property, Category( "Animation" )] public string AttackAnimBool { get; internal set; } = "fire";
	#endregion

	protected override void PostLoad()
	{
		base.PostLoad();

		Log.Info( "Conquest", "Loading weapon info" );

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
