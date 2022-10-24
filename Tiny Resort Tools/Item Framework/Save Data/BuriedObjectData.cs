using System;
using System.Collections.Generic;

namespace TinyResort {
    
    [Serializable]
    internal class BuriedObjectData : ItemSaveData {

        public static List<BuriedObjectData> all = new List<BuriedObjectData>();
        public static List<BuriedObjectData> lostAndFound = new List<BuriedObjectData>();
        
        public static void LoadAll() {
            lostAndFound = (List<BuriedObjectData>)TRItems.Data.GetValue("BuriedObjectDataLostAndFound", new List<BuriedObjectData>());
            TRTools.Log($"Loading BuriedObjectData lostAndFound: {lostAndFound.Count}");
            
            all = (List<BuriedObjectData>)TRItems.Data.GetValue("BuriedObjectData", new List<BuriedObjectData>());
            TRTools.Log($"Loading BuriedObjectData: {all.Count}");
            foreach (var item in all) {
                try {
                    if (item.Load() == null) {
                        if (!lostAndFound.Contains(item)) { lostAndFound.Add(item); }
                    }
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
            }
        }

        // Buried items are always tileObjectID 30, so we set it to -1 to make it an empty tile
        public static void Save(BuriedItem toRemove) {
            all.Add(new BuriedObjectData {
                customItemID = TRItems.customItemsByItemID[toRemove.itemId].customItemID, stackSize = toRemove.stackedAmount, 
                objectXPos = toRemove.xPos, objectYPos = toRemove.yPos
            });
            BuriedManager.manage.allBuriedItems.Remove(toRemove);
            WorldManager.manageWorld.onTileMap[toRemove.xPos, toRemove.yPos] = -1;
        }

        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = 30;
            BuriedManager.manage.allBuriedItems.Add(new BuriedItem(customItem.inventoryItem.getItemId(), stackSize, objectXPos, objectYPos));

            return customItem;
        }

    }

}
