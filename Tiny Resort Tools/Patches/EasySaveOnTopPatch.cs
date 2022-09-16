using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "EasySaveOnTop")]
    internal class EasySaveOnTopPatch {
        
        [HarmonyPostfix]
        internal static void patch(SaveLoad __instance) {
            TRData.injectDataEvent?.Invoke();
            TRData.postSaveEvent?.Invoke();
        }
        
    }
    
}
