using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;


namespace EvaFuel
{

    public class FileOperations
    {
        public static FileOperations Instance;

        public readonly static string MOD = Assembly.GetAssembly(typeof(EvaFuelManager)).GetName().Name;
        public static readonly String ROOT_PATH = KSPUtil.ApplicationRootPath;
        private string SAVE_PATH = ROOT_PATH + "saves/" + HighLogic.SaveFolder;
        public static string TT_DATAFILE = MOD + ".cfg";
        public static String TT_NODENAME = MOD;

        /// <summary>
        /// Set the Instance upon creation, this will keep this around forever
        /// </summary>
        public FileOperations()
        {
            Instance = this;
        }

        /// <summary>
        /// Get the name of the file to save/load
        /// </summary>
        /// <returns>string with the file name</returns>
        private static string getKerbalEvaFile()
        {
            // This happens when this is called before a save is loaded or created
            if (HighLogic.SaveFolder == "DestructiblesTest" || HighLogic.SaveFolder == "")
                return "";
            return (ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/" + TT_DATAFILE);
        }

        /// <summary>
        /// Load data from a file
        /// </summary>
        /// <returns>Dictionary with the data</returns>
        public Dictionary<string, kerbalEVAFueldata> loadKerbalEvaData()
        {
            ConfigNode configFile = new ConfigNode();
            ConfigNode configFileNode = new ConfigNode();
            ConfigNode[] configDataNodes;

            Dictionary<string, kerbalEVAFueldata> kerbalEVAList = new Dictionary<string, kerbalEVAFueldata>();
            string fname = getKerbalEvaFile();
            if (fname != "" && File.Exists(fname))
            {
                try
                {
                    configFile = ConfigNode.Load(fname);

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
                }
                catch (Exception ex)
                { }

            }
            return kerbalEVAList;
        }

        /// <summary>
        /// Save the kerbalEVAlist data to a file
        /// </summary>
        /// <param name="kerbelEVAList"></param>
        public void saveKerbalEvaData(Dictionary<string, kerbalEVAFueldata> kerbelEVAList)
        {
            string fname = getKerbalEvaFile();
            ConfigNode configFile = new ConfigNode();
            ConfigNode configFileNode = new ConfigNode();
            ConfigNode configDataNode;

            if (fname == "" || kerbelEVAList == null)
                return;

            foreach (var ked in kerbelEVAList)
            {
                configDataNode = new ConfigNode("EvaData");
                configDataNode.SetValue("name", ked.Value.name, true);
                configDataNode.SetValue("evaPropAmt", ked.Value.evaPropAmt, true);

                configFileNode.AddNode("EvaData", configDataNode);

            }
            configFile.SetNode(TT_NODENAME, configFileNode, true);

            configFile.Save(fname);
        }

    }
}
