using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadChangers")]
    public class LoadChangersPatch {
        
        [HarmonyPostfix]
        public static void prefix(SaveLoad __instance) {
            TRTools.Log("Started Loading In");
            TRData.preLoadEvent?.Invoke();
        }
        
        [HarmonyPostfix]
        public static void postfix(SaveLoad __instance) {
            TRTools.Log("Finished Loading In");
            TRTools.InMainMenu = false;
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
        }

    }

}
