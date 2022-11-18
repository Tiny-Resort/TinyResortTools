using HarmonyLib;
using TMPro;
using UnityEngine;

namespace TinyResort;

[HarmonyPatch(typeof(TMP_InputField), "ActivateInputFieldInternal")]
internal class ActivateInputFieldInternal {

    [HarmonyPostfix]
    internal static void patch(TMP_InputField __instance) {
        if (Time.realtimeSinceStartup - OpenChat.SlashOpenedChat < 0.25f) {
            __instance.text = "/";
            __instance.MoveToEndOfLine(false, false);
        }
    }
}
