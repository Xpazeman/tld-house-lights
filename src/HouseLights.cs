using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace HouseLights
{
    public class ElectrolizerConfig
    {
        public AuroraElectrolizer electrolizer = null;
        public float[] ranges = null;
    }

    class HouseLights
    {
        public static string modsFolder;
        public static string modDataFolder;
        public static string settingsFile = "config.json";

        public static HLOptions options;

        public static bool lightsOn = false;
        public static List<ElectrolizerConfig> electroSources = new List<ElectrolizerConfig>();

        public static List<GameObject> lightSwitches = new List<GameObject>();

        public static void OnLoad()
        {
            Debug.Log("[house-lights] Version " + Assembly.GetExecutingAssembly().GetName().Version);

            modsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            modDataFolder = Path.Combine(modsFolder, "house-lights");

            Settings hlSettings = new Settings();
            hlSettings.AddToModSettings("House Lights Settings");
            options = hlSettings.setOptions;

            RegisterCommands();
        }

        internal static void Init()
        {
            electroSources.Clear();
            lightSwitches.Clear();
            lightsOn = false;
        }

        internal static void AddElectrolizer(AuroraElectrolizer light)
        {
            ElectrolizerConfig newLight = new ElectrolizerConfig
            {
                electrolizer = light,
                ranges = new float[light.m_LocalLights.Length]
            };

            for (int i = 0; i < light.m_LocalLights.Length; i++)
            {
                float curRange = light.m_LocalLights[i].range;
                newLight.ranges[i] = curRange;
            }

            electroSources.Add(newLight);
        }

        internal static void GetSwitches()
        {
            List<GameObject> rObjs = HouseLightsUtils.GetRootObjects();
            List<GameObject> result = new List<GameObject>();

            int wCount = 0;

            foreach (GameObject rootObj in rObjs)
            {
                HouseLightsUtils.GetChildrenWithName(rootObj, "lightswitch", result);

                if (result.Count > 0)
                {
                    foreach (GameObject child in result)
                    {
                        if (child.name != "XPZ_Switch")
                        {
                            child.layer = 12;
                            lightSwitches.Add(child);
                            child.name = "XPZ_Switch";
                            wCount++;
                        }
                    }
                }


            }

            Debug.Log("[house-lights] Light switches found:" + wCount + ".");
        }

        internal static void ToggleLightsState()
        {
            lightsOn = !lightsOn;
        }

        internal static void UpdateElectroLights(AuroraManager mngr)
        {
            for (int e = 0; e < electroSources.Count; e++)
            {
                if (lightsOn)
                {
                    for (int i = 0; i < electroSources[e].electrolizer.m_LocalLights.Length; i++)
                    {
                        float cur_range = electroSources[e].ranges[i];

                        cur_range *= options.rangeMultiplier;
                        cur_range = Math.Min(cur_range, 20f);

                        electroSources[e].electrolizer.m_LocalLights[i].range = cur_range;
                    }

                    HouseLightsUtils.SetPrivFloat(electroSources[e].electrolizer, "m_CurIntensity", options.intensityValue);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateLight", false);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateFX", false);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateEmissiveObjects", false);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateAudio", null);
                }
                else
                {
                    HouseLightsUtils.SetPrivFloat(electroSources[e].electrolizer, "m_CurIntensity", 0f);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateLight", true);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateFX", true);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateEmissiveObjects", true);
                    HouseLightsUtils.InvokePrivMethod(electroSources[e].electrolizer, "UpdateAudio", null);
                }
                
            }
        }

        internal static void RegisterCommands()
        {
            uConsole.RegisterCommand("toggle_lights", ToggleLightsState);
            uConsole.RegisterCommand("thl", ToggleLightsState);
        }
    }
}
