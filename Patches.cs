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

        [HarmonyPatch(typeof(JetpackItem), "Update")]
        [HarmonyPostfix]
        static void JetpackItem_Update_Postfix(ref JetpackItem __instance, ref Vector3 ___forces, ref RaycastHit ___rayHit) {
            if(__instance.heldByPlayerOnServer) {
                float fill = ___forces.magnitude - ___rayHit.distance >= 0f ? (___forces.magnitude - ___rayHit.distance) / 50f : 0f;
                JetpackWarningPlugin.meter.GetComponent<Image>().fillAmount = fill;
                JetpackWarningPlugin.warning.SetActive((fill * 100f) > 75f);
            }
        }
    }
}
