using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "EasySaveOnTop")]
    internal class EasySaveOnTopPatch {
        [HarmonyPostfix]
        internal static void patch(SaveLoad __instance) {
            TRTools.Log("Finished Saving Game");
            TRData.injectDataEvent?.Invoke();
            TRData.postSaveEvent?.Invoke();
        }
    }
    
}
