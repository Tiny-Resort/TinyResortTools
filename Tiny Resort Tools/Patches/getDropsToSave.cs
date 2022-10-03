using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(WorldManager), "getDropsToSave")]
    internal class getDropsToSave {

        public static void Prefix(WorldManager __instance) {
            if (!TRItems._droppedItemsEnabled) {
                foreach (var item in WorldManager.manageWorld.itemsOnGround) {
                    if (TRItems.customItemsByID.ContainsKey(item.myItemId)) {
                        TRTools.Log($"Setting Modded Item to not save {item.myItemId}");
                        item.saveDrop = false;
                    }
                }
            }
        }
        
    }
}