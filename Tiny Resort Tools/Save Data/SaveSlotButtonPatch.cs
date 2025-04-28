using HarmonyLib;

namespace TinyResort
{
    // This is run anytime a save slot is clicked. It will happened before any loading is started. 

    [HarmonyPatch(typeof(SaveSlotButton), "onPress")]
    internal class SaveSlotButtonPatch {

        [HarmonyPrefix]
        internal static void Prefix(SaveLoad __instance) {

            //TRTools.Log($"Running onPress");
            TRData.preLoadEvent?.Invoke(); 
            // TRTools.LogError("INSIDE: SaveSlotButtonPatch");
        }

    }
}
