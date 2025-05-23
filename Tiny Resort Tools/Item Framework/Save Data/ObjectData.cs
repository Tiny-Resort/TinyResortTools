using System;
using System.Collections.Generic;

namespace TinyResort
{
    [Serializable]
    internal class ObjectData : ItemSaveData {

        public static List<ObjectData> all = new();
        public static List<ObjectData> lostAndFound = new();

        public static void LoadAll() {
            lostAndFound = (List<ObjectData>)TRItems.Data.GetValue("ObjectDataLostAndFound", new List<ObjectData>());

            //TRTools.Log($"Loading ObjectData lostAndFound: {lostAndFound.Count}");

            all = (List<ObjectData>)TRItems.Data.GetValue("ObjectData", new List<ObjectData>());

            //TRTools.Log($"Loading ObjectData: {all.Count}");
            foreach (var item in all)
                try {
                    if (item.Load() == null)
                        if (!lostAndFound.Contains(item))
                            lostAndFound.Add(item);
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
        }

        public static void Save(
            int tileObjectID, int objectXPos, int objectYPos, int rotation, int houseXPos, int houseYPos
        ) {

            all.Add(
                new ObjectData {
                    customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID, rotation = rotation,
                    objectXPos = objectXPos, objectYPos = objectYPos, houseXPos = houseXPos, houseYPos = houseYPos
                }
            );

            var tileID = WorldManager.Instance.allObjects[tileObjectID].tileObjectChest ? 23 : -1;
            var tileStatus = WorldManager.Instance.allObjects[tileObjectID].tileObjectChest
                ? WorldManager.Instance.onTileStatusMap[objectXPos, objectYPos]
                : -1;

            if (WorldManager.Instance.allObjects[tileObjectID].tileObjectConnect)
                tileID = WorldManager.Instance.allObjects[tileObjectID].tileObjectConnect.isFence ? 323 : tileID;

            // If the object is in a house, remove it from inside the house
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            if (houseDetails != null) {

                TRItems.customTileObjectByID[tileObjectID]
                    .tileObject.removeMultiTiledObjectInside(objectXPos, objectYPos, rotation, houseDetails);
                houseDetails.houseMapOnTile[objectXPos, objectYPos] = tileID;
                houseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = tileStatus;

                // Refreshes the house so that the object actually appears

                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
                if (house) {
                    if (house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture)
                        house.tileObjectsInHouse[objectXPos, objectYPos]
                            .tileObjectFurniture.updateOnTileStatus(objectXPos, objectYPos, houseDetails);
                    house.refreshHouseTiles();
                }

            }

            // If the object is not in a house, remove it from the overworld
            else {
                TRItems.customTileObjectByID[tileObjectID]
                    .tileObject.removeMultiTiledObject(objectXPos, objectYPos, rotation);
                WorldManager.Instance.onTileMap[objectXPos, objectYPos] = tileID;
                WorldManager.Instance.onTileStatusMap[objectXPos, objectYPos] = tileStatus;
                WorldManager.Instance.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
                NetworkNavMesh.nav.updateChunkInUse();
            }

        }

        // TODO: Maybe remove particle/sound effects from objects outside
        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            var tmpHouseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            if (tmpHouseDetails != null) {
                customItem.tileObject.PlaceMultiTiledObjectInside(objectXPos, objectYPos, rotation, tmpHouseDetails);

                tmpHouseDetails.houseMapOnTile[objectXPos, objectYPos] = customItem.tileObject.tileObjectId;
                tmpHouseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = 0;

                // Must check if localChar is null because it will fail on first load since HouseManager doesn't exist
                //if (NetworkMapSharer.Instance.localChar) {
                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
                house.refreshHouseTiles();

                //}

            }
            else {
                customItem.tileObject.placeMultiTiledObject(objectXPos, objectYPos, rotation);
                WorldManager.Instance.onTileMap[objectXPos, objectYPos] = customItem.tileObject.tileObjectId;
                WorldManager.Instance.onTileStatusMap[objectXPos, objectYPos] = 0;
                WorldManager.Instance.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);

                //NetworkNavMesh.nav.updateChunkInUse();
                WorldManager.Instance.unlockClientTile(objectXPos, objectYPos);
            }
            return customItem;
        }
    }
}
