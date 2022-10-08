using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveSlotButton), "onPress")]
    internal class SaveSlotButtonPatch {
        
        [HarmonyPrefix]
        internal static void Prefix(SaveLoad __instance) {
            if (TRTools.InMainMenu) { TRData.initialLoadEvent?.Invoke(); }
            TRData.preLoadEvent?.Invoke();
        }

    }

}
