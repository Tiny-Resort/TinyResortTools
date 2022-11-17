using HarmonyLib;

namespace TinyResort; 

// This is run before the SaveGame method (first save method and before the coroutine) is started. 

[HarmonyPatch(typeof(SaveLoad), "SaveGame")]
internal class SaveGamePatch {

    [HarmonyPrefix]
    internal static void Prefix(SaveLoad __instance) {
        TRData.preSaveEvent?.Invoke();
        TRData.cleanDataEvent?.Invoke();
    }
}
