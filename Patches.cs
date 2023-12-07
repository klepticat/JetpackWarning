using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace JetpackWarning {
    class Patches {
        static private bool playJetpackCritical = false;
        static private bool playingJetpackCritical = false;
        static private float currentFill = 0f;
        static private float fillVelocity = 0f;

        static private float criticalFill = 0.75f;
        static private float fillTime = 0.1f;

        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        [HarmonyPostfix]
        static void PlayerControllerB_LateUpdate_Postfix(ref PlayerControllerB __instance) {
            if(__instance.IsOwner && (!__instance.IsServer || __instance.isHostPlayerObject) && __instance.isPlayerControlled && !__instance.isPlayerDead) {
                if(__instance.isHoldingObject && __instance.currentlyHeldObjectServer is JetpackItem) {
                    JetpackItem jetpack = (JetpackItem)__instance.currentlyHeldObjectServer;
                    JetpackWarningPlugin.meterContainer.SetActive(true);

                    Vector3 forces = (Vector3)typeof(JetpackItem).GetField("forces", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(jetpack);

                    RaycastHit hit;
                    Physics.Raycast(__instance.transform.position, forces, out hit, 25f, StartOfRound.Instance.allPlayersCollideWithMask);

                    float fill = forces.magnitude - hit.distance >= 0f ? (forces.magnitude - hit.distance) / 50f : 0f;
                    JetpackWarningPlugin.meter.GetComponent<Image>().fillAmount = currentFill;
                    JetpackWarningPlugin.warning.SetActive(currentFill > criticalFill);

                    currentFill = Mathf.SmoothDamp(currentFill, fill, ref fillVelocity, fillTime);

                    playJetpackCritical = currentFill > criticalFill;

                    Color meterColor = Color.Lerp(new Color(1f, 0.82f, 0.405f, 1f), new Color(0.769f, 0.243f, 0.243f, 1f), currentFill);

                    JetpackWarningPlugin.meter.GetComponent<Image>().color = meterColor;
                    JetpackWarningPlugin.frame.GetComponent<Image>().color = meterColor;
                    JetpackWarningPlugin.warning.GetComponent<Image>().color = meterColor;

                    if(playJetpackCritical) {
                        if(!playingJetpackCritical) {
                            playingJetpackCritical = true;
                            jetpack.jetpackBeepsAudio.clip = JetpackWarningPlugin.jetpackCriticalBeep;
                            jetpack.jetpackBeepsAudio.Play();
                        }
                    } else playingJetpackCritical = false;
                } else {
                    JetpackWarningPlugin.meterContainer.SetActive(false);
                }
            }
        }

        // the jetpack critical beep doesn't work properly when releasing LMB and then pressing it again while critical
        // need to look into this
        [HarmonyPatch(typeof(JetpackItem), "SetJetpackAudios")]
        [HarmonyPrefix]
        static bool JetpackItem_SetJetpackAudios_Prefix(ref bool ___jetpackActivated, ref AudioSource ___jetpackBeepsAudio) {
            return !playingJetpackCritical;
        }
    }
}