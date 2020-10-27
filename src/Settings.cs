using ModSettings;
using System.Reflection;

namespace HouseLights
{
    internal class HouseLightsSettings : JsonModSettings
    {
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

        [Name("Cast Shadows")]
        [Description("If set to yes, lights will cast shadows (can show artifacts and might reduce performance)")]
        public bool castShadows = false;

        [Name("Colorless lights")]
        [Description("If set to yes, lights will cast a more white light. If set to no, they will cast light with the default color.")]
        public bool whiteLights = false;

        [Name("Stove genrator")]
        [Description("If setto yes, stoves are working as power generator and needed to be burning (+20C) to get lights. If not, lights work regardless of stove status.")]
        public bool stoveGenerator = false;

        [Name("Generator temp")]
        [Description("Optimal stove temperature for electricity to flow; if temperature is lower than this, it will reduce light intesivity. Recommended 50+")]
        [Slider(10f, 80f)]
        public float stoveGeneratorTemp = 50f;

        [Name("Generator min temp")]
        [Description("Optimal stove temperature for electricity to flow; if temperature is lower than this, it will reduce light intesivity. Recommended 15")]
        [Slider(0f, 15f)]
        public float stoveGeneratorMinTemp = 15f;

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            RefreshFields();
        }

        internal void RefreshFields()
        {
            if (stoveGenerator)
            {
                SetFieldVisible(nameof(stoveGeneratorTemp), true);
                SetFieldVisible(nameof(stoveGeneratorMinTemp), true);
            }
            else
            {
                SetFieldVisible(nameof(stoveGeneratorTemp), false);
                SetFieldVisible(nameof(stoveGeneratorMinTemp), false);
            }
        }
    }
    internal static class Settings
    {
        public static HouseLightsSettings options;

        public static void OnLoad()
        {
            options = new HouseLightsSettings();
            options.AddToModSettings("House Lights Settings");
        }
    }
}
