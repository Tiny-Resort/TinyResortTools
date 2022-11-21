using HarmonyLib;

namespace TinyResort;

[HarmonyPatch(typeof(RenderMap), "runMapFollow")]
internal class RenderMapPatch {

    // Forcibly clears the top notification so that it can be replaced immediately
    [HarmonyPostfix]
    internal static void runMapFollowPatch(RenderMap __instance) => TRMap.FixMarkerScale();
}
