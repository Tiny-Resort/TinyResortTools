using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "SaveGame")]
    internal class SaveGamePatch {

        [HarmonyPrefix]
        internal static void Prefix(SaveLoad __instance) {
            TRTools.Log($"Running SaveGame");
            TRData.preSaveEvent?.Invoke();
            TRData.cleanDataEvent?.Invoke();
        }
        
    }

}
