/*using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(InventoryItem), "getItemId")]
    internal class getItemId {

        internal static bool runByAPI = false;
        
        public static bool Prefix(InventoryItem __instance) {
            if (runByAPI) {
                var cheatButton = typeof(InventoryItem).GetField("itemId", BindingFlags.Instance | BindingFlags.NonPublic);
                var itemId = (int?)cheatButton?.GetValue(__instance);
                TRItems.itemNeedsRepaired = false;
                if (itemId == -1) {
                    int num = Inventory.inv.itemIdBackUp(__instance);
                    if (num != -1) { TRItems.itemNeedsRepaired = true;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}*/