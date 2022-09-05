using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "EasySaveOnTop")]
    public class EasySaveOnTopPatch {
        [HarmonyPostfix]
        public static void patch(SaveLoad __instance) {
            TRTools.Log("Finished Saving Game");
            TRData.injectDataEvent?.Invoke();
            TRData.postSaveEvent?.Invoke();
        }
    }
    
}
