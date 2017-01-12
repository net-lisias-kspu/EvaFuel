using System;
using UnityEngine;

namespace EvaFuel
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	public class EvaFuelManager : MonoBehaviour
    {
        public EvaFuelSettings settings { get; set; }

        public void Awake()
        {
            this.settings = new EvaFuelSettings();
            this.LoadSettings();
            GameEvents.onCrewOnEva.Add(this.onEvaStart);
			GameEvents.onCrewBoardVessel.Add(this.onEvaEnd);
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

        public void onEvaStart(GameEvents.FromToAction<Part, Part> data)
        {
            string shipProp = this.settings.ShipPropellantName;
            string shipElec = this.settings.ShipElectricityName;
            string evaProp = this.settings.EvaPropellantName;
            double evaFuelMax = this.settings.EvaTankFuelMax;
			double conversionFactor = this.settings.FuelConversionFactor; //conversionFactor for changing ration of ship fuel to Eva Fuel. Take special care to check multiplication or division
            int messageLife = this.settings.ScreenMessageLife;
			int warningLife = this.settings.ScreenMessageWarningLife;


			double takenFuel = data.from.RequestResource(shipProp, evaFuelMax / conversionFactor);
			double fuelRequest = takenFuel * conversionFactor;

			if (Math.Abs(fuelRequest - evaFuelMax) < 0.001) //Floats and doubles don't like exaqct numbers. :/ Need to test for similarity rather than equality.
            {
				data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
				ScreenMessages.PostScreenMessage("Filled EVA tank with " + Math.Round(takenFuel, 2).ToString() + " units of " + shipProp + ".", messageLife, ScreenMessageStyle.UPPER_CENTER);
			} else
			{
				if (fuelRequest == 0) //Check if it's likely a rescue contract ship.
                {
					double shipElectricity = data.from.RequestResource(shipElec, 1);
					if (shipElectricity == 0)
                    {
						if (data.from.vessel.Parts.Count == 1) //only one part on ship
                        {
							data.to.RequestResource(evaProp, evaFuelMax - 1);//give one unit of eva propellant
							ScreenMessages.PostScreenMessage("The Kerbal manages to scrounge together 1 unit of " + evaProp + ".", warningLife, ScreenMessageStyle.UPPER_CENTER);
						}
					} else //This has electricity, and thus isn't a rescue contract ship.
                    {
						data.from.RequestResource(shipElec, -1);
						data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
						ScreenMessages.PostScreenMessage("Warning! No " + shipProp + " available for EVA!", warningLife, ScreenMessageStyle.UPPER_CENTER);
					}
				} else
                {
					data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
					ScreenMessages.PostScreenMessage("Warning! Only " + Math.Round(takenFuel, 2).ToString() + " units of " + shipProp + " available for EVA!", warningLife, ScreenMessageStyle.UPPER_CENTER);
				}
			}
		}

		public void onEvaEnd(GameEvents.FromToAction<Part, Part> data)
        {
            string shipProp = this.settings.ShipPropellantName;
            string evaProp = this.settings.EvaPropellantName;
            double evaFuelMax = this.settings.EvaTankFuelMax;
			double conversionFactor = this.settings.FuelConversionFactor;
            int messageLife = this.settings.ScreenMessageLife;

            double fuelLeft = data.from.RequestResource(evaProp, evaFuelMax);

			data.to.RequestResource(shipProp, -fuelLeft / conversionFactor);
			ScreenMessages.PostScreenMessage("Returned "  + Math.Round(fuelLeft / conversionFactor, 2).ToString() + " units of " + shipProp + " to ship.", messageLife, ScreenMessageStyle.UPPER_CENTER);
		}
	}
}
