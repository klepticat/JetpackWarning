using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace JetpackWarning {
    class Patches {
        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        [HarmonyPostfix]
        static void PlayerControllerB_LateUpdate_Postfix(ref PlayerControllerB __instance) {
            if(__instance.isHoldingObject && __instance.currentlyHeldObjectServer != null && __instance.currentlyHeldObjectServer is JetpackItem) {
                JetpackWarningPlugin.meterContainer.SetActive(true);
            } else {
                JetpackWarningPlugin.meterContainer.SetActive(false);
            }
        }

        static bool playJetpackCritical = false;
        static bool playingJetpackCritical = false;

        [HarmonyPatch(typeof(JetpackItem), "Update")]
        [HarmonyPostfix]
        static void JetpackItem_Update_Postfix(ref JetpackItem __instance, ref Vector3 ___forces, ref RaycastHit ___rayHit) {
            if(__instance.heldByPlayerOnServer) {
                float fill = ___forces.magnitude - ___rayHit.distance >= 0f ? (___forces.magnitude - ___rayHit.distance) / 50f : 0f;
                JetpackWarningPlugin.meter.GetComponent<Image>().fillAmount = fill;
                JetpackWarningPlugin.warning.SetActive(fill > 0.85f);

                playJetpackCritical = fill > 0.85f;

                Color meterColor = Color.Lerp(new Color(1f, 0.82f, 0.405f, 1f), new Color(0.769f, 0.243f, 0.243f, 1f), fill);

                JetpackWarningPlugin.meter.GetComponent<Image>().color = meterColor;
                JetpackWarningPlugin.frame.GetComponent<Image>().color = meterColor;
                JetpackWarningPlugin.warning.GetComponent<Image>().color = meterColor;
            }
        }

        [HarmonyPatch(typeof(JetpackItem), "SetJetpackAudios")]
        [HarmonyPrefix]
        static bool JetpackItem_SetJetpackAudios_Prefix(ref bool ___jetpackActivated, ref AudioSource ___jetpackBeepsAudio) {
            if(playJetpackCritical) {
                if(!playingJetpackCritical) {
                    playingJetpackCritical = true;
                    ___jetpackBeepsAudio.clip = JetpackWarningPlugin.jetpackCriticalBeep;
                    ___jetpackBeepsAudio.Play();
                }

                return false;
            } else {
                playingJetpackCritical = false;
                return true;
            }
        }
    }
}