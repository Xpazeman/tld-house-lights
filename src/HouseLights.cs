using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace HouseLights
{
    public class ElectrolizerConfig : MelonMod
    {
        public AuroraElectrolizer electrolizer = null;
        public float[] ranges = null;
        public Color[] colors = null;
    }

    public class ElectrolizerLightConfig : MelonMod
    {
        public AuroraLightingSimple electrolizer = null;
        public float[] ranges = null;
        public Color[] colors = null;
    }

    class HouseLights : MelonMod
    {
        public static bool lightsOn = false;
        public static List<ElectrolizerConfig> electroSources = new List<ElectrolizerConfig>();
        public static List<ElectrolizerLightConfig> electroLightSources = new List<ElectrolizerLightConfig>();

        public static List<GameObject> lightSwitches = new List<GameObject>();
        public static float stoveHeatRatio = 0f;
        public static float stoveTempIncr = 0f;
        public static bool onlyLow = true;

        public static List<string> notReallyOutdoors = new List<string>
        {
            "DamTransitionZone"
        };

        public override void OnApplicationStart()
        {
            Settings.OnLoad();
            Debug.Log("[house-lights] Version " + Assembly.GetExecutingAssembly().GetName().Version);
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
                ranges = new float[light.m_LocalLights.Length],
                colors = new Color[light.m_LocalLights.Length]
            };

            for (int i = 0; i < light.m_LocalLights.Length; i++)
            {
                float curRange = light.m_LocalLights[i].range;
                Color curColor = light.m_LocalLights[i].color;
                newLight.ranges[i] = curRange;
                newLight.colors[i] = curColor;
            }

            electroSources.Add(newLight);
        }

        internal static void AddElectrolizerLight(AuroraLightingSimple light)
        {
            ElectrolizerLightConfig newLight = new ElectrolizerLightConfig
            {
                electrolizer = light,
                ranges = new float[light.m_LocalLights.Length],
                colors = new Color[light.m_LocalLights.Length]
            };

            for (int i = 0; i < light.m_LocalLights.Length; i++)
            {
                float curRange = light.m_LocalLights[i].range;
                Color curColor = light.m_LocalLights[i].color;
                newLight.ranges[i] = curRange;
                newLight.colors[i] = curColor;
            }

            electroLightSources.Add(newLight);
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
            // this should be redundant but in case someone change settings in active scene, this should 'reset' ratio
            if (!Settings.options.stoveGenerator)
            {
                stoveHeatRatio = 1f;
            }

            for (int e = 0; e < electroSources.Count; e++)
            {
                if (electroSources[e].electrolizer != null && electroSources[e].electrolizer.m_LocalLights != null)
                {

                    for (int i = 0; i < electroSources[e].electrolizer.m_LocalLights.Length; i++)
                    {
                        float cur_range = electroSources[e].ranges[i];

                        cur_range *= Settings.options.rangeMultiplier;
                        cur_range = Math.Min(cur_range, 20f);

                        electroSources[e].electrolizer.m_LocalLights[i].range = cur_range;

                        ColorHSV curColor = electroSources[e].colors[i];

                        if (Settings.options.whiteLights)
                            curColor.s *= 0.15f;

                        electroSources[e].electrolizer.m_LocalLights[i].color = curColor;

                        if (Settings.options.castShadows)
                        {
                            electroSources[e].electrolizer.m_LocalLights[i].shadows = LightShadows.Soft;
                        }
                    }

                    if (lightsOn && !mngr.AuroraIsActive())
                    {
                        if (!electroSources[e].electrolizer.gameObject.name.Contains("Alarm") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Headlight") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Taillight") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Television") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Computer") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Machine") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("ControlBox") &&
                            !electroSources[e].electrolizer.gameObject.name.Contains("Interiorlight"))
                        {
                            electroSources[e].electrolizer.m_CurIntensity = (Settings.options.intensityValue * stoveHeatRatio);
                            electroSources[e].electrolizer.UpdateLight(false);
                            electroSources[e].electrolizer.UpdateFX(false);
                            electroSources[e].electrolizer.UpdateEmissiveObjects(false);
                            electroSources[e].electrolizer.StopAudio();
                        }
                    }
                    else if (!mngr.AuroraIsActive())
                    {
                        electroSources[e].electrolizer.m_CurIntensity = 0f;
                        electroSources[e].electrolizer.UpdateLight(true);
                        electroSources[e].electrolizer.UpdateFX(true);
                        electroSources[e].electrolizer.UpdateEmissiveObjects(true);
                        electroSources[e].electrolizer.UpdateAudio();
                    }
                    else
                    {
                        electroSources[e].electrolizer.UpdateIntensity(Time.deltaTime, true);
                    }
                }
            }

            for (int e = 0; e < electroLightSources.Count; e++)
            {
                if (electroLightSources[e].electrolizer != null && electroLightSources[e].electrolizer.m_LocalLights != null)
                {

                    for (int i = 0; i < electroLightSources[e].electrolizer.m_LocalLights.Length; i++)
                    {
                        float cur_range = electroLightSources[e].ranges[i];

                        cur_range *= Settings.options.rangeMultiplier;
                        cur_range = Math.Min(cur_range, 20f);

                        electroLightSources[e].electrolizer.m_LocalLights[i].range = cur_range;

                        ColorHSV curColor = electroLightSources[e].colors[i];

                        if (Settings.options.whiteLights)
                            curColor.s *= 0.15f;

                        electroLightSources[e].electrolizer.m_LocalLights[i].color = curColor;

                        if (Settings.options.castShadows)
                        {
                            electroLightSources[e].electrolizer.m_LocalLights[i].shadows = LightShadows.Soft;
                        }
                    }

                    if (lightsOn && !mngr.AuroraIsActive())
                    {
                        if (!electroLightSources[e].electrolizer.gameObject.name.Contains("Alarm") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Headlight") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Taillight") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Television") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Computer") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Machine") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("ControlBox") &&
                            !electroLightSources[e].electrolizer.gameObject.name.Contains("Interiorlight"))
                        {
                            electroLightSources[e].electrolizer.m_CurIntensity = (Settings.options.intensityValue * stoveHeatRatio);
                            electroLightSources[e].electrolizer.UpdateLight(false);
                            electroLightSources[e].electrolizer.UpdateEmissiveObjects(false);
                            electroLightSources[e].electrolizer.StopAudio();
                        }
                    }
                    else if (!mngr.AuroraIsActive())
                    {
                        electroLightSources[e].electrolizer.m_CurIntensity = 0f;
                        electroLightSources[e].electrolizer.UpdateLight(true);
                        electroLightSources[e].electrolizer.UpdateEmissiveObjects(true);
                        electroLightSources[e].electrolizer.UpdateAudio();
                    }
                    else
                    {
                        electroLightSources[e].electrolizer.UpdateIntensity(Time.deltaTime);
                    }
                }
            }
        }

        internal static void RegisterCommands()
        {
            /*uConsole.RegisterCommand("toggle_lights", ToggleLightsState);
            uConsole.RegisterCommand("thl", ToggleLightsState);*/
        }
    }
}
