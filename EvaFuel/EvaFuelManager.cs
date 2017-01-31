using System;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.UI.Screens.Flight;


namespace EvaFuel
{
    public class kerbalEVAFueldata
    {
        public string name;
        public double evaPropAmt;
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class EvaFuelManager : MonoBehaviour
    {

        public EvaFuelSettings settings { get; set; }

        static Dictionary<string, kerbalEVAFueldata> kerbalEVAlist;

        public void Awake()
        {
            this.settings = new EvaFuelSettings();
            this.LoadSettings();
            GameEvents.onCrewOnEva.Add(this.onEvaStart);
            GameEvents.onCrewBoardVessel.Add(this.onEvaEnd);

            GameEvents.onCrewOnEva.Add(this.onEvaHandler);
            GameEvents.onCrewBoardVessel.Add(this.onBoardHandler);
            GameEvents.onVesselSwitching.Add(this.onVesselSwitching);
            FileOperations fileops = new FileOperations();
            if (kerbalEVAlist == null)
                kerbalEVAlist = FileOperations.Instance.loadKerbalEvaData();
            if (kerbalEVAlist == null)
                Log.Error("Awake, kerbalEVAlist is null");
            Log.Info("Awake");

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

            if (ModEnabled)
            {
                string shipProp = this.settings.ShipPropellantName;
                string evaProp = this.settings.EvaPropellantName;
                double evaFuelMax = this.settings.EvaTankFuelMax;
                double conversionFactor = this.settings.FuelConversionFactor; //conversionFactor for changing ration of ship fuel to Eva Fuel. Take special care to check multiplication or division
                int messageLife = this.settings.ScreenMessageLife;


                double takenFuel = data.from.RequestResource(shipProp, evaFuelMax / conversionFactor);
                double fuelRequest = takenFuel * conversionFactor;

                bool rescueShip = false;
                if (fuelRequest == 0)
                { //Only check for rescue vessel status if there's no EVA fuel in the current ship.
                    var crewList = data.to.vessel.GetVesselCrew();
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        if (Contracts.ContractSystem.Instance != null)
                        {
                            var contracts = Contracts.ContractSystem.Instance.Contracts;
                            for (int currentCrew = 0; currentCrew < crewList.Count; currentCrew++)
                            {
                                for (int currentContract = 0; currentContract < contracts.Count; currentContract++)
                                {
                                    if (contracts[currentContract].Title.Contains(crewList[currentCrew].name) && data.from.vessel.name.Contains(crewList[currentCrew].name.Split(null)[0]))
                                    {//Please do not rename your ship to have a rescue contract Kerbal name in it if the contract is active.
                                        rescueShip = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (fuelRequest + 0.001 > evaFuelMax)
                { //Floats and doubles don't like exact numbers. :/ Need to test for similarity rather than equality.
                    data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
                    if (ShowInfoMessage)
                    {
                        ScreenMessages.PostScreenMessage("Filled EVA tank with " + Math.Round(takenFuel, 2).ToString() + " units of " + shipProp + ".", messageLife, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                else if (rescueShip == true && fuelRequest == 0)
                {
                    data.to.RequestResource(evaProp, evaFuelMax - 1);//give one unit of eva propellant
                    if (ShowLowFuelWarning && (!DisableLowFuelWarningLandSplash || !data.from.vessel.LandedOrSplashed))
                    {
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Rescue fuel!", "Warning! There was no fuel aboard ship, so only one single unit of " + evaProp + " was able to be scrounged up for the journey!", "OK", false, HighLogic.UISkin);
                    }
                }
                else
                {
                    data.to.RequestResource(evaProp, evaFuelMax - fuelRequest);
                    if (ShowLowFuelWarning && (!DisableLowFuelWarningLandSplash || !data.from.vessel.LandedOrSplashed))
                    {
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Low EVA Fuel!", "Warning! Only " + Math.Round(takenFuel, 2).ToString() + " units of " + shipProp + " were available for EVA! Meaning you only have " + Math.Round(fuelRequest, 2).ToString() + " units of " + evaProp + "!", "OK", false, HighLogic.UISkin);
                    }
                }
            }
        }

        public void onEvaEnd(GameEvents.FromToAction<Part, Part> data)
        {
            if (ModEnabled)
            {
                string shipProp = this.settings.ShipPropellantName;
                string evaProp = this.settings.EvaPropellantName;
                double evaFuelMax = this.settings.EvaTankFuelMax;
                double conversionFactor = this.settings.FuelConversionFactor;
                int messageLife = this.settings.ScreenMessageLife;

                double fuelLeft = data.from.RequestResource(evaProp, evaFuelMax);

                data.to.RequestResource(shipProp, -fuelLeft / conversionFactor);
                if (ShowInfoMessage)
                {
                    ScreenMessages.PostScreenMessage("Returned " + Math.Round(fuelLeft / conversionFactor, 2).ToString() + " units of " + shipProp + " to ship.", messageLife, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }



        private void OnDestroy()
        {
            GameEvents.onCrewOnEva.Remove(this.onEvaHandler);
            GameEvents.onCrewBoardVessel.Remove(this.onBoardHandler);
        }

        string resourceName = "EVA Propellant";

        Part lastPart = null;

        /// <summary>
        /// Gets the correct amount of resources from the origination vessel.  
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void onVesselSwitching(Vessel from, Vessel to)
        {
            if (to == null || from == null)
                return;

            Log.Info("onVesselSwitching: from: " + from.Parts.First().partInfo.title + "   to: " + to.Parts.First().partInfo.title);
            if (lastPart == to.Parts.First())
            {
                Log.Info("lastPart == data.to");

                KerbalEVA kEVA = to.Parts.First().FindModuleImplementing<KerbalEVA>();
                resourceName = kEVA.propellantResourceName;

                kerbalEVAFueldata ked;

                var kerbalResource = to.Parts.First().Resources.Where(p => p.info.name == resourceName).First();
                var shipResource = from.Parts.First().Resources.Where(p => p.info.name == resourceName).First();
                if (shipResource != null)
                {
                    Log.Info("shipResource found");
                }
                else
                    Log.Info("shipResource not found");

                if (kerbalEVAlist == null)
                    Log.Error("kerbalEVAlist is null");

                if (!kerbalEVAlist.TryGetValue(to.Parts.First().partInfo.title, out ked))
                {
                    ked = new kerbalEVAFueldata();
                    ked.name = to.Parts.First().partInfo.title;
                    ked.evaPropAmt = kerbalResource.maxAmount;  // New kerbals always get the maxAmount

                    kerbalEVAlist.Add(ked.name, ked);
                }

                // fillFromPod controls whether the Kerbal has a fixed amount of fuel for the entire mission or not.
                // if false, then the kerbal is not able to fill from the pod

                if (!HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().fillFromPod)
                {
                    double giveBack = kerbalResource.maxAmount - ked.evaPropAmt;

                    double sentBackAmount = from.Parts.First().RequestResource(this.resourceName, -1 * giveBack);
                    kerbalResource.amount = ked.evaPropAmt;

                    Log.Info(string.Format("Returned {0} {1} to {2}",
                        sentBackAmount,
                        this.resourceName,
                        from.Parts.First().partInfo.title));
                }
                FileOperations.Instance.saveKerbalEvaData(kerbalEVAlist);
            }
        }



        private void onEvaHandler(GameEvents.FromToAction<Part, Part> data)
        {
            Log.Info("onEvaHandler");
            if (data.to == null || data.from == null)
                return;


            var resource = data.from.Resources.Where(p => p.info.name == resourceName).First();
            if (resource == null)
            {
                Log.Info("Resource not found: " + resourceName + " in part: " + data.from.partInfo.title);
                return;
            }
            lastPart = data.to;

            Log.Info(
                string.Format("[{0}] Caught OnCrewOnEva event to part ({1}) containing this resource ({2})",
                    this.GetType().Name,
                    data.to.partInfo.title,
                    this.resourceName));
        }


        private void onBoardHandler(GameEvents.FromToAction<Part, Part> data)
        {
            if (data.to == null || data.from == null)
                return;

            Log.Info("onBoardHandler");
            KerbalEVA kEVA = data.from.FindModuleImplementing<KerbalEVA>();
            resourceName = kEVA.propellantResourceName;

            var fromResource = data.from.Resources.Where(p => p.info.name == resourceName).First();
            if (fromResource == null)
            {
                Log.Info("Resource not found: " + resourceName + " in part: " + data.from.partInfo.title);
                return;
            }
            kerbalEVAFueldata ked;

            if (kerbalEVAlist.TryGetValue(data.from.partInfo.title, out ked))
            {
                Log.Info("kerbal: " + data.from.partInfo.title + " found in list");
                ked.evaPropAmt = fromResource.amount;
            }
            else
            {
                // This is needed here in case the mod is added to an existing game while a kerbal is
                // already on EVA
                ked = new kerbalEVAFueldata();

                ked.name = data.from.partInfo.title;
                ked.evaPropAmt = fromResource.amount;

                kerbalEVAlist.Add(ked.name, ked);
            }
            FileOperations.Instance.saveKerbalEvaData(kerbalEVAlist);

            if (HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().fillFromPod)
            {
                double sentAmount = data.to.RequestResource(this.resourceName, -fromResource.amount);

                fromResource.amount += sentAmount;

                Log.Info(string.Format("Returned {0} {1} to {2}",
                    -sentAmount,
                    this.resourceName,
                    data.to.partInfo.title));
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

        [GameParameters.CustomParameterUI("Fill from Pod (if false, unable to refuel for entire mission")]
        public bool fillFromPod = true;


    }
    
}