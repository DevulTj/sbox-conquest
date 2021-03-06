using Sandbox;

namespace Conquest;

[Library( "conquest_deployableammocrate", Title = "Ammo Crate" )]
partial class AmmoCrateGadget : BaseGadget
{
	[Net] public AmmoCrateEntity CurrentAmmoCrate { get; set; }

	public override WeaponSlot Slot => WeaponSlot.Gadget;

	// Only if there's no crates
	public override bool CanPrimaryAttack()
	{
		if ( !base.CanPrimaryAttack() )
			return false;

		return !CurrentAmmoCrate.IsValid();
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( Host.IsClient )
			return;

		using ( Prediction.Off() )
		{
			var entity = new AmmoCrateEntity()
			{

				Position = Owner.EyePosition + Owner.EyeRotation.Forward * 50 + Owner.EyeRotation.Down * 10f,
				Rotation = Owner.EyeRotation
			};

			entity.Velocity = Owner.EyeRotation.Forward * 500;

			CurrentAmmoCrate = entity;
		}
	}
}
