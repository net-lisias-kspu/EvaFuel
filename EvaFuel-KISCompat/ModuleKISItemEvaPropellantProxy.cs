using KIS;
using KSPDev.GUIUtils;
using System;

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

        public override void OnItemUse(KIS_Item item, KIS_Item.UseFrom useFrom)
        {
            string shipProp = this.settings.ShipPropellantName;
            string evaProp = this.settings.EvaPropellantName;

            if (useFrom != KIS_Item.UseFrom.KeyUp && item.inventory.invType == ModuleKISInventory.InventoryType.Pod)
            {
				double fuelLeft = 0;
				double fuelMax = 0;
				fuelLeft = GetCanisterFuelResource(item).amount;
				fuelMax = GetCanisterFuelResource(item).maxAmount;

				double fuelRequest = item.inventory.part.RequestResource(shipProp, fuelMax - fuelLeft);
				item.SetResource(evaProp, fuelLeft + fuelRequest);

				if (fuelRequest < fuelMax - fuelLeft)
                {
					ScreenMessaging.ShowPriorityScreenMessage("Warning! Only " + Math.Round(fuelRequest, 2).ToString() + " units of " + shipProp + " were left to fill the tank!");
				}
                else
                {
					ScreenMessaging.ShowPriorityScreenMessage("Fuel tank refueled with " + Math.Round(fuelRequest, 2).ToString() + " units of " + shipProp + ".");
				}
				UISoundPlayer.instance.Play(refuelSndPath);
			}
            else
            {
				base.OnItemUse(item, useFrom);
			}
		}
	}
}
