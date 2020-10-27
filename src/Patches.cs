using System;
using Harmony;
using Il2CppSystem.Security.Cryptography.X509Certificates;
using MelonLoader;
using UnityEngine;

namespace HouseLights
{
    class Patches
    {
        /*[HarmonyPatch(typeof(GameManager), "Awake")]
        internal class GameManager_Awake
        {
            public static void Prefix()
            {
                if (!InterfaceManager.IsMainMenuActive())
                {
                    HouseLights.Init();
                }
            }
        }*/

        [HarmonyPatch(typeof(GameManager), "InstantiatePlayerObject")]
        internal class GameManager_InstantiatePlayerObject
        {
            public static void Prefix()
            {
                if (!InterfaceManager.IsMainMenuActive())
                {
                    HouseLights.Init();
                }
            }
        }


    [HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted")]
        internal class MissionServicesManager_SceneLoadCompleted
        {
            private static void Postfix(MissionServicesManager __instance)
            {
                // We do not need to scan when outside, right ?
                if (GameManager.GetWeatherComponent().IsIndoorScene()) {
                    HouseLights.GetSwitches();
                    //HouseLights.GetStoves();
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
                if (__result && GameManager.GetWeatherComponent().IsIndoorScene() && HouseLights.lightsOn && HouseLights.stoveHeatRatio >= 0.5f)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Fire), "Update")]
        internal class Fire_Update_Prefix
        {
            private static bool Prefix(Fire __instance)
            {
                if (!Settings.options.stoveGenerator) { return false; }
                if (GameManager.m_IsPaused) { return false; }
                // MelonLogger.Log(__instance.GetCurrentTempIncrease());

                float tempIncr = 0f;
                float currTempIncr = 0f;

                if (__instance.m_HeatSource.IsTurnedOn() && __instance.GetFireState() != FireState.Off) {
                    GameObject obj = __instance.transform.GetParent().gameObject;
                    if ((bool)obj.name.ToLower().Contains("woodstove") || obj.name.ToLower().Contains("potbellystove"))
                    {
                        // get warmest stove in scene
                        currTempIncr = __instance.GetCurrentTempIncrease();
                        if ( currTempIncr > tempIncr)
                        {
                            tempIncr = __instance.GetCurrentTempIncrease();
                        }
                        HouseLights.stoveHeatRatio = Mathf.Clamp(((tempIncr - Settings.options.stoveGeneratorMinTemp) / (Settings.options.stoveGeneratorTemp - Settings.options.stoveGeneratorMinTemp)), 0f, 1f);
                        //MelonLogger.Log(obj.name + " " + __instance.GetCurrentTempIncrease() + " " + HouseLights.stoveHeatRatio);
                    }
                }
                return false;
            }
        }
    }
}
