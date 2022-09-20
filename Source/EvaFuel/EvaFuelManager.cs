/*
	This file is part of EvaFuel /L Unleashed
		© 2022 LisiasT
		© 2017-2020 linuxgurugamer
		© 2016 AliceTheGorgon
		© 2014 Vendan

	EvaFuel /L Unleashed is double licensed, as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	EvaFuel /L Unleashed is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with EvaFuel /L Unleashed.
	If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with EvaFuel /L Unleashed.
	If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;

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

       public  static Dictionary<string, kerbalEVAFueldata> kerbalEVAlist;

        public List<string> ShipwreckNames = new List<String>();

        private void Start()
        {
            Log.info("Start");
            DontDestroyOnLoad(this);
        }

        public void Awake()
        {
            Log.dbg("Awake");
            GameEvents.onCrewOnEva.Add(this.onEvaStart);            
            
            GameEvents.onCrewBoardVessel.Add(this.onEvaEnd);

#if false
            FileOperations fileops = new FileOperations();
            if (kerbalEVAlist == null)
                kerbalEVAlist = FileOperations.Instance.loadKerbalEvaData();
#endif
            if (kerbalEVAlist == null)
                Log.error("Awake, kerbalEVAlist is null");

            // store rescue ship names
            List<string> locTags = new List<string>()
            {
                "#autoLOC_6100036",
                "#autoLOC_6100037",
                "#autoLOC_6100038",
                "#autoLOC_6100039",
                "#autoLOC_6100040",
                "#autoLOC_6100041",
                "#autoLOC_6100042",
                "#autoLOC_6100043",
                "#autoLOC_6100044",
            };
            Localizer.Init();
            foreach(string tag in locTags)
            {
                string localizedName;
                if (Localizer.TryGetStringByTag(tag, out localizedName))
                {
                    ShipwreckNames.Add(localizedName);
                }
                
            }
        }


        public bool ModEnabled { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ModEnabled; } }
        public bool ShowInfoMessage { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowInfoMessage; } }
        public bool ShowLowFuelWarning { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().ShowLowFuelWarning; } }
        public bool DisableLowFuelWarningLandSplash { get { return HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().DisableLowFuelWarningLandSplash; } }

        public string resourceName { get { return HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().EvaPropellantName; } }
        public string shipPropName { get { return HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName; } }

        public void onEvaStart(GameEvents.FromToAction<Part, Part> data)
        {
            if (ModEnabled)
            Log.dbg("onEvaStart, ModEnabled: {0}", EvaFuelDifficultySettings.Instance.ModEnabled);
            {
                double fuelInEVAPack = 0;
                double takenFuel = 0;
                double fuelRequest = 0;
                //if (!data.from.Resources.Contains(HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName))
                {
                    //Log.Info("data.to/from not null");
                    //Log.Info("Part does not contain the resource: " + HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName);
                    fuelInEVAPack = this.onEvaHandler(data);
                }


                double evaTankFuelMax = HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings> ().EvaTankFuelMax;
                // Check available volume for EVA fuel - it might be reduced by some external factors
                // Default value for EVA fuel is 5 units however EVAFuel may be configured for greater amount
                // Treat propellantResourceDefaultAmount as multiplier where 5 units correspond to 100%
                //
                if (data.to.vessel.evaController != null) {
                    double defaultEVAFuel = data.to.vessel.evaController.propellantResourceDefaultAmount;
                    Log.detail ("default EVA resource amount: {0}", defaultEVAFuel);
                    evaTankFuelMax *= defaultEVAFuel / 5;
                }

                takenFuel = data.from.RequestResource(HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName, 
					(evaTankFuelMax - fuelInEVAPack) / 
					HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor);
                fuelRequest = takenFuel * HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor;
                
                
                Log.detail("fuelInEVAPack: {0}   takenFuel: {1}    fuelRequest: {2}", fuelInEVAPack, takenFuel, fuelRequest);

                bool rescueShip = false;
                if ((fuelRequest + fuelInEVAPack) == 0)
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
                                    if (contracts[currentContract].Title.Contains(crewList[currentCrew].name.Split(null)[0]) 
                                         && (data.from.vessel.name.Contains(crewList[currentCrew].name.Split(null)[0]) || IsShipwreck(data.from.vessel.name)))
                                    {//Please do not rename your ship to have a rescue contract Kerbal name in it if the contract is active.
                                        Log.info("Is a rescue ship!");
                                        rescueShip = true;
                                    }
                                }
                            }
                        }
                    }
                }

                //Floats and doubles don't like exact numbers. :/ Need to test for similarity rather than equality.
				if (fuelRequest + fuelInEVAPack + 0.001 > evaTankFuelMax)
                {
					data.to.RequestResource(resourceName, evaTankFuelMax - (fuelRequest + fuelInEVAPack));
                    if (ShowInfoMessage)
                    {
                        ScreenMessages.PostScreenMessage("Filled EVA tank with " + Math.Round(takenFuel, 2).ToString() + " units of " + resourceName + ".", HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ScreenMessageLife, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                else if (rescueShip == true && (fuelRequest + fuelInEVAPack) == 0)
                {
					data.to.RequestResource(resourceName, evaTankFuelMax - 1);//give one unit of eva propellant
                    if (ShowLowFuelWarning && (!DisableLowFuelWarningLandSplash || !data.from.vessel.LandedOrSplashed))
                    {
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "evafuel1", "Rescue fuel!", "Warning! There was no fuel aboard ship, so only one single unit of " + resourceName + " was able to be scrounged up for the journey!", "OK", false, HighLogic.UISkin);
                    }
                }
                else
                {
					data.to.RequestResource(resourceName, evaTankFuelMax - (fuelRequest + fuelInEVAPack));
                    if (ShowLowFuelWarning && (!DisableLowFuelWarningLandSplash || !data.from.vessel.LandedOrSplashed))
                    {
                        PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "evafuel2", "Low EVA Fuel!", "Warning! Only " + Math.Round(takenFuel, 2).ToString() + " units of " + HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName + " were available for EVA! Meaning you only have " + Math.Round(fuelRequest, 2).ToString() + " units of " + resourceName + "!", "OK", false, HighLogic.UISkin);
                    }
                }
            }
        }

        private bool IsShipwreck(string name)
        {
            return ShipwreckNames.Any(s => name.Contains(s));
        }

        public void onEvaEnd(GameEvents.FromToAction<Part, Part> data)
        {
            if (ModEnabled)
            Log.dbg("onEvaEnd");
            {
                
                double fuelLeft = data.from.RequestResource(resourceName, HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().EvaTankFuelMax);

                double fuelStored = data.to.RequestResource(HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName, -fuelLeft / HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor);
                if (ShowInfoMessage)
                {
                    ScreenMessages.PostScreenMessage("Returned " + Math.Round(fuelLeft / HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().FuelConversionFactor, 2).ToString() + " units of " + HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName + " to ship.", HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ScreenMessageLife, ScreenMessageStyle.UPPER_CENTER);
                }
                data.from.RequestResource(resourceName,  fuelStored + fuelLeft);
                Log.detail("fuelLeft: {0}    fuelStored: {1}", fuelLeft, fuelStored);
                
                onBoardHandler(data, fuelLeft + fuelStored);
            }
        }



        private void OnDestroy()
        {
            Log.dbg("OnDestroy");
            GameEvents.onCrewOnEva.Remove(this.onEvaStart);
            
            GameEvents.onCrewBoardVessel.Remove(this.onEvaEnd);
        }


        private double onEvaHandler(GameEvents.FromToAction<Part, Part> data)
        {
            Log.dbg("onEvaHandler");
            Log.dbg("data.from.name: {0}    data.to.name: {1}", data.from.name, data.to.name);

            double fuelTaken = 0;
            if (data.to == null || data.from == null)
                return 0;
            Log.detail("onEvaHandler, from: {0}   to: {1}", data.from.partInfo.title, data.to.partInfo.title);
            Log.detail("onEvaHandler, from: {0}   to: {1}", data.from.partInfo.name, data.to.partInfo.name);
            char[] delimiterChars = { '(', ')' };
            string[] words = data.to.name.Split(delimiterChars);
            string foundName = words[1];
            Log.dbg("Detected name: {0}", foundName);

            PartResource kerbalResource = null;

            //
            // The following is in case different kerbals have different eva propellant 
            //
            KerbalEVA kEVA = data.to.FindModuleImplementing<KerbalEVA>();

            IEnumerable<PartResource> kerbalResourceList = data.to.Resources.Where(p => p.resourceName == EVAFuelSettings.Instance.ShipPropellantName);
            if (kerbalResourceList.Count() > 0)
                kerbalResource = kerbalResourceList.First();
            else
                Log.detail("Kerbal Resource not found: {0}", EVAFuelSettings.Instance.ShipPropellantName);

            if (kerbalEVAlist == null)
            {
                Log.error("kerbalEVAlist is null");
                return 0;
            }
            else
            {
                kerbalEVAFueldata ked;
                Log.detail("Searching list for Kerbal: {0}", foundName);
                if (!kerbalEVAlist.TryGetValue(foundName, out ked))
                {
                    ked = new kerbalEVAFueldata();
                    ked.name = foundName;
                    ked.evaPropAmt = 0; //  kerbalResource.maxAmount;  // New kerbals always get the maxAmount

                    kerbalEVAlist.Add(ked.name, ked);
                    Log.detail("Adding full amount to new kerbal on EVA (not found in kerbalEVAList: {0}", data.to.partInfo.title);
                }
                fuelTaken = ked.evaPropAmt;
                ked.evaPropAmt = 0;

                // fillFromPod controls whether the Kerbal has a fixed amount of fuel for the entire mission or not.
                // if false, then the kerbal is not able to fill from the pod
#if false
                if (HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().fillFromPod)
                {
                    double giveBack = kerbalResource.maxAmount - ked.evaPropAmt;                    

                    double sentBackAmount = data.from.RequestResource(this.resourceName, -1 * giveBack);
                    kerbalResource.amount = ked.evaPropAmt;

                    Log.Info(string.Format("Returned {0} {1} to {2}",
                        sentBackAmount,
                        this.resourceName,
                        data.from.partInfo.title));
                }
#endif
                //FileOperations.Instance.saveKerbalEvaData(kerbalEVAlist);
               
            }
            

          //  lastPart = data.to;

            Log.detail("[{0}] Caught OnCrewOnEva event to part ({1}) containing this resource ({2})",
                    this.GetType().Name,
                    data.to.partInfo.title,
                    EVAFuelSettings.Instance.EvaPropellantName);
            Log.detail("Kerbal had stored: {0}", fuelTaken);
            return fuelTaken;
        }

        private void onBoardHandler(GameEvents.FromToAction<Part, Part> data, double fuelLeft)
        {
            Log.detail("onBoardHandler, fuelLeft: {0}", fuelLeft);
            if (data.to == null || data.from == null)
                return;
            Log.detail("onBoardHandler, from: {0}   to: {1}", data.from.partInfo.title, data.to.partInfo.title);
            Log.detail("onBoardHandler, from: {0}   to: {2}", data.from.partInfo.name, data.to.partInfo.name);

            if (fuelLeft == 0)
                return;
            KerbalEVA kEVA = data.from.FindModuleImplementing<KerbalEVA>();
            // EVAFuelSettings.Instance.EvaPropellantName = kEVA.propellantEVAFuelSettings.Instance.EvaPropellantName;

            var fromResource = data.from.Resources.Where(p => p.info.name == resourceName).First();
            if (fromResource == null)
            {
                Log.warn("Resource not found: {0} in part: {1}", EVAFuelSettings.Instance.EvaPropellantName, data.from.partInfo.title);          
                return;
            }
            kerbalEVAFueldata ked;

            if (kerbalEVAlist.TryGetValue(data.from.partInfo.title, out ked))
            {
                ked.evaPropAmt = fuelLeft;
            }
            else
            {
                Log.detail("Adding new kerbal on EVA (not found in kerbalEVAList: {0}", data.to.partInfo.title);

                // This is needed here in case the mod is added to an existing game while a kerbal is
                // already on EVA
                ked = new kerbalEVAFueldata();

                ked.name = data.from.partInfo.title;
                ked.evaPropAmt = fuelLeft;
                kerbalEVAlist.Add(ked.name, ked);
            }
            Log.detail("Storing EVA fuel: {0}", ked.evaPropAmt);
            //FileOperations.Instance.saveKerbalEvaData(kerbalEVAlist);
#if false
            if (HighLogic.CurrentGame.Parameters.CustomParams<EvaFuelDifficultySettings>().fillFromPod)
            {
                double sentAmount = data.to.RequestResource(this.resourceName, -fromResource.amount);

                fromResource.amount += sentAmount;

                Log.Info(string.Format("Returned {0} {1} to {2}",
                    -sentAmount,
                    this.resourceName,
                    data.to.partInfo.title));
            }
#endif
        }
    }
}