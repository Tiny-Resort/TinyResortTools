using System;
using System.Collections.Generic;

namespace TinyResort {
    
    [Serializable]
    internal class ObjectData : ItemSaveData {

        public static List<ObjectData> all = new List<ObjectData>();
        public static List<ObjectData> lostAndFound = new List<ObjectData>();

        public static void LoadAll() {
            lostAndFound = (List<ObjectData>)TRItems.Data.GetValue("ObjectDataLostAndFound", new List<ObjectData>());
            TRTools.Log($"Loading ObjectData lostAndFound: {lostAndFound.Count}");
            
            all = (List<ObjectData>)TRItems.Data.GetValue("ObjectData", new List<ObjectData>());
            TRTools.Log($"Loading ObjectData: {all.Count}");
            foreach (var item in all) {
                if (item.Load() == null) {
                    if (!lostAndFound.Contains(item)) lostAndFound.Add(item);
                }
            }
        }

        public static void Save(int tileObjectID, int objectXPos, int objectYPos, int rotation, int houseXPos, int houseYPos) {
            all.Add(new ObjectData {
                customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID, rotation = rotation, 
                objectXPos = objectXPos, objectYPos = objectYPos, houseXPos = houseXPos, houseYPos = houseYPos
            });

            var tileID = WorldManager.manageWorld.allObjects[tileObjectID].tileObjectChest ? 23 : -1;
            var tileStatus = WorldManager.manageWorld.allObjects[tileObjectID].tileObjectChest ? WorldManager.manageWorld.onTileStatusMap[objectXPos,objectYPos] : -1;

            // If the object is in a house, remove it from inside the house
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            if (houseDetails != null) {
                TRTools.Log("Found item in house");

                TRItems.customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObjectInside(objectXPos, objectYPos, rotation, houseDetails);
                houseDetails.houseMapOnTile[objectXPos, objectYPos] = tileID;
                houseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = tileStatus;
                
                // Refreshes the house so that the object actually appears

                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
                if (house) {
                    if (house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture) house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture.updateOnTileStatus(objectXPos, objectYPos, houseDetails);
                    house.refreshHouseTiles();
                }

            }
            
            // If the object is not in a house, remove it from the overworld
            else {
                TRItems.customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObject(objectXPos, objectYPos, rotation);
                WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = tileID;
                WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = tileStatus;
                WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
                NetworkNavMesh.nav.updateChunkInUse();
            }

        }

        // TODO: Maybe remove particle/sound effects from objects outside
        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
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
            return customItem;
        }

    }

}
