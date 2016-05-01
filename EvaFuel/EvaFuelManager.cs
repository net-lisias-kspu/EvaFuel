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
			data.to.RequestResource("EVA Propellant", EVATankFuelMax - ShipMonoProp);
			if (ShipMonoProp == EVATankFuelMax) {
				ScreenMessages.PostScreenMessage ("Filled EVA tank with " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant.", ScreenMessageLife, ScreenMessageStyle.UPPER_CENTER);
			}
			if (ShipMonoProp < EVATankFuelMax) {
				ScreenMessages.PostScreenMessage ("Warning! Only " + Math.Round(ShipMonoProp, 2).ToString() + " units of MonoPropellant for EVA!", ScreenMessageWarningLife, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		public void onEvaEnd(GameEvents.FromToAction<Part, Part> data) {
			double FuelLeft = data.from.RequestResource("EVA Propellant", EVATankFuelMax);
			data.to.RequestResource("MonoPropellant", -FuelLeft);
			ScreenMessages.PostScreenMessage ("Returned "  + Math.Round(FuelLeft, 2).ToString() + " units of MonoPropellant to ship.", ScreenMessageLife, ScreenMessageStyle.UPPER_CENTER);
		}
	}
}
