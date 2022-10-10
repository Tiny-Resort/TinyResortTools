using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyResort {
    
    [Serializable]
    internal class BridgeData : ItemSaveData {

        public static List<BridgeData> all = new List<BridgeData>();
        public int bridgeLength;

        public static void LoadAll() {
            all = (List<BridgeData>)TRItems.Data.GetValue("BridgeData", new List<BridgeData>());
            TRTools.Log($"Loading BridgeData: {all.Count}");
            foreach (var item in all) { item.Load(); }
        }

        public static void Save(int tileObjectID, int objectXPos, int objectYPos, int rotation, int bridgeLength) {
            
            all.Add(new BridgeData {
                customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID, rotation = rotation, 
                bridgeLength = bridgeLength, objectXPos = objectXPos, objectYPos = objectYPos
            });
            
            TRItems.customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObject(objectXPos, objectYPos, rotation);
            WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = -1;
            WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = -1;
            WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
            NetworkNavMesh.nav.updateChunkInUse();

        }

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            customItem.tileObject.placeBridgeTiledObject(objectXPos, objectYPos, rotation, bridgeLength);
            WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
            WorldManager.manageWorld.unlockClientTile(objectXPos, objectYPos);
        }

    }

}
