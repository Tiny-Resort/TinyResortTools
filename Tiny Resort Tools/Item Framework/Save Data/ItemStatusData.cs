using System;
using System.Collections.Generic;

namespace TinyResort; 

[Serializable]
internal class ItemStatusData : ItemSaveData {

    public static List<ItemStatusData> all = new();
    public static List<ItemStatusData> lostAndFound = new();

    public static void LoadAll() {
        lostAndFound = (List<ItemStatusData>)TRItems.Data.GetValue("ItemStatusDataLostAndFound", new List<ItemStatusData>());

        //TRTools.Log($"Loading ItemStatusData lostAndFound: {lostAndFound.Count}");

        all = (List<ItemStatusData>)TRItems.Data.GetValue("ItemStatusData", new List<ItemStatusData>());

        //TRTools.Log($"Loading ItemStatusData: {all.Count}");

        foreach (var item in all)
            try {
                if (item.Load() == null)
                    if (!lostAndFound.Contains(item))
                        lostAndFound.Add(item);
            }
            catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
    }

    public static void Save(int itemID, int objectXPos, int objectYPos, int houseXPos, int houseYPos) {
        all.Add(
            new ItemStatusData {
                customItemID = TRItems.customItemsByItemID[itemID].customItemID,
                objectXPos = objectXPos,
                objectYPos = objectYPos,
                houseXPos = houseXPos,
                houseYPos = houseYPos
            }
        );
        var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);

        if (houseDetails != null)
            houseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = -1;
        else
            WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = -1;
    }

    public TRCustomItem Load() {
        if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
        var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);

        if (houseDetails != null) { houseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = customItem.inventoryItem.getItemId(); }
        else {
            WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = customItem.inventoryItem.getItemId();
            WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
            NetworkNavMesh.nav.updateChunkInUse();

            //WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[objectXPos, objectYPos]].showObjectOnStatusChange.showGameObject(customItem.invItem.getItemId());
        }

        return customItem;
    }

}
