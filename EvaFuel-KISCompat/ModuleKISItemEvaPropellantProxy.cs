using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KIS;
[assembly: KSPAssemblyDependency("KIS", 1, 1)]

namespace EvaFuel
{
	public class ModuleKISItemEvaPropellantProxy : ModuleKISItemEvaPropellant
	{
		public override void OnItemUse(KIS_Item item, KIS_Item.UseFrom useFrom)
		{
			print("KISCompat OnItemUse called!");
			if (useFrom != KIS_Item.UseFrom.KeyUp
			    && item.inventory.invType == ModuleKISInventory.InventoryType.Pod)
			{
				//Logger.logInfo("Refuel {0} from pod's monopropellant tank", item.availablePart.name);
				// ... here goes your logic...
				item.
				List EVAPropellantLeft = new List item.GetResources();
				print ("KISCompat, EVAPropellantLeft: " + EVAPropellantLeft.ToString());
				double ShipMonoProp = item.inventory.part.RequestResource ("MonoPropellant", 10);
				//print("KISCompat, ShipMonoProp: " + ShipMonoProp.ToString());
				item.SetResource ("EVA Propellant", ShipMonoProp);
			}
			else
			{
				base.OnItemUse(item, useFrom);
			}
		}
	}
}
