using System;
using System.Collections.Generic;

namespace TinyResort; 

[Serializable]
internal class BridgeData : ItemSaveData {

    public static List<BridgeData> all = new();
    public static List<BridgeData> lostAndFound = new();

    public int bridgeLength;

    public static void LoadAll() {
        lostAndFound = (List<BridgeData>)TRItems.Data.GetValue("BridgeDataLostAndFound", new List<BridgeData>());

        //TRTools.Log($"Loading BridgeData lostAndFound: {lostAndFound.Count}");

        all = (List<BridgeData>)TRItems.Data.GetValue("BridgeData", new List<BridgeData>());

        //TRTools.Log($"Loading BridgeData: {all.Count}");

        foreach (var item in all)
            try {
                if (item.Load() == null)
                    if (!lostAndFound.Contains(item))
                        lostAndFound.Add(item);
            }
            catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
    }

    public static void Save(int tileObjectID, int objectXPos, int objectYPos, int rotation, int bridgeLength) {
        all.Add(
            new BridgeData {
                customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID,
                rotation = rotation,
                bridgeLength = bridgeLength,
                objectXPos = objectXPos,
                objectYPos = objectYPos
            }
        );

        TRItems.customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObject(objectXPos, objectYPos, rotation);
        WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = -1;
        WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = -1;
        WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
        NetworkNavMesh.nav.updateChunkInUse();

    }

    public TRCustomItem Load() {
        if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
        customItem.tileObject.placeBridgeTiledObject(objectXPos, objectYPos, rotation, bridgeLength);
        WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
        WorldManager.manageWorld.unlockClientTile(objectXPos, objectYPos);
        return customItem;
    }
}
