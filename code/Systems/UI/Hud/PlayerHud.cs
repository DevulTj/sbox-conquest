﻿using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace Conquest;

[UseTemplate("systems/ui/hud/playerhud.html")]
public class PlayerHud : BaseHud
{
	public static PlayerHud Current { get; set; } 

	public Panel Root { get; set; }
	public Panel Vitals { get; set; }
	public Label Health { get; set; }
	public Panel HealthBar { get; set; }
	public NameTags Tags { get; set; }

	// weapon info
	public Panel GunVitals { get; set; }
	public Label GunAmmo { get; set; }
	public Label GunReserve { get; set; }

	public Label GunName { get; set; }

	public Label PlayerName { get; set; }

	public Panel Inventory { get; set; }

	public List<InventoryItem> Items { get; set; } = new();

	public Panel Main { get; set; }

	public Panel LeftObjects { get; set; }
	public Panel RightObjects { get; set; }

	// @ref
	public RespawnScreen RespawnScreen { get; set; }

	protected virtual bool ShouldShowMainPanel()
	{
		var player = Local.Pawn as Player;

		if ( !player.IsValid() )
			return false;

		if ( player.LifeState != LifeState.Alive )
			return false;

		return true;
	}

	public PlayerHud()
	{
		Current = this;

		for (int i = 0; i < 5; i++ )
		{
			var item = Inventory?.AddChild<InventoryItem>( i < 2 ? "large" : "small" );
			if ( item != null )
			{
				item.SlotIndex = i;
				Items.Add( item );
			}
		}

		RespawnScreen.BindClass( "active", () => Local.Pawn is not Player );
		Main.BindClass( "active", () => ShouldShowMainPanel() );
	}


	float Forward;
	float Left;

	float storedHpPercent = 1;

	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn as Player;
		if ( !player.IsValid() )
			return;

		var controller = player.Controller;

		if ( controller is not null )
		{
			Forward = Forward.LerpTo( (Input.Forward * controller.Velocity.Length) * 0.005f, Time.Delta * 10f );
			Left = Left.LerpTo( (Input.Left * controller.Velocity.Length) * 0.005f, Time.Delta * 10f );

			var panelTransform = new PanelTransform();
			panelTransform.AddRotation( Left, Left, 0 );
			panelTransform.AddRotation( Forward, -Forward, 0 );

			LeftObjects.Style.Transform = panelTransform;
			var righttransform = new PanelTransform();
			righttransform.AddRotation( Left, -Left, 0 );
			righttransform.AddRotation( Forward, -Forward, 0 );

			RightObjects.Style.Transform = righttransform;
		}

		if ( Health == null )
			return;
		
		Health.Text = $"{player?.Health:n0}";
		var healthPercent = ( player.Health / 100f ) * 100f;

		storedHpPercent = storedHpPercent.LerpTo( healthPercent, Time.Delta * 10f );
		HealthBar.Style.Width = Length.Percent( storedHpPercent );

		HealthBar.SetClass( "hurt", healthPercent < 0.4 );
		HealthBar.SetClass( "dying", healthPercent < 0.2 );

		var weapon = Local.Pawn.ActiveChild as BaseWeapon;

		if ( Local.Pawn.ActiveChild is MeleeWeapon melee )
		{
			GunVitals.Style.Opacity = 1;

			GunName.Text = $"{melee.ClassInfo.Title.ToUpper()}";
			GunAmmo.Text = $"∞";
			GunReserve.Text = $"∞";
			GunVitals.Style.Opacity = 1;
		}
		else if ( weapon is not null && weapon.ShowAmmoCount )
		{
			GunName.Text = $"{weapon.ClassInfo.Title.ToUpper()}";
			GunAmmo.Text = $"{weapon.AmmoClip.ToString("D3")}";
			GunReserve.Text = $"{weapon.AvailableAmmo().ToString("D3")}";
			GunVitals.Style.Opacity = 1;
		}
		else if ( Local.Pawn.ActiveChild is BaseGadget gadget )
		{
			GunVitals.Style.Opacity = 1;

			GunName.Text = $"{gadget.ClassInfo.Title.ToUpper()}";
			GunAmmo.Text = $"∞";
			GunReserve.Text = $"∞";
			GunVitals.Style.Opacity = 1;
		}
		else
		{
			GunVitals.Style.Opacity = 0;
		}

		PlayerName.Text = $"{player.Client.Name.ToUpper()}";
	}
}
