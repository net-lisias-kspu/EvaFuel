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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Asset = KSPe.IO.Asset<EvaFuel.Startup>;

namespace EvaFuel
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class EVAFuelGlobals : MonoBehaviour
    {
        public static bool changeEVAPropellent;
        void Start()
        {
            changeEVAPropellent = false;
        }
    }

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    class SelectEVAFuelType : MonoBehaviour
    {
        private static SelectEVAFuelType _instance;
        public static SelectEVAFuelType Instance => _instance;

        public float lastTimeTic = 0;

        private Rect settingsRect;

        int curResIndex = -1;
        Vector2 scrollPosition1;
        static List<PartResourceDefinition> allResources = null;
        static List<string> allResourcesDisplayNames = null;
        static List<string> fuelResources = null;
        static List<string> bannedResources = null;

        public string selectedFuel;
        GUIStyle smallButtonStyle, smallScrollBar;
        static string EVA_FUELRESOURCES = "FUELRESOURCES";
        static string BANNED_RESOURCES = "BANNED";

        bool allRes = false;
        bool fuelRes = true;

        public SelectEVAFuelType() { }

        void Start()
        {
            _instance = this;
            smallButtonStyle = new GUIStyle(HighLogic.Skin.button);
            smallButtonStyle.stretchHeight = false;
            smallButtonStyle.fixedHeight = 20f;

            smallScrollBar = new GUIStyle(HighLogic.Skin.verticalScrollbar);
            smallScrollBar.fixedWidth = 8f;

            settingsRect = new Rect(200, 200, 275, 400);
            scrollPosition1 = Vector2.zero;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        void OnGUI()
        {
            if (allResources == null)
                getAllResources();

            // The settings are only available in the space center
            GUI.skin = HighLogic.Skin;
            settingsRect = GUILayout.Window("EVAFuelSettings".GetHashCode(),
                                            settingsRect,
                                            SettingsWindowFcn,
                                            "EVA Fuel Settings",
                                            GUILayout.ExpandWidth(true),
                                            GUILayout.ExpandHeight(true));
        }


        private readonly Asset.ConfigNode FUEL_RESOURCES = Asset.ConfigNode.For("EvaFuel", "fuelResources.cfg");
        public List<String> getFuelResources(bool banned = false)
        {
            List<string> fr = new List<String>();
            if (!FUEL_RESOURCES.IsLoadable)
            {
                Log.error("File not found: {0}", FUEL_RESOURCES.Path);
                return fr;
            }

            ConfigNode configFile = FUEL_RESOURCES.Load().Node;

            if (configFile != null)
            {
                ConfigNode configFileNode = configFile.GetNode(this.GetType().Namespace);

                if (configFileNode != null)
                {
                    ConfigNode configDataNode = banned ? configFileNode.GetNode(BANNED_RESOURCES) : configFileNode.GetNode(EVA_FUELRESOURCES);
                    if (configDataNode != null)
                        fr = configDataNode.GetValuesList("resource");
                }
                else
                    Log.error("NODENAME not found: {0}", this.GetType().Namespace);
            }

            return fr;
        }


        void fillResourceDisplayNames()
        {
            if (fuelResources == null || allResources == null)
                getAllResources();
            allResourcesDisplayNames = new List<string>();
            int cnt = 0;
            if (fuelRes && fuelResources.Count > 0)
            {
                foreach (var s in fuelResources)
                {
                    try
                    {
                        var ar = allResources.Find(o => o.name == s);
                        if (ar.displayName != null)
                            allResourcesDisplayNames.Add(ar.displayName);
                        else
                            allResourcesDisplayNames.Add( ar.name);
                        if (ar.name == EVAFuelSettings.Instance.ShipPropellantName)
                            curResIndex = cnt;
                        cnt++;

                    } catch
                    {
                        Log.error("Can't find resource: {0} in allResources", s);
                    }
                }
            }
            else
            {
                foreach (var ar in allResources)
                {
                    if (bannedResources.Contains(ar.name))
                        continue;
                    if (ar.displayName != null)
                        allResourcesDisplayNames.Add(ar.displayName);
                    else
                        allResourcesDisplayNames.Add(ar.name);
                    if (ar.name == EVAFuelSettings.Instance.ShipPropellantName)
                        curResIndex = cnt;
                    cnt++;
                }

            }
        }

        void getAllResources()
        {
            allResources = new List<PartResourceDefinition>();

            foreach (PartResourceDefinition rs in PartResourceLibrary.Instance.resourceDefinitions)
            {
                allResources.Add(rs);
            }
            allResources = allResources.OrderBy(o => o.displayName).ToList();
            fuelResources = getFuelResources();
            bannedResources = getFuelResources(true);
            
            fillResourceDisplayNames();
            
        }

        void SettingsWindowFcn(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Select EVA Propellent from list below");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            var newallRes = GUILayout.Toggle(allRes, "All resources");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            var newfuelRes = GUILayout.Toggle(fuelRes, "Fuel resources");
            GUILayout.EndVertical();
            if (newfuelRes && allRes)
            {
                allRes = false;
                fuelRes = true;
                fillResourceDisplayNames();
            }
            else
                if (newallRes & fuelRes)
            {
                allRes = true;
                fuelRes = false;
                fillResourceDisplayNames();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1);

            curResIndex = GUILayout.SelectionGrid(curResIndex, allResourcesDisplayNames.ToArray(), 1, smallButtonStyle);

            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK")) this.Commit();
            {
                answer = Answer.answered;
                if (allRes)
                {
                    selectedFuel = allResources[curResIndex].name;
                }
                else
                {
                    int cnt = 0;
                    for (int i = 0; i < fuelResources.Count; i++)                    
                    {
                        try
                        {
                            var ar = allResources.Find(o => fuelResources[i] == o.name);

                            if (cnt == curResIndex)
                            {
                                selectedFuel = ar.name;
                                break;
                            }
                            cnt++;
                        }
                        catch
                        { }
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            // This call allows the user to drag the window around the screen
            GUI.DragWindow();
        }
    }



    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings
    // HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>()


    public class EvaFuelDifficultySettings
    {
        private static EvaFuelDifficultySettings _instance = null;
        public static EvaFuelDifficultySettings Instance => _instance??(_instance = new EvaFuelDifficultySettings());

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

#if false
        [GameParameters.CustomParameterUI("Fill from Pod", toolTip = "(if false, unable to refuel for entire mission")]
        public bool fillFromPod = true;
#endif
    }

    public class EVAFuelSettings
    {
        private static EVAFuelSettings _instance = null;
        public static EVAFuelSettings Instance => _instance??(_instance = new EVAFuelSettings());

        [GameParameters.CustomFloatParameterUI("EVA Fuel Tank Max", minValue = 0.5f, maxValue = 15.0f, asPercentage = false, displayFormat = "0.0",
           toolTip = "Maximum amount of EVA fuel")]
        public double EvaTankFuelMax = 5.0f;

        [GameParameters.CustomFloatParameterUI("EVA Fuel Conversion Factor", minValue = 0.1f, maxValue = 10.0f, asPercentage = false, displayFormat = "0.0",
          toolTip = "Each 1 unit of ship fuel will become this many units of Eva Fuel")]
        public double FuelConversionFactor = 1.0f;

        // [GameParameters.CustomStringParameterUI("EVA Propellent Type", autoPersistance = true, lines = 1, title = "EVA Propellent Type")]       
        public string EvaPropellantName = "EVA Propellant";

        [GameParameters.CustomParameterUI("Change EVA Propellent Type")]
        public bool changeEVAPropellent = false;

        [GameParameters.CustomStringParameterUI("EVA Propellent Type", autoPersistance = true, lines = 1, title = "EVA Propellent Type")]
        public string ShipPropellantName = "MonoPropellant";


        [GameParameters.CustomParameterUI("Add resource to command pods", toolTip ="Command pod is defined as parts with crew & MonoProp)")]
        public bool addToCmdPods = true;

        [GameParameters.CustomFloatParameterUI("Amount of resource to add:", minValue = 0.1f, maxValue = 10.0f, asPercentage = false, displayFormat = "0.0")]
        public double resourcesAmtToAdd = 5.0f;

        [GameParameters.CustomParameterUI("Multiply resource added by max crew")]
        public bool resourcePerCrew = true;


        [GameParameters.CustomStringParameterUI("Ship Electricity Name", autoPersistance = true, lines = 2, title = "Ship Electricity Name")]
        public string ShipElectricityName = "ElectricCharge";

        [GameParameters.CustomIntParameterUI("Screen Message Life", maxValue = 10)]
        public int ScreenMessageLife = 5;

        // Currently not used
        //[GameParameters.CustomIntParameterUI("Screen Message Warning Life", maxValue = 10)]
        //public int ScreenMessageWarningLife = 10;
    }

}

