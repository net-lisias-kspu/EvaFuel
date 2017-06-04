using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using TweakScale;


namespace EvaFuel
{
    public class ModuleEVAFuel : PartModule
    {
        [KSPField(isPersistant = true)]
        public string evaFuelResource = string.Empty;

        [KSPField(isPersistant = true)]
        bool initialized = false;

        [KSPField(isPersistant = true)]
        bool evaFuelResourceAdded = false;

        public double density;
        public double unitCost;
        public double specificHeatCapacity;



        public override void OnStart(PartModule.StartState state)
        {
            Log.Info("ModuleEVAFuel.OnStart");
            try
            {
                if (initialized && HighLogic.LoadedSceneIsEditor)
                {
                    if (evaFuelResourceAdded)
                    {
                        part.Resources.Remove(evaFuelResource);
                        evaFuelResourceAdded = false;
                        evaFuelResource = string.Empty;
                    }
                    initialized = false;
                }
                if (!initialized)
                {
                    this.enabled = true;

                    AssignResourcesToPart();
                    initialized = true;
                } 
            }
            catch (Exception e)
            {
                Log.Error("OnStart Error: " + e.Message);
                throw;
            }
        }

        private void AssignResourcesToPart()
        {
            Log.Info("ModuleEVAFuel.AssignResourceToPart");
            try
            {

                evaFuelResource = HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName;

                if (!part.Resources.Contains(evaFuelResource))
                {
                    ConfigNode resourceNode = new ConfigNode("RESOURCE");

                    PartResourceDefinition resourceDefinition = PartResourceLibrary.Instance.GetDefinition(evaFuelResource);
                    if (resourceDefinition != null)
                    {
                        this.density = resourceDefinition.density;
                        this.unitCost = resourceDefinition.unitCost;
                        this.specificHeatCapacity = resourceDefinition.specificHeatCapacity;
                    }

                    var maxAmount = HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().resourcesAmtToAdd;
                    if (HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().resourcePerCrew)
                        maxAmount *= this.part.CrewCapacity;

                    resourceNode.AddValue("name", evaFuelResource);
                    resourceNode.AddValue("maxAmount", maxAmount);
                    resourceNode.AddValue("amount", maxAmount);

                    part.Resources.Add(resourceNode);
                    var r = part.Resources.Get(evaFuelResource);
                    r.flowMode = PartResource.FlowMode.None;
                    evaFuelResourceAdded = true;
                }
            }
            catch (Exception e)
            {
                Log.Error("AssignResourcesToPart Error " + e.Message);
                throw;
            }
        }
    }
}
