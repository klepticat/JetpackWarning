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
                    float jetpackPower = (float)typeof(JetpackItem).GetField("jetpackPower", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(jetpack);

                    RaycastHit hit;
                    Physics.Raycast(__instance.transform.position, forces, out hit, 25f, StartOfRound.Instance.allPlayersCollideWithMask);
                    
                    float fill_acceleration, fill_real_speed, fill, interpolation;
                    
                    // If jetpackPower is over 55, the user is probably flying in circles,
                    // which is safe until jetpackPower becomes very high and things get out of control.
                    if(jetpackPower < 80){
                        interpolation = Mathf.Clamp(jetpackPower / 25f - 2.2f, 0, 1f) / 2f; // jetpackPower 55-80, 0 to 0.5
                    }
                    else{
                        interpolation = 0.5f - Mathf.Clamp(jetpackPower / 20f - 4f, 0, 1f) / 2f; // jetpackPower 80-100, 0.5 to 0
                    }
                    if(forces.magnitude > 47){ // Switch meter to show real speed when near exploding
                        interpolation = Mathf.Clamp(forces.magnitude / 3f - 15.6666f, 0, 1f); // forces.magnitude 47-50, 0 to 1
                    }
                    fill_real_speed = forces.magnitude - hit.distance >= 0 ? (forces.magnitude- hit.distance) / 50f : 0f;
                    fill_acceleration = (forces.magnitude/2) + (jetpackPower/2.2f) - hit.distance >= 0 ? ((forces.magnitude/2) + (jetpackPower/2.2f) - hit.distance) / 50f : 0f;
                    fill = Mathf.Lerp(fill_acceleration, fill_real_speed, interpolation);

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

        [HarmonyPatch(typeof(JetpackItem), "SetJetpackAudios")]
        [HarmonyPrefix]
        static bool JetpackItem_SetJetpackAudios_Prefix(ref bool ___jetpackActivated, ref AudioSource ___jetpackBeepsAudio) {
            return !playingJetpackCritical;
        }

        [HarmonyPatch(typeof(JetpackItem), "JetpackEffect")]
        [HarmonyPostfix]
        static void JetpackItem_JetpackEffect_Postfix(ref bool __0, JetpackItem __instance) {
            if(__0){ // jetpack has been enabled
                if(playJetpackCritical){
                    playingJetpackCritical = true;
                    __instance.jetpackBeepsAudio.clip = JetpackWarningPlugin.jetpackCriticalBeep;
                    __instance.jetpackBeepsAudio.Play();
                }
            }
            return;
        }
    }
}