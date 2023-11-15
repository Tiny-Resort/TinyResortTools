using System;
using System.Collections.Generic;

namespace TinyResort {
    [Serializable]
    internal class ObjectTopData : ItemSaveData {

        public static List<ObjectTopData> all = new();
        public static List<ObjectTopData> lostAndFound = new();
        public int onTopPos;
        public int status;
        public int mannequinID;

        public static void LoadAll() {
            lostAndFound = (List<ObjectTopData>)TRItems.Data.GetValue(
                "ObjectTopDataLostAndFound", new List<ObjectTopData>()
            );

            //TRTools.Log($"Loading ObjectTopData lostAndFound: {lostAndFound.Count}");

            all = (List<ObjectTopData>)TRItems.Data.GetValue("ObjectTopData", new List<ObjectTopData>());

            //TRTools.Log($"Loading ObjectTopData: {all.Count}");
            foreach (var item in all)
                try {
                    if (item.Load() == null)
                        if (!lostAndFound.Contains(item))
                            lostAndFound.Add(item);
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
        }

        public static void Save(
            int tileObjectID, int ObjectXPos, int ObjectYPos, int rotation, int HouseXPos, int HouseYPos, int status,
            int onTopPos
        ) {

            if (TRItems.customTileTypeByID.ContainsKey(tileObjectID))
                all.Add(
                    new ObjectTopData {
                        customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID, rotation = rotation,
                        onTopPos = onTopPos, status = status, objectXPos = ObjectXPos, objectYPos = ObjectYPos,
                        houseXPos = HouseXPos, houseYPos = HouseYPos, mannequinID = -1
                    }
                );
            else if (TRItems.customItemsByItemID.ContainsKey(status))
                all.Add(
                    new ObjectTopData {
                        customItemID = TRItems.customItemsByItemID[status].customItemID, rotation = rotation,
                        onTopPos = onTopPos, status = status, objectXPos = ObjectXPos, objectYPos = ObjectYPos,
                        houseXPos = HouseXPos, houseYPos = HouseYPos, mannequinID = tileObjectID
                    }
                );

            // If it's in a house, remove it from the house
            if (HouseXPos > -1 && HouseYPos > -1 && HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos) != null) {
                ItemOnTopManager.manage.removeItemOnTop(
                    ItemOnTopManager.manage.getItemOnTopInPosition(
                        onTopPos, ObjectXPos, ObjectYPos, HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos)
                    )
                );
                var displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(HouseXPos, HouseYPos);
                if (displayPlayerHouseTiles && displayPlayerHouseTiles.tileObjectsInHouse[ObjectXPos, ObjectYPos])
                    displayPlayerHouseTiles.tileObjectsInHouse[ObjectXPos, ObjectYPos]
                        .checkOnTopInside(
                            ObjectXPos, ObjectYPos,
                            HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos)
                        );
            }

            // If it's not in a house, remove it from the overworld tiles
            else {
                ItemOnTopManager.manage.removeItemOnTop(
                    ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, ObjectXPos, ObjectYPos, null)
                );
                WorldManager.Instance.unlockClientTile(ObjectXPos, ObjectYPos);
                WorldManager.Instance.refreshAllChunksInUse(ObjectXPos, ObjectYPos);
            }

        }

        // TODO: we still need to test removing/restoring from a custom table with a custom item
        // TODO: we should add a condition of saving non modded items in situations where a non modded item is on top of a modded one
        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;

            // If mannequin is not -1, then the custom item is "inside" another item (like a mannequin for clothing)
            var itemID = mannequinID == -1 ? customItem.tileObject.tileObjectId : mannequinID;
            var tmpHouseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            ItemOnTopManager.manage.placeItemOnTop(
                itemID, onTopPos, status, rotation, objectXPos, objectYPos, tmpHouseDetails
            );
            WorldManager.Instance.unlockClientTile(objectXPos, objectYPos);
            WorldManager.Instance.refreshAllChunksInUse(objectXPos, objectYPos);

            // Must check if localChar is null because it will fail on first load since HouseManager doesn't exist
            if (NetworkMapSharer.Instance.localChar && tmpHouseDetails != null) {
                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
                house.refreshHouseTiles();
            }
            return customItem;
        }
    }
}
