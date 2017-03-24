using KIS;
using KSPDev.GUIUtils;
using System;
using UnityEngine;

[assembly: KSPAssemblyDependency("KIS", 1, 1)]
namespace EvaFuel
{
    public class ModuleKISItemEvaPropellantProxy : ModuleKISItemEvaPropellant
    {


		public bool ModEnabled { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ModEnabled; } }
		public bool KISIntegrationEnabled { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().KISIntegrationEnabled; } }
		public bool ShowInfoMessage { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowInfoMessage; } }
		public bool ShowLowFuelWarning { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowLowFuelWarning; } }


        public override void OnItemUse(KIS_Item item, KIS_Item.UseFrom useFrom)
        {
			if (ModEnabled && KISIntegrationEnabled)
            {
				//string HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName = this.settings.HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantNameellantName;
				//string HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().EvaPropellantName = HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().EvaPropellantName;
				//double HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor = HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor;

				if (useFrom != KIS_Item.UseFrom.KeyUp && item.inventory.invType == ModuleKISInventory.InventoryType.Pod) {
					double fuelLeft = 0;
					double fuelMax = 0;


					fuelLeft = ModuleKISItemEvaPropellant.GetCanisterFuelResource (item).amount;
					fuelMax = ModuleKISItemEvaPropellant.GetCanisterFuelResource (item).maxAmount;

					double takenFuel = item.inventory.part.RequestResource (HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName, (fuelMax - fuelLeft) / HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor);
					double fuelRequest = takenFuel * HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor;
					item.SetResource (HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().EvaPropellantName, fuelLeft + fuelRequest);

					if (fuelRequest + 0.001 < fuelMax - fuelLeft) {//0.001 for floating point rounding issues. Don't want to trigger insufficient fuel all the time.
						if (ShowLowFuelWarning) {
							PopupDialog.SpawnPopupDialog (new Vector2 (0.5f, 0.5f), new Vector2 (0.5f, 0.5f), "Low EVA Fuel!", "Warning! Only " + Math.Round (takenFuel, 2).ToString () + " units of " + HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName + " were available to refill the EVA Canister! Meaning it only has " + Math.Round (fuelLeft + fuelRequest, 2).ToString () + " units of " + HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().EvaPropellantName + "!", "OK", false, HighLogic.UISkin);
						}
					} else {
						if (ShowInfoMessage) {
							ScreenMessaging.ShowPriorityScreenMessage ("Fuel tank refueled with " + Math.Round (takenFuel, 2).ToString () + " units of " + HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName + ".");
						}
					}
					UISoundPlayer.instance.Play (refuelSndPath);
				} else {
					base.OnItemUse (item, useFrom);
				}
			} else {
				base.OnItemUse (item, useFrom);
			}
		}
	}
}
