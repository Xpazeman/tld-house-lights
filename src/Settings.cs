using ModSettings;
using System.IO;
using System;
using System.Text;

namespace HouseLights
{
    internal class HLOptions
    {
        public float rangeMultiplier = 1.4f;
        public float intensityValue = 2f;

        public bool disableAuroraFlicker = false;
    }

    internal class Settings : ModSettingsBase
    {
        internal readonly HLOptions setOptions = new HLOptions();

        [Name("Intensity Value")]
        [Description("Set the intensity for the lights.")]
        [Slider(0f, 3f, 1)]
        public float intensityValue = 2f;

        [Name("Range Multiplier")]
        [Description("Values above 1 make the lights cast light further. 2 will make them reach double the distance than default, 0 turns the lights off.")]
        [Slider(0f, 5f, 1)]
        public float rangeMultiplier = 1.4f;

        [Name("Turn off aurora light flicker")]
        [Description("If set to yes, aurora powered lights won't flicker and will stay on.")]
        public bool disableAuroraFlicker = false;

        internal Settings()
        {
            if (File.Exists(Path.Combine(HouseLights.modDataFolder, HouseLights.settingsFile)))
            {
                string opts = File.ReadAllText(Path.Combine(HouseLights.modDataFolder, HouseLights.settingsFile));
                setOptions = FastJson.Deserialize<HLOptions>(opts);

                intensityValue = setOptions.intensityValue;
                rangeMultiplier = setOptions.rangeMultiplier;
                disableAuroraFlicker = setOptions.disableAuroraFlicker;
            }
        }

        protected override void OnConfirm()
        {
            setOptions.intensityValue = (float)Math.Round(intensityValue, 1);
            setOptions.rangeMultiplier = (float)Math.Round(rangeMultiplier, 1);
            setOptions.disableAuroraFlicker = disableAuroraFlicker;

            string jsonOpts = FastJson.Serialize(setOptions);

            File.WriteAllText(Path.Combine(HouseLights.modDataFolder, HouseLights.settingsFile), jsonOpts);
        }
    }
}
