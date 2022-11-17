using HarmonyLib;

namespace TinyResort; 

[HarmonyPatch(typeof(WorldManager), "getDropsToSave")]
internal class getDropsToSave {

    public static void Prefix(WorldManager __instance) {
        foreach (var item in WorldManager.manageWorld.itemsOnGround)
            if (TRItems.customItemsByItemID.ContainsKey(item.myItemId))

                //TRTools.Log($"Setting Modded Item to not save {item.myItemId}");
                item.saveDrop = false;
    }
}
