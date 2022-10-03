using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(DropSaves), "loadDrops")]
    internal class loadDrops {
        
        [HarmonyPostfix]
        public static void Postfix() {

            if (TRItems._droppedItemsEnabled) {
                TRItems.savedDroppedItems = (List<ItemSaveData>)TRItems.Data.GetValue("DroppedItemList", new List<ItemSaveData>());
                if (TRItems.savedDroppedItems.Count > 0) {
                    foreach (var item in TRItems.savedDroppedItems) {
                        if (!TRItems.customItems.TryGetValue(item.uniqueID, out var customItem)) continue;
                        TRTools.Log($"Restorting items");
                        item.droppedItem.Restore();
                    }
                }
            }
        }
    }
}
