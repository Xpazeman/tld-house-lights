using System;
using Harmony;
using MelonLoader;
using UnityEngine;

namespace HouseLights
{
    class Patches
    {
        [HarmonyPatch(typeof(GameManager), "InstantiatePlayerObject")]
        internal class GameManager_InstantiatePlayerObject
        {
            public static void Prefix()
            {
                if (!InterfaceManager.IsMainMenuActive())
                {
                    HouseLights.Init();
                    HouseLights.GetSwitches();
                }
            }
        }

    [HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted")]
        internal class MissionServicesManager_SceneLoadCompleted
        {
            private static void Postfix(MissionServicesManager __instance)
            {
                // Fire_Update_Prefix will not run if there are no fire sources (eg post office)
                if (Settings.options.stoveGenerator)
                {
                    HouseLights.stoveHeatRatio = 0f;
                }
                else
                {
                    HouseLights.stoveHeatRatio = 1f;
                }
                // if not stove generator, scan extenral and indoors scenes, if using it, scan only internal ones
                if (!Settings.options.stoveGenerator || (Settings.options.stoveGenerator && GameManager.GetWeatherComponent().IsIndoorScene())) {
                    HouseLights.stoveTempIncr = 0f;
                    HouseLights.GetSwitches();
                }
            }
        }

        [HarmonyPatch(typeof(AuroraElectrolizer), "Initialize")]
        internal class AuroraElectrolizer_Initialize
        {
            private static void Postfix(AuroraElectrolizer __instance)
            {
                AuroraActivatedToggle[] radios = __instance.gameObject.GetComponentsInParent<AuroraActivatedToggle>();
                AuroraScreenDisplay[] screens = __instance.gameObject.GetComponentsInChildren<AuroraScreenDisplay>();

                if (radios.Length == 0 && screens.Length == 0)
                {
                    HouseLights.AddElectrolizer(__instance);
                }

            }
        }

        [HarmonyPatch(typeof(AuroraManager), "RegisterAuroraLightSimple", new Type[] { typeof(AuroraLightingSimple) })]
        internal class AuroraManager_RegisterLightSimple
        {
            private static void Postfix(AuroraManager __instance, AuroraLightingSimple auroraLightSimple)
            {
                HouseLights.AddElectrolizerLight(auroraLightSimple);
            }
        }


        //[HarmonyPatch(typeof(AuroraManager), "Update")]
        [HarmonyPatch(typeof(AuroraManager), "UpdateForceAurora")]
        internal class AuroraManager_UpdateForceAurora
        {
            private static void Postfix(AuroraManager __instance)
            {
                if (HouseLights.electroSources.Count > 0)
                {
                    HouseLights.UpdateElectroLights(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "GetInteractiveObjectDisplayText", new Type[] {typeof(GameObject)})]
        internal class PlayerManager_GetObjText
        {
            private static void Postfix(PlayerManager __instance, ref string __result, GameObject interactiveObject)
            {
                if (interactiveObject.name == "XPZ_Switch")
                {
                    if (HouseLights.lightsOn)
                    {
                        __result = "Turn Lights Off";
                    }
                    else
                    {
                        __result = "Turn Lights On";
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")]
        internal class PlayerManager_InteractiveObjectsProcessInteraction
        {
            private static void Postfix(PlayerManager __instance, ref bool __result)
            {
                if (__instance.m_InteractiveObjectUnderCrosshair != null && __instance.m_InteractiveObjectUnderCrosshair.name == "XPZ_Switch")
                {
                    HouseLights.ToggleLightsState();
                    Vector3 scale = __instance.m_InteractiveObjectUnderCrosshair.transform.localScale;
                    __instance.m_InteractiveObjectUnderCrosshair.transform.localScale = new Vector3(scale.x, scale.y * -1, scale.z);

                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(AuroraElectrolizer), "UpdateIntensity")]
        internal class AuroraElectrolizer_UpdateIntensity
        {
            private static bool Prefix(AuroraElectrolizer __instance)
            {
                if (Settings.options.disableAuroraFlicker)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(Weather), "IsTooDarkForAction", new Type[] { typeof(ActionsToBlock) })]
        internal class Weather_IsTooDarkForAction
        {
            private static void Postfix(Weather __instance, ref bool __result)
            {
                if (__result && GameManager.GetWeatherComponent().IsIndoorScene() && HouseLights.lightsOn && (HouseLights.stoveHeatRatio >= 0f))
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Fire), "Update")]
        internal class Fire_Update_Prefix
        {
            private static void Prefix(Fire __instance)
            {
                if (!GameManager.m_IsPaused && Settings.options.stoveGenerator)
                {
                    if (__instance.m_HeatSource.IsTurnedOn() && __instance.GetFireState() != FireState.Off)
                    {
                        GameObject obj = __instance.transform.GetParent()?.gameObject;
                        float ratio = 0f;
                        if (obj && (obj.name.ToLower().Contains("woodstove") || obj.name.ToLower().Contains("potbellystove")))
                        {
                            // get warmest stove in scene
                            float currTempIncr = __instance.GetCurrentTempIncrease();
                            float throttleDownSec = Settings.options.stoveGeneratorThrottleDown * 60f;
                            if (currTempIncr > HouseLights.stoveTempIncr)
                            {
                                HouseLights.stoveTempIncr = __instance.GetCurrentTempIncrease();
                            }
                            /*
                            buring out fire does not reduce heat in vanilla game
                            While we will not fix this (and in a way ember state does keep heat level), we will throttle down electricity output on last 10mins
                            */
                            ratio = Mathf.InverseLerp(Settings.options.stoveGeneratorMinTemp, Settings.options.stoveGeneratorTemp, HouseLights.stoveTempIncr);
                            if (__instance.GetRemainingLifeTimeSeconds() < throttleDownSec && __instance.m_ElapsedOnTODSecondsUnmodified > throttleDownSec)
                            {
                                 ratio *= Mathf.InverseLerp(0, throttleDownSec, __instance.GetRemainingLifeTimeSeconds());
                            }
                            HouseLights.stoveHeatRatio = ratio;
                        }
                    }
                }
            }
        }
    }
}
