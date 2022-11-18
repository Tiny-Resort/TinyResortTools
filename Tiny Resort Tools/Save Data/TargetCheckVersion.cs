using HarmonyLib;

namespace TinyResort;

// Due to timing of re-adding items, the check version message gets sent to players. 
// This message is located in the wrong place in Vanilla gameplay and doesn't function properly
// so I am disabling it. 
[HarmonyPatch(typeof(CharMovement), "TargetCheckVersion")]
[HarmonyPriority(1)]
internal class TargetCheckVersion {
    [HarmonyPrefix] public static bool Prefix() => false;
}
