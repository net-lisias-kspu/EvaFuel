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
		public bool ModEnabled
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ModEnabled; } }
		public bool ShowInfoMessage
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowInfoMessage; } }
		public bool ShowLowFuelWarning
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowLowFuelWarning; } }
		public bool DisableLowFuelWarningLandSplash
		{ get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().DisableLowFuelWarningLandSplash; } }


        public void onEvaStart(GameEvents.FromToAction<Part, Part> data)
        {

			if (ModEnabled) {
	            string shipProp = this.settings.ShipPropellantName;
	            string evaProp = this.settings.EvaPropellantName;
	            double evaFuelMax = this.settings.EvaTankFuelMax;
				double conversionFactor = this.settings.FuelConversionFactor; //conversionFactor for changing ration of ship fuel to Eva Fuel. Take special care to check multiplication or division
	            int messageLife = this.settings.ScreenMessageLife;


				double takenFuel = data.from.RequestResource(shipProp, evaFuelMax / conversionFactor);
				double fuelRequest = takenFuel * conversionFactor;

				bool rescueShip = false;
				if (fuelRequest == 0) { //Only check for rescue vessel status if there's no EVA fuel in the current ship.
					var crewList = data.to.vessel.GetVesselCrew ();
					if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
						if (Contracts.ContractSystem.Instance != null) {
							var contracts = Contracts.ContractSystem.Instance.Contracts;
							for (int currentCrew = 0; currentCrew < crewList.Count; currentCrew++) {
								for (int currentContract = 0; currentContract < contracts.Count; currentContract++) {
									if (contracts [currentContract].Title.Contains (crewList [currentCrew].name) && data.from.vessel.name.Contains (crewList [currentCrew].name.Split (null) [0])) {//Please do not rename your ship to have a rescue contract Kerbal name in it if the contract is active.
										rescueShip = true;
									}
								}
							}
						}
					}
				}

				if (fuelRequest + 0.001 > evaFuelMax) { //Floats and doubles don't like exact numbers. :/ Need to test for similarity rather than equality.
					data.to.RequestResource (evaProp, evaFuelMax - fuelRequest);
					if (ShowInfoMessage) {
						ScreenMessages.PostScreenMessage ("Filled EVA tank with " + Math.Round (takenFuel, 2).ToString () + " units of " + shipProp + ".", messageLife, ScreenMessageStyle.UPPER_CENTER);
					}
				} else if (rescueShip == true && fuelRequest == 0) {
					data.to.RequestResource (evaProp, evaFuelMax - 1);//give one unit of eva propellant
					if (ShowLowFuelWarning && (!DisableLowFuelWarningLandSplash || !data.from.vessel.LandedOrSplashed)) {
						PopupDialog.SpawnPopupDialog (new Vector2 (0.5f, 0.5f), new Vector2 (0.5f, 0.5f), "Rescue fuel!", "Warning! There was no fuel aboard ship, so only one single unit of " + evaProp + " was able to be scrounged up for the journey!", "OK", false, HighLogic.UISkin);
					}
				} else {
					data.to.RequestResource (evaProp, evaFuelMax - fuelRequest);
					if (ShowLowFuelWarning && (!DisableLowFuelWarningLandSplash || !data.from.vessel.LandedOrSplashed)) {
						PopupDialog.SpawnPopupDialog (new Vector2 (0.5f, 0.5f), new Vector2 (0.5f, 0.5f), "Low EVA Fuel!", "Warning! Only " + Math.Round (takenFuel, 2).ToString () + " units of " + shipProp + " were available for EVA! Meaning you only have " + Math.Round (fuelRequest, 2).ToString () + " units of " + evaProp + "!", "OK", false, HighLogic.UISkin);
					}
				}
			}
		}

		public void onEvaEnd(GameEvents.FromToAction<Part, Part> data)
        {
			if (ModEnabled) {
				string shipProp = this.settings.ShipPropellantName;
				string evaProp = this.settings.EvaPropellantName;
				double evaFuelMax = this.settings.EvaTankFuelMax;
				double conversionFactor = this.settings.FuelConversionFactor;
				int messageLife = this.settings.ScreenMessageLife;

				double fuelLeft = data.from.RequestResource (evaProp, evaFuelMax);

				data.to.RequestResource (shipProp, -fuelLeft / conversionFactor);
				if (ShowInfoMessage) {
					ScreenMessages.PostScreenMessage ("Returned " + Math.Round (fuelLeft / conversionFactor, 2).ToString () + " units of " + shipProp + " to ship.", messageLife, ScreenMessageStyle.UPPER_CENTER);
				}
			}
		}
	}

	public class EvaFuelDifficultySettings : GameParameters.CustomParameterNode
	{
		public override string Title { get { return "Things"; } }
		public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
		public override string Section { get { return "EvaFuel"; } }
		public override int SectionOrder { get { return 1; } }
		public override bool HasPresets { get { return false; } }

		[GameParameters.CustomParameterUI("Enable mod for this save?")]
		public bool ModEnabled = true;

		//[GameParameters.CustomStringParameterUI("Only works if KIS is installed")]
		//public string KISInfo = "";
		[GameParameters.CustomParameterUI("Enable KIS integration?")]
		public bool KISIntegrationEnabled = true;

		[GameParameters.CustomParameterUI("Show fuel transfer message?")]
		public bool ShowInfoMessage = false;

		[GameParameters.CustomParameterUI("Show low fuel warning?")]
		public bool ShowLowFuelWarning = true;

		[GameParameters.CustomParameterUI("Disable warning when landed/splashed?")]
		public bool DisableLowFuelWarningLandSplash = true;


	}
}
