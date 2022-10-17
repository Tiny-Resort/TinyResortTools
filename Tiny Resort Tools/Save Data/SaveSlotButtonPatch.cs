using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveSlotButton), "onPress")]
    internal class SaveSlotButtonPatch {
        
        [HarmonyPrefix]
        internal static void Prefix(SaveLoad __instance) {
            TRTools.Log($"Running onPress");
            TRData.preLoadEvent?.Invoke();
        }

    }

}
