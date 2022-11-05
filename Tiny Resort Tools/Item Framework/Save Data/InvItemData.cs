using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyResort; 

[Serializable]
internal class InvItemData : ItemSaveData {

    public static List<InvItemData> all;
    public static List<InvItemData> lostAndFound = new();

    public static void LoadAll() {
        lostAndFound = (List<InvItemData>)TRItems.Data.GetValue("InvItemDataLostAndFound", new List<InvItemData>());

        //TRTools.Log($"Loading InvItemData lostAndFound: {lostAndFound.Count}");

        all = (List<InvItemData>)TRItems.Data.GetValue("InvItemData", new List<InvItemData>());

        //TRTools.Log($"Loading InvItemData: {all.Count}");
        foreach (var item in all)
            try {
                if (item.Load() == null)
                    if (!lostAndFound.Contains(item))
                        lostAndFound.Add(item);
            }
            catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
    }

    public static void Save(int slotNo, int stackSize) {
        all.Add(new InvItemData { customItemID = TRItems.customItemsByItemID[Inventory.inv.invSlots[slotNo].itemNo].customItemID, slotNo = slotNo, stackSize = stackSize });
        Inventory.inv.invSlots[slotNo].updateSlotContentsAndRefresh(-1, 0);
    }

    public TRCustomItem Load() {
        if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
        Inventory.inv.invSlots[slotNo].updateSlotContentsAndRefresh(customItem.inventoryItem.getItemId(), stackSize);

        return customItem;
    }

}
