using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "SaveGame")]
    public class SaveGamePatch {

        [HarmonyPrefix]
        public static void patch(SaveLoad __instance) {
            TRTools.Log("Started Saving Game");
            TRData.preSaveEvent?.Invoke();
            TRData.cleanDataEvent?.Invoke();
        }

    }

}
