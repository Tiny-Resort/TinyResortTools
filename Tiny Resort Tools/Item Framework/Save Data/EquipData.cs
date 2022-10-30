using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    internal class EquipData : ItemSaveData {

        public static List<EquipData> all = new List<EquipData>();
        public static List<EquipData> lostAndFound = new List<EquipData>();

        internal enum EquipLocations { Hat, Face, Shirt, Pants, Shoes }
        public EquipLocations equipSlot;

        public static InventorySlot[] slots;

        public static void LoadAll() {
            slots = new[] { EquipWindow.equip.hatSlot, EquipWindow.equip.faceSlot, EquipWindow.equip.shirtSlot, EquipWindow.equip.pantsSlot, EquipWindow.equip.shoeSlot };

            lostAndFound = (List<EquipData>)TRItems.Data.GetValue("EquipDataLostAndFound", new List<EquipData>());
            //TRTools.Log($"Loading EquipData lostAndFound: {lostAndFound.Count}");
            
            all = (List<EquipData>)TRItems.Data.GetValue("EquipData", new List<EquipData>());
            //TRTools.Log($"Loading EquipData: {all.Count}");
            foreach (var item in all) {
                try {
                    if (item.Load() == null) {
                        if (!lostAndFound.Contains(item)) { lostAndFound.Add(item); }
                    }
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
            }
        }

        public static void Save(int stackSize, EquipLocations equipSlot) {
            all.Add(new EquipData { customItemID = TRItems.customItemsByItemID[slots[(int)equipSlot].itemNo].customItemID, stackSize = stackSize, equipSlot = equipSlot });
            slots[(int) equipSlot].updateSlotContentsAndRefresh(-1, 0);
        }

        // DEBUG: Method was changed significantly from pre-save refactoring
        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            slots[(int)equipSlot].updateSlotContentsAndRefresh(customItem.inventoryItem.getItemId(), stackSize);

            return customItem;
        }

    }

}
