﻿using System;
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
            int messageLife = this.settings.ScreenMessageLife;
            int waringLife = this.settings.ScreenMessageWarningLife;

            double fuelRequest = data.from.RequestResource(shipProp, evaFuelMax);

			if (fuelRequest == evaFuelMax)
            {
				data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
				ScreenMessages.PostScreenMessage("Filled EVA tank with " + Math.Round(fuelRequest, 2).ToString() + " units of " + shipProp + ".", messageLife, ScreenMessageStyle.UPPER_CENTER);
			}
			if (fuelRequest < evaFuelMax)
            {
				if (fuelRequest == 0) //Check if it's likely a rescue contract ship.
                {
					double shipElectricity = data.from.RequestResource(shipElec, 1);
					if (shipElectricity == 0)
                    {
						if (data.from.vessel.Parts.Count == 1) //only one part on ship
                        {
							data.to.RequestResource(evaProp, evaFuelMax - 1);//give one unit of eva propellant
							ScreenMessages.PostScreenMessage("The Kerbal manages to scrounge together 1 unit of " + evaProp + ".", waringLife, ScreenMessageStyle.UPPER_CENTER);
						}
					}
                    else //This has electricity, and thus isn't a rescue contract ship.
                    {
						data.from.RequestResource(shipElec, -1);
						data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
						ScreenMessages.PostScreenMessage("Warning! No " + shipProp + " available for EVA!", waringLife, ScreenMessageStyle.UPPER_CENTER);
					}
				}
                else
                {
					data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
					ScreenMessages.PostScreenMessage("Warning! Only " + Math.Round(fuelRequest, 2).ToString() + " units of " + shipProp + " available for EVA!", waringLife, ScreenMessageStyle.UPPER_CENTER);
				}
			}
		}

		public void onEvaEnd(GameEvents.FromToAction<Part, Part> data)
        {
            string shipProp = this.settings.ShipPropellantName;
            string evaProp = this.settings.EvaPropellantName;
            double evaFuelMax = this.settings.EvaTankFuelMax;
            int messageLife = this.settings.ScreenMessageLife;

            double fuelLeft = data.from.RequestResource(evaProp, evaFuelMax);

			data.to.RequestResource(shipProp, -fuelLeft);
			ScreenMessages.PostScreenMessage("Returned "  + Math.Round(fuelLeft, 2).ToString() + " units of " + shipProp + " to ship.", messageLife, ScreenMessageStyle.UPPER_CENTER);
		}
	}
}
