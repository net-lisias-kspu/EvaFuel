namespace EvaFuel
{
    public class EvaFuelSettings
    {
        public double EvaTankFuelMax;
		public double FuelConversionFactor;
        public string EvaPropellantName;
        public string ShipPropellantName;
        public string ShipElectricityName;

        public int ScreenMessageLife;
        public int ScreenMessageWarningLife;
        

        public EvaFuelSettings()
        {
            this.EvaTankFuelMax = 5;
            this.EvaPropellantName = "EVA Propellant";
            this.ShipPropellantName = "MonoPropellant";
            this.ShipElectricityName = "ElectricCharge";


            this.ScreenMessageLife = 5;
            this.ScreenMessageWarningLife = 10;

			this.FuelConversionFactor = 1;
        }

        public void Load(ConfigNode node)
        {
            node.TryGetValue("EvaTankFuelMax", ref this.EvaTankFuelMax);
			node.TryGetValue("FuelConversionFactor", ref this.FuelConversionFactor);
            node.TryGetValue("EvaPropellantName", ref this.EvaPropellantName);
            node.TryGetValue("ShipPropellantName", ref this.ShipPropellantName);
            node.TryGetValue("ShipElectricityName", ref this.ShipElectricityName);
            node.TryGetValue("ScreenMessageLife", ref this.ScreenMessageLife);
            node.TryGetValue("ScreenMessageWarningLife", ref this.ScreenMessageWarningLife);
        }
    }
}
