using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EvaFuel {
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	public class EvaFuelManager : MonoBehaviour {

		double EVATankFuelMax = 5;
		int ScreenMessageLife = 5;
		int ScreenMessageWarningLife = 10;

		public void Awake() {
			GameEvents.onCrewOnEva.Add(this.onEvaStart);
			GameEvents.onCrewBoardVessel.Add(this.onEvaEnd);
		}

		public void onEvaStart(GameEvents.FromToAction<Part, Part> data) {
			double ShipMonoProp = data.from.RequestResource("MonoPropellant", EVATankFuelMax);

			//debug
			print("EvaFuel, part count: " + data.from.vessel.Parts.Count.ToString());

			if (ShipMonoProp == EVATankFuelMax) {
				data.to.RequestResource("EVA Propellant", EVATankFuelMax - ShipMonoProp);
				ScreenMessages.PostScreenMessage ("Filled EVA tank with " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant.", ScreenMessageLife, ScreenMessageStyle.UPPER_CENTER);
			}
			if (ShipMonoProp < EVATankFuelMax) {
				if (ShipMonoProp == 0) { //Check if it's likely a rescue contract ship.
					double ShipElectricity = data.from.RequestResource ("ElectricCharge", 1);
					if (ShipElectricity == 0) {
						if (data.from.vessel.Parts.Count == 1) { //only one part on ship
							data.to.RequestResource ("EVA Propellant", EVATankFuelMax - 1);//give one unit of eva propellant
							ScreenMessages.PostScreenMessage ("The Kerbal manages to scrounge together 1 unit of EVA Propellant.", ScreenMessageWarningLife, ScreenMessageStyle.UPPER_CENTER);
						}
					} else { //This has electricity, and thus isn't a rescue contract ship.
						data.from.RequestResource ("ElectricCharge", -1);
						data.to.RequestResource ("EVA Propellant", EVATankFuelMax - ShipMonoProp);
						ScreenMessages.PostScreenMessage ("Warning! No MonoPropellant for EVA!", ScreenMessageWarningLife, ScreenMessageStyle.UPPER_CENTER);
					}

				} else {
					data.to.RequestResource("EVA Propellant", EVATankFuelMax - ShipMonoProp);
					ScreenMessages.PostScreenMessage ("Warning! Only " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant for EVA!", ScreenMessageWarningLife, ScreenMessageStyle.UPPER_CENTER);
				}
			}
		}

		public void onEvaEnd(GameEvents.FromToAction<Part, Part> data) {
			double FuelLeft = data.from.RequestResource("EVA Propellant", EVATankFuelMax);
			data.to.RequestResource("MonoPropellant", -FuelLeft);
			ScreenMessages.PostScreenMessage ("Returned "  + Math.Round(FuelLeft, 2).ToString() + " units of MonoPropellant to ship.", ScreenMessageLife, ScreenMessageStyle.UPPER_CENTER);
		}
	}
}
