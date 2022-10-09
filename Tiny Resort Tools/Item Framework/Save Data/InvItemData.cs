using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    internal class InvItemData : ItemSaveData {

        public static List<InvItemData> all = new List<InvItemData>();

        public static void LoadAll() {
            all = (List<InvItemData>)TRItems.Data.GetValue("InvItemData", new List<InvItemData>());
            foreach (var item in all) { item.Load(); }
        }

        public static void Save(int slotNo, int stackSize) {
            all.Add(new InvItemData {
                customItemID = TRItems.customItemsByItemID[Inventory.inv.invSlots[slotNo].itemNo].customItemID, 
                slotNo = slotNo, stackSize = stackSize
            });
            Inventory.inv.invSlots[slotNo].updateSlotContentsAndRefresh(-1, 0);
        }

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            Inventory.inv.invSlots[slotNo].updateSlotContentsAndRefresh(customItem.invItem.getItemId(), stackSize);
        }

    }

}
