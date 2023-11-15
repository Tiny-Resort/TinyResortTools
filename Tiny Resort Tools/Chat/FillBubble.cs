using HarmonyLib;

namespace TinyResort {
    [HarmonyPatch(typeof(ChatBubble), "fillBubble")]
    internal class FillBubble {

        [HarmonyPostfix]
        public static void Postfix(ChatBubble __instance, string name, string message) {
            if (string.IsNullOrEmpty(name)) __instance.contents.text = message;
        }
    }
}
