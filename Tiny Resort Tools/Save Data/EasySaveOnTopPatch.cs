using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    // This is being run during the save process (towards the end). It had `EasySaveBuried` running after it though.
    // We might want to move it there? I am not sure the reason it was put here in the first place. 
    // We might inject buried data into the Buried Data Easy Save file (.ES3) 
    
    [HarmonyPatch(typeof(SaveLoad), "EasySaveOnTop")]
    internal class EasySaveOnTopPatch {
        
        [HarmonyPostfix]
        internal static void patch(SaveLoad __instance) {
            TRData.trueSaveEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
            TRData.postSaveEvent?.Invoke();
        }
        
    }
    
}
