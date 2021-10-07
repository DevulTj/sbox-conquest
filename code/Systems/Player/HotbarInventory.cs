using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conquest
{
	partial class PlayerInventory : BaseInventory
	{
		public PlayerInventory( Player player ) : base( player ) { }

		public override bool Add( Entity ent, bool makeActive = false )
		{
			var player = Owner as Player;
			var inventoryItem = ent as BaseWeapon;
			var notices = true;
			//
			// We don't want to pick up the same weapon twice
			// But we'll take the ammo from it Winky Face
			//
			if ( inventoryItem is not null && IsCarryingType( ent.GetType() ) )
			{
				if ( inventoryItem is BaseWeapon baseWeapon )
				{
					var ammo = baseWeapon.AmmoClip;
					var ammoType = baseWeapon.AmmoType;

					if ( ammo > 0 )
					{
						player.GiveAmmo( ammoType, ammo );

						if ( notices )
						{
							Sound.FromWorld( "dm.pickup_ammo", ent.Position );
						}
					}
				}

				// Despawn it
				ent.Delete();
				return false;
			}

			if ( inventoryItem is not null && notices )
			{
				Sound.FromWorld( "dm.pickup_weapon", ent.Position );
			}

			return base.Add( ent, makeActive );
		}

		public bool IsCarryingType( Type t )
		{
			return List.Any( x => x.GetType() == t );
		}
	}
}
