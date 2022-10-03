using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadChangers")]
    internal class LoadChangersPatch {
        
        [HarmonyPrefix]
        internal static void prefix(SaveLoad __instance) {
            if (TRTools.InMainMenu) { TRData.initialLoadEvent?.Invoke(); }
            TRData.preLoadEvent?.Invoke();
        }
        
        [HarmonyPostfix]
        internal static void postfix(SaveLoad __instance) {
            TRTools.InMainMenu = false;
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
        }

    }

}
