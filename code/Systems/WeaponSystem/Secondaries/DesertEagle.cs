using SandboxEditor;
using Sandbox;

namespace Conquest;

[Library( "conquest_deserteagle", Title = "Desert Eagle" )]
[EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
partial class DesertEagle : BaseWeapon
{
	public override Vector3 RecoilOnShot => new Vector3( Rand.Float( -40f, 40f ), 100f, 0 );
	public override float RecoilRecoveryScaleFactor => base.RecoilRecoveryScaleFactor * 2f;
}
