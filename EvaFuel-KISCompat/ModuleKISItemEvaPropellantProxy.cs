using KIS;
using KSPDev.GUIUtils;
using System;
using UnityEngine;

[assembly: KSPAssemblyDependency("KIS", 1, 1)]
namespace EvaFuel
{
    public class ModuleKISItemEvaPropellantProxy : ModuleKISItemEvaPropellant
    {
        public EvaFuelSettings settings { get; set; }

        public override void OnAwake()
        {
            this.settings = new EvaFuelSettings();
            this.LoadSettings();
            base.OnAwake();
        }


        public void LoadSettings()
        {
            ConfigNode[] nodes;
            nodes = GameDatabase.Instance.GetConfigNodes("EvaFuelSettings");
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    this.settings.Load(nodes[i]);
                }
            }
        }
		public bool ModEnabled
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ModEnabled; } }
		public bool KISIntegrationEnabled
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().KISIntegrationEnabled; } }
		public bool ShowInfoMessage
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowInfoMessage; } }
		public bool ShowLowFuelWarning
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowLowFuelWarning; } }


        public override void OnItemUse(KIS_Item item, KIS_Item.UseFrom useFrom)
        {
			if (ModEnabled && KISIntegrationEnabled) {
				string shipProp = this.settings.ShipPropellantName;
				string evaProp = this.settings.EvaPropellantName;
				double conversionFactor = this.settings.FuelConversionFactor;

				if (useFrom != KIS_Item.UseFrom.KeyUp && item.inventory.invType == ModuleKISInventory.InventoryType.Pod) {
					double fuelLeft = 0;
					double fuelMax = 0;
					fuelLeft = GetCanisterFuelResource (item).amount;
					fuelMax = GetCanisterFuelResource (item).maxAmount;

					double takenFuel = item.inventory.part.RequestResource (shipProp, (fuelMax - fuelLeft) / conversionFactor);
					double fuelRequest = takenFuel * conversionFactor;
					item.SetResource (evaProp, fuelLeft + fuelRequest);

					if (fuelRequest + 0.001 < fuelMax - fuelLeft) {//0.001 for floating point rounding issues. Don't want to trigger insufficient fuel all the time.
						if (ShowLowFuelWarning) {
							PopupDialog.SpawnPopupDialog (new Vector2 (0.5f, 0.5f), new Vector2 (0.5f, 0.5f), "Low EVA Fuel!", "Warning! Only " + Math.Round (takenFuel, 2).ToString () + " units of " + shipProp + " were available to refill the EVA Canister! Meaning it only has " + Math.Round (fuelLeft + fuelRequest, 2).ToString () + " units of " + evaProp + "!", "OK", false, HighLogic.UISkin);
						}
					} else {
						if (ShowInfoMessage) {
							ScreenMessaging.ShowPriorityScreenMessage ("Fuel tank refueled with " + Math.Round (takenFuel, 2).ToString () + " units of " + shipProp + ".");
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
