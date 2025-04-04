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
            Log.dbg("ModuleEVAFuel.OnStart");
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
                Log.error(e, "OnStart Error");
                throw;
            }
        }

        private void AssignResourcesToPart()
        {
            Log.dbg("ModuleEVAFuel.AssignResourceToPart");
            try
            {

                evaFuelResource = EVAFuelSettings.Instance.ShipPropellantName;

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

                    double maxAmount = EVAFuelSettings.Instance.resourcesAmtToAdd;
                    if (EVAFuelSettings.Instance.resourcePerCrew)
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
                Log.error(e, "AssignResourcesToPart Error");
                throw;
            }
        }
    }
}
