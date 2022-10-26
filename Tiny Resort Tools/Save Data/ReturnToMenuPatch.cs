using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "returnToMenu")]
    internal class ReturnToMenuPatch {
        
        [HarmonyPostfix]
        internal static void patch(SaveLoad __instance) {
            TRTools.LeavingMainMenu = false;
            TRTools.InMainMenu = true;
        }
        
    }

}
