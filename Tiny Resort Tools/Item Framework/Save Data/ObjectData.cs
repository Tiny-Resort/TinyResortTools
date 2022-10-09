using System;
using System.Collections.Generic;

namespace TinyResort {
    
    [Serializable]
    internal class ObjectData : ItemSaveData {

        public static List<ObjectData> all = new List<ObjectData>();

        public static void LoadAll() {
            all = (List<ObjectData>)TRItems.Data.GetValue("ObjectData", new List<ObjectData>());
            foreach (var item in all) { item.Load(); }
        }

        public static void Save(int tileObjectID, int objectXPos, int objectYPos, int rotation, int houseXPos, int houseYPos) {
            
            all.Add(new ObjectData {
                customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID, rotation = rotation, 
                objectXPos = objectXPos, objectYPos = objectYPos, houseXPos = houseXPos, houseYPos = houseYPos
            });
            
            // If the object is in a house, remove it from inside the house
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            if (houseDetails != null) {
                TRItems.customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObjectInside(objectXPos, objectYPos, rotation, houseDetails);
                houseDetails.houseMapOnTile[objectXPos, objectYPos] = -1;
                houseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = -1;
                
                // Refreshes the house so that the object actually appears
                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
                if (house && house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture) {
                    house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture.updateOnTileStatus(objectXPos, objectYPos, houseDetails);
                    house.refreshHouseTiles();
                }

            }
            
            // If the object is not in a house, remove it from the overworld
            else {
                TRItems.customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObject(objectXPos, objectYPos, rotation);
                WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = -1;
                WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = -1;
                WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
                NetworkNavMesh.nav.updateChunkInUse();
            }

        }

        // TODO: Maybe remove particle/sound effects from objects outside
        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            var tmpHouseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            if (tmpHouseDetails != null) {
                customItem.tileObject.placeMultiTiledObjectInside(objectXPos, objectYPos, rotation, tmpHouseDetails);

                tmpHouseDetails.houseMapOnTile[objectXPos, objectYPos] = customItem.tileObject.tileObjectId;
                tmpHouseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = 0;

                // Must check if localChar is null because it will fail on first load since HouseManager doesn't exist
                //if (NetworkMapSharer.share.localChar) {
                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
                house.refreshHouseTiles();

                //}

            }
            else {
                customItem.tileObject.placeMultiTiledObject(objectXPos, objectYPos, rotation);
                WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = customItem.tileObject.tileObjectId;
                WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = 0;
                WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);

                //NetworkNavMesh.nav.updateChunkInUse();
                WorldManager.manageWorld.unlockClientTile(objectXPos, objectYPos);
            }
        }

    }

}
