using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyResort {

    [Serializable]
    internal class ChestData : ItemSaveData {

        public static List<ChestData> all = new List<ChestData>();

        public static void LoadAll() {
            all = (List<ChestData>)TRItems.Data.GetValue("ChestData", new List<ChestData>());
            foreach (var item in all) { item.Load(); }
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

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            var tmpHouseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            ContainerManager.manage.changeSlotInChest(objectXPos, objectYPos, slotNo, customItem.invItem.getItemId(), stackSize, tmpHouseDetails);
        }

    }

}
