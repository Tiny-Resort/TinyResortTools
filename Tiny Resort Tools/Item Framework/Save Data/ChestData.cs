using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyResort {

    [Serializable]
    internal class ChestData : ItemSaveData {

        public static List<ChestData> all = new List<ChestData>();
        public static List<ChestData> lostAndFound = new List<ChestData>();

        public static void LoadAll() {
            lostAndFound = (List<ChestData>)TRItems.Data.GetValue("ChestDataLostAndFound", new List<ChestData>());
            TRTools.Log($"Loading ChestData lostAndFound: {lostAndFound.Count}");
            
            all = (List<ChestData>)TRItems.Data.GetValue("ChestData", new List<ChestData>());
            TRTools.Log($"Loading ChestData: {all.Count}");
            foreach (var item in all) {
                try {
                    if (item.Load() == null) {
                        if (!lostAndFound.Contains(item)) { lostAndFound.Add(item); }
                    }
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
            }
        }

        public static void Save(ChestPlaceable chestPlaceable, int objectXPos, int objectYPos, int houseXPos, int houseYPos) {
            
            // Checks if the chest is in a house
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            
            // Gains access to the contents of the chest
            chestPlaceable.checkIfEmpty(objectXPos, objectYPos, houseDetails);

            // Goes through each slot in the chest, unloading and saving each custom item
            var chest = ContainerManager.manage.activeChests.First(p => p.xPos == objectXPos && p.yPos == objectYPos && p.inside == (houseDetails != null));

            for (var slotNo = 0; slotNo < chest.itemIds.Length; slotNo++) {
                if (TRItems.customItemsByItemID.ContainsKey(chest.itemIds[slotNo])) {
                    all.Add(new ChestData {
                        customItemID = TRItems.customItemsByItemID[chest.itemIds[slotNo]].customItemID, slotNo = slotNo, stackSize = chest.itemStacks[slotNo], 
                        objectXPos = objectXPos, objectYPos = objectYPos, houseXPos = houseXPos, houseYPos = houseYPos
                    });
                    ContainerManager.manage.changeSlotInChest(objectXPos, objectYPos, slotNo, -1, 0, houseDetails);
                }
            }
            
        }

        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            var tmpHouseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            ContainerManager.manage.changeSlotInChest(objectXPos, objectYPos, slotNo, customItem.invItem.getItemId(), stackSize, tmpHouseDetails);

            return customItem;
        }

    }

}
