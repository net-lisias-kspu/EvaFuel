using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EvaFuel
{

    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class EVAFuelSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "General Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "EVA Fuel"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        

        [GameParameters.CustomFloatParameterUI("EVA Fuel Tank Max", minValue = 0.5f, maxValue = 15.0f, asPercentage = false, displayFormat = "0.0",
           toolTip = "Maximum amount of EVA fuel")]
        public double EvaTankFuelMax = 5.0f;

        [GameParameters.CustomFloatParameterUI("EVA Fuel Conversion Factor", minValue = 0.1f, maxValue = 10.0f, asPercentage = false, displayFormat = "0.0",
          toolTip = "Ratio")]
        public double FuelConversionFactor = 1.0f;

        [GameParameters.CustomStringParameterUI("EVA Propellent Type", autoPersistance = true, lines = 1, title = "EVA Propellent Type")]
        public string EvaPropellantName = "EVA Propellant";

        [GameParameters.CustomStringParameterUI("Ship Propellent Type", autoPersistance = true, lines = 1, title = "Ship Propellent Type")]
         public string ShipPropellantName = "MonoPropellant";


        [GameParameters.CustomStringParameterUI("Ship Electricity Name", autoPersistance = true, lines = 2, title = "Ship Electricity Name")]
        public string ShipElectricityName = "ElectricCharge";

        [GameParameters.CustomIntParameterUI("Screen Message Life", maxValue = 10)]
        public int ScreenMessageLife = 5;

        [GameParameters.CustomIntParameterUI("Screen Message Warning Life", maxValue = 10)]
        public int ScreenMessageWarningLife = 10;

        [GameParameters.CustomParameterUI("Fill from Pod")]
        public bool fillFromPod = true;


#if false
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;

                case GameParameters.Preset.Normal:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;

                case GameParameters.Preset.Moderate:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;

                case GameParameters.Preset.Hard:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;
            }
        }
#endif
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            return true;
            //            return true; //otherwise return true
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }

    }

}