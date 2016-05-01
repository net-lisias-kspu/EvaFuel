using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KIS;
[assembly: KSPAssemblyDependency("KIS", 1, 1)]

namespace EvaFuel {
	public class ModuleKISItemEvaPropellantProxy : ModuleKISItemEvaPropellant {

		int ScreenMessageLife = 5;
		int ScreenMessageWarningLife = 10;

		public override void OnItemUse(KIS_Item item, KIS_Item.UseFrom useFrom) {
			print("KISCompat OnItemUse called!");
			if (useFrom != KIS_Item.UseFrom.KeyUp && item.inventory.invType == ModuleKISInventory.InventoryType.Pod) {
				//Logger.logInfo("Refuel {0} from pod's monopropellant tank", item.availablePart.name);
				// ... here goes your logic...

				double FuelLeft = 0;
				double FuelMax = 0;

				foreach (KIS_Item.ResourceInfo itemRessource in item.GetResources()) {
					if (itemRessource.resourceName == "EVA Propellant") {
						FuelLeft = itemRessource.amount;
						FuelMax = itemRessource.maxAmount;
						item.inventory.PlaySound (refuelSndPath, false, false);
					}
				}

				double ShipMonoProp = item.inventory.part.RequestResource ("MonoPropellant", FuelMax - FuelLeft);
				item.SetResource ("EVA Propellant", FuelLeft + ShipMonoProp);
				if (ShipMonoProp + 0.001 < FuelMax - FuelLeft) { //"+ 0.001" is to prevent triggering on rounding errors, just in case.
					ScreenMessages.PostScreenMessage("Warning! Only " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant were left to fill the tank!", ScreenMessageWarningLife, ScreenMessageStyle.UPPER_CENTER);
				} else {
					ScreenMessages.PostScreenMessage("Fuel tank refueled with " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant.", ScreenMessageLife, ScreenMessageStyle.UPPER_CENTER);
				}
				item.inventory.PlaySound (refuelSndPath, false, false);
			} else {
				base.OnItemUse(item, useFrom);
			}
		}
	}
}
