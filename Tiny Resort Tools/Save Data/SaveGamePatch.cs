using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "SaveGame")]
    internal class SaveGamePatch {

        [HarmonyPrefix]
        internal static void patch(SaveLoad __instance) {
            TRData.preSaveEvent?.Invoke();
            TRData.cleanDataEvent?.Invoke();
        }

    }

}
