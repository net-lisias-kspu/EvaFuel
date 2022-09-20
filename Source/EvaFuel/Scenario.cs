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
using KSP.IO;
using KSP;
using System.IO;

using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace EvaFuel
{
    // This class handels load- and save-operations.
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    class EvaFuelScenarioModule : ScenarioModule
    {
        public readonly static string MOD = Assembly.GetAssembly(typeof(EvaFuelManager)).GetName().Name;
        public static String TT_NODENAME = MOD;

        public override void OnSave(ConfigNode node)
        {
            try
            {
              
                ConfigNode configFileNode = new ConfigNode(TT_NODENAME);
                ConfigNode configDataNode;

                if (EvaFuelManager.kerbalEVAlist == null)
                    return;

                foreach (var ked in EvaFuelManager.kerbalEVAlist)
                {
                    configDataNode = new ConfigNode("EvaData");
                    configDataNode.SetValue("name", ked.Value.name, true);
                    configDataNode.SetValue("evaPropAmt", ked.Value.evaPropAmt, true);

                    configFileNode.AddNode("EvaData", configDataNode);

                }              


                node.AddNode(configFileNode);
            }
            catch (Exception e)
            {
                Debug.LogError("[KRnD] OnSave(): " + e.ToString());
            }
        }

        public override void OnLoad(ConfigNode configFile)
        {
            Log.Info("OnLoad");
            ConfigNode configFileNode = new ConfigNode();
            try
            {
                ConfigNode[] configDataNodes;

                Dictionary<string, kerbalEVAFueldata> kerbalEVAList = new Dictionary<string, kerbalEVAFueldata>();                
               
                configFileNode = configFile.GetNode(TT_NODENAME);
                if (configFileNode != null)
                {

                    configDataNodes = configFileNode.GetNodes("EvaData");
                    foreach (var dataNode in configDataNodes)
                    {
                        kerbalEVAFueldata ked = new kerbalEVAFueldata();

                        dataNode.TryGetValue("name", ref ked.name);
                        ked.evaPropAmt = Double.Parse(dataNode.GetValue("evaPropAmt"));
                        kerbalEVAList.Add(ked.name, ked);

                    }
                }
            

               
                EvaFuelManager.kerbalEVAlist = kerbalEVAList;
                return;
            }
            catch (Exception e)
            {
                Debug.LogError("[EvaFuel] OnLoad(): " + e.ToString());
            }
        }
    }

}