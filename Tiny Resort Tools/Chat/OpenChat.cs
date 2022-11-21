using HarmonyLib;
using UnityEngine;

namespace TinyResort;

[HarmonyPatch(typeof(InputMaster), "OpenChat")]
internal class OpenChat {

    public static float SlashOpenedChat;

    [HarmonyPostfix]
    internal static void postfix(InputMaster __instance, ref bool __result) {
        if (LeadPlugin.useSlashToOpenChat.Value && !__result && !ChatBox.chat.chatOpen
         && Input.GetKeyDown(KeyCode.Slash)) {
            __result = true;
            SlashOpenedChat = Time.realtimeSinceStartup;
        }
    }
}
