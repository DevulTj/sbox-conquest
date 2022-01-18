using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest;

partial class PlayerInventory : IBaseInventory
{
	public virtual int MaxSlots => 5;

	public virtual int MaxGadgets => 2;

	public Player Owner { get; init; }

	// 0
	public Carriable PrimaryWeapon { get; set; }

	// 1
	public Carriable SecondaryWeapon { get; set; }

	// 2
	public Carriable MeleeWeapon { get; set; }

	// 3-♾️
	public List<Carriable> Gadgets { get; set; } = new();

	public PlayerInventory( Player player )
	{
		Owner = player;
	}

	public Entity Active => Owner.ActiveChild;

	public bool Add( Entity ent, bool makeactive = false )
	{
		Host.AssertServer();

		//
		// Can't pickup if already owned
		//
		if ( ent.Owner != null )
			return false;

		//
		// Let the entity reject the inventory
		//
		if ( !ent.CanCarry( Owner ) )
			return false;

		var weapon = ent as Carriable;

		switch( weapon.Slot )
		{
			case WeaponSlot.Primary:
			{
				if ( PrimaryWeapon.IsValid() )
					return false;

				PrimaryWeapon = weapon;
				break;
			};
			case WeaponSlot.Secondary:
			{
				if ( SecondaryWeapon.IsValid() )
					return false;

				SecondaryWeapon = weapon;
				break;
			};
			case WeaponSlot.Melee:
			{
				if ( MeleeWeapon.IsValid() )
					return false;

				MeleeWeapon = weapon;
				break;
			};
			case WeaponSlot.Gadget:
			{
				if ( Gadgets.Count >= MaxGadgets )
					return false;

				Gadgets.Add( weapon );
				break;
			};
		}

		ent.OnCarryStart( Owner );

		if ( makeactive )
			SetActive( ent );

		return true;
	}

	public bool Contains( Entity ent )
	{
		if ( ent == PrimaryWeapon )
			return true;
		if ( ent == SecondaryWeapon )
			return true;
		if ( ent == MeleeWeapon )
			return true;
		if ( Gadgets.Contains( ent ) )
			return true;

		return false;
	}

	public int Count()
	{
		var count = 0;

		if ( PrimaryWeapon is not null )
			++count;
		if ( SecondaryWeapon is not null )
			++count;
		if ( MeleeWeapon is not null )
			++count;

		count += Gadgets.Count;

		return count;
	}

	public void DeleteContents()
	{
		Host.AssertServer();

		PrimaryWeapon?.Delete();
		SecondaryWeapon?.Delete();
		MeleeWeapon?.Delete();

		for ( int i = Gadgets.Count - 1; i >= 0; i-- )
		{
			Gadgets[i]?.Delete();
		}
	}

	public bool Drop( Entity ent )
	{
		if ( !Host.IsServer )
			return false;

		if ( !Contains( ent ) )
			return false;

		ent.OnCarryDrop( Owner );

		return ent.Parent == null;
	}

	public Entity DropActive()
	{
		if ( !Host.IsServer ) return null;

		var ac = Owner.ActiveChild;
		if ( !ac.IsValid() ) return null;

		if ( Drop( ac ) )
		{
			Owner.ActiveChild = null;
			return ac;
		}

		return null;
	}

	public int GetActiveSlot()
	{
		if ( Active == PrimaryWeapon )
			return (int)WeaponSlot.Primary;
		if ( Active == SecondaryWeapon )
			return (int)WeaponSlot.Secondary;
		if ( Active == MeleeWeapon )
			return (int)WeaponSlot.Melee;

		for ( int i = 0; i < Gadgets.Count; i++ )
		{
			var gadget = Gadgets[i];
			if ( gadget == Active )
				return (int)WeaponSlot.Gadget + i;
		}

		return -1;
	}

	protected Entity GetGadget( int i )
	{
		var realIndex = i - (int)WeaponSlot.Gadget;

		if ( Gadgets.Count > realIndex )
			return Gadgets[realIndex];

		return null;
	}

	public Entity GetSlot( int i )
	{
		return i switch
		{
			0 => PrimaryWeapon,
			1 => SecondaryWeapon,
			2 => MeleeWeapon,
			_ => GetGadget( i )
		};
	}

	public void OnChildAdded( Entity child )
	{
		if ( child is not Carriable weapon )
			return;

		switch( weapon.Slot )
		{
			case WeaponSlot.Primary:
			{
				PrimaryWeapon = weapon;
				break;
			};
			case WeaponSlot.Secondary:
			{
				SecondaryWeapon = weapon;
				break;
			};
			case WeaponSlot.Melee:
			{
				MeleeWeapon = weapon;
				break;
			};
			case WeaponSlot.Gadget:
			{
				Gadgets.Add( weapon );
				break;
			};
		}
	}

	public void OnChildRemoved( Entity child )
	{
		if ( child is not Carriable weapon )
			return;

		switch ( weapon.Slot )
		{
			case WeaponSlot.Primary:
			{
				PrimaryWeapon = null;
				break;
			};
			case WeaponSlot.Secondary:
			{
				SecondaryWeapon = null;
				break;
			};
			case WeaponSlot.Melee:
			{
				MeleeWeapon = null;
				break;
			};
			case WeaponSlot.Gadget:
			{
				Gadgets.Remove( weapon );
				break;
			};
		}
	}

	public bool SetActive( Entity ent )
	{
		if ( Active == ent )
		{
			return false;
		}

		if ( !Contains( ent ) )
		{
			return false;
		}

		Owner.ActiveChild = ent;

		return true;
	}

	public bool SetActiveSlot( int i, bool allowempty = false )
	{
		var ent = GetSlot( i );

		Log.Info( ent );

		if ( Owner.ActiveChild == ent )
			return false;

		if ( !allowempty && ent == null )
			return false;

		Owner.ActiveChild = ent;
		return ent.IsValid();
	}

	public bool SwitchActiveSlot( int idelta, bool loop )
	{
		var count = Count();
		if ( count == 0 ) return false;

		var slot = GetActiveSlot();
		var nextSlot = slot + idelta;

		if ( loop )
		{
			while ( nextSlot < 0 ) nextSlot += count;
			while ( nextSlot >= count ) nextSlot -= count;
		}
		else
		{
			if ( nextSlot < 0 ) return false;
			if ( nextSlot >= count ) return false;
		}

		Log.Info( nextSlot );

		return SetActiveSlot( nextSlot, false );
	}
}
