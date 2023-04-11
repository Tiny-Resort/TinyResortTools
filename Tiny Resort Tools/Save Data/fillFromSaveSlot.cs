using HarmonyLib;

namespace TinyResort;

[HarmonyPatch(typeof(SaveSlotButton), nameof(SaveSlotButton.fillFromSaveSlot))]
internal class FillFromSaveSlotPatch {
    internal static void Postfix(SaveSlotButton __instance) => __instance.islandName.text += $"\n<size=75%>(Slot #{__instance.slotNoToLoad})";
}
