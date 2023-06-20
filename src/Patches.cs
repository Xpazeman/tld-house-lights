using System;
using Il2Cpp;
using HarmonyLib;
using UnityEngine;
using Il2CppTLD.ModularElectrolizer;
using MelonLoader;

namespace HouseLights
{
    class Patches
    {
        [HarmonyPatch(typeof(GameManager), "InstantiatePlayerObject")]
        internal class GameManager_InstantiatePlayerObject
        {
            public static void Prefix()
            {
                if (!InterfaceManager.IsMainMenuEnabled() && (!GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) || HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene)))
                {
                    MelonLogger.Msg("Scene Init");

                    HouseLights.Init();
                    HouseLights.GetSwitches();
                }
            }
        }

        [HarmonyPatch(typeof(AuroraModularElectrolizer), "Initialize")]
        internal class AuroraElectrolizer_Initialize
        {
            private static void Postfix(AuroraModularElectrolizer __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled() || (GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) && !HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene)))
                {
                    return;
                }

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
                if (InterfaceManager.IsMainMenuEnabled() || (GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) && !HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) && !Settings.options.enableOutside))
                {
                    return;
                }

                HouseLights.AddElectrolizerLight(auroraLightSimple);
            }
        }

        [HarmonyPatch(typeof(AuroraManager), "UpdateForceAurora")]
        internal class AuroraManager_UpdateForceAurora
        {
            private static void Postfix(AuroraManager __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled() || (GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) && !HouseLights.notReallyOutdoors.Contains(GameManager.m_ActiveScene) && !Settings.options.enableOutside))
                {
                    return;
                }

                if (HouseLights.electroSources.Count > 0)
                {
                    HouseLights.UpdateElectroLights(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.UpdateHUDText), new Type[] { typeof(Panel_HUD) })]
        internal class PlayerManage_UpdateHUDText
        {
            private static void Postfix(PlayerManager __instance, Panel_HUD hud)
            {
                if (GameManager.GetMainCamera() == null) return;
                
                GameObject interactiveObject = __instance.GetInteractiveObjectUnderCrosshairs(100);
                string hoverText;

                if (interactiveObject != null && interactiveObject.name == "XPZ_Switch")
                {
                    if (HouseLights.lightsOn)
                    {
                        hoverText = "Turn Lights Off";
                    }
                    else
                    {
                        hoverText = "Turn Lights On";
                    }

                    hud.SetHoverText(hoverText, interactiveObject, HoverTextState.CanInteract);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction")]
        internal class PlayerManager_InteractiveObjectsProcessInteraction
        {
            private static void Postfix(PlayerManager __instance, ref bool __result)
            {
                GameObject interactiveObject = __instance.GetInteractiveObjectUnderCrosshairs(100);

                if (interactiveObject != null && interactiveObject.name == "XPZ_Switch")
                {
                    HouseLights.ToggleLightsState();
                    GameAudioManager.PlaySound("Stop_RadioAurora", __instance.gameObject);

                    Vector3 scale = interactiveObject.transform.localScale;
                    interactiveObject.transform.localScale = new Vector3(scale.x, scale.y * -1, scale.z);

                    //Play Sound

                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(AuroraModularElectrolizer), "UpdateIntensity")]
        internal class AuroraElectrolizer_UpdateIntensity
        {
            private static bool Prefix(AuroraModularElectrolizer __instance)
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
                if (__result && GameManager.GetWeatherComponent().IsIndoorScene() && HouseLights.lightsOn)
                {
                    __result = false;
                }
            }
        }
    }
}
