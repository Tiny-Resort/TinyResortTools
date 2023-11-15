using HarmonyLib;

namespace TinyResort
{
    // This is run before the SaveGame method (first save method and before the coroutine) is started. 

    [HarmonyPatch(typeof(SaveAndLoad), "SaveGame")]
    internal class SaveGamePatch {

        [HarmonyPrefix]
        internal static void Prefix(SaveAndLoad __instance) {
            TRData.preSaveEvent?.Invoke();
            TRData.cleanDataEvent?.Invoke();
        }
    }
}
