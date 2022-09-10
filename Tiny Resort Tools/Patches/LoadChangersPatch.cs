using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadChangers")]
    public class LoadChangersPatch {
        
        [HarmonyPostfix]
        public static void prefix(SaveLoad __instance) {
            TRData.preLoadEvent?.Invoke();
        }
        
        [HarmonyPostfix]
        public static void postfix(SaveLoad __instance) {
            TRTools.InMainMenu = false;
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
        }

    }

}
