using ModSettings;
using System.IO;
using System;
using System.Text;

namespace HouseLights
{
    internal class HouseLightsSettings : JsonModSettings
    {
        [Section("Light strength")]

        [Name("Intensity Value")]
        [Description("Set the intensity for the lights.")]
        [Slider(0f, 3f, 1)]
        public float intensityValue = 2f;

        [Name("Range Multiplier")]
        [Description("Values above 1 make the lights cast light further. 2 will make them reach double the distance than default, 0 turns the lights off.")]
        [Slider(0f, 5f, 1)]
        public float rangeMultiplier = 1.4f;

        [Section("Performance")]

        [Name("Enable Outside")]
        [Description("Toggle to enable or disable the mod while outdoors. Can impact performance, but will make it available in places without a loading screen.")]
        public bool enableOutside = false;

        [Name("Distance Culling")]
        [Description("Reduce this value if you are having performance issues. It will make lights further from this distance be always turned off.")]
        [Slider(10, 50, 1)]
        public int cullDistance = 50;

        [Section("Misc")]

        [Name("Turn off aurora light flicker")]
        [Description("If set to yes, aurora powered lights won't flicker and will stay on.")]
        public bool disableAuroraFlicker = false;

        [Name("Cast Shadows")]
        [Description("If set to yes, lights will cast shadows (can show artifacts and might reduce performance)")]
        public bool castShadows = false;

        [Name("Colorless lights")]
        [Description("If set to yes, lights will cast a more white light. If set to no, they will cast light with the default color.")]
        public bool whiteLights = false;
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
