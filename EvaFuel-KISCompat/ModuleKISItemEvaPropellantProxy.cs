using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using KSPDev.GUIUtils;
using KIS;
[assembly: KSPAssemblyDependency("KIS", 1, 1)]

namespace EvaFuel {
	public class ModuleKISItemEvaPropellantProxy : ModuleKISItemEvaPropellant {

		public override void OnItemUse(KIS_Item item, KIS_Item.UseFrom useFrom) {
			if (useFrom != KIS_Item.UseFrom.KeyUp && item.inventory.invType == ModuleKISInventory.InventoryType.Pod) {
				double FuelLeft = 0;
				double FuelMax = 0;
				FuelLeft = GetCanisterFuelResource (item).amount;
				FuelMax = GetCanisterFuelResource (item).maxAmount;

				double ShipMonoProp = item.inventory.part.RequestResource ("MonoPropellant", FuelMax - FuelLeft);
				item.SetResource ("EVA Propellant", FuelLeft + ShipMonoProp);

				if (ShipMonoProp < FuelMax - FuelLeft) {
					ScreenMessaging.ShowPriorityScreenMessage("Warning! Only " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant were left to fill the tank!");
				} else {
					ScreenMessaging.ShowPriorityScreenMessage("Fuel tank refueled with " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant.");
				}
				UISoundPlayer.instance.Play(refuelSndPath);
			} else {
				base.OnItemUse(item, useFrom);
			}
		}
	}
}
