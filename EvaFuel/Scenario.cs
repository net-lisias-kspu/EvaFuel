using System;
using System.Linq;
using System.Text;
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