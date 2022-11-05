using System;
using System.Collections.Generic;

namespace TinyResort; 

[Serializable]
internal class ObjectTopData : ItemSaveData {

    public static List<ObjectTopData> all = new();
    public static List<ObjectTopData> lostAndFound = new();
    public int onTopPos;
    public int status;

    public static void LoadAll() {
        lostAndFound = (List<ObjectTopData>)TRItems.Data.GetValue("ObjectTopDataLostAndFound", new List<ObjectTopData>());

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

    public static void Save(int tileObjectID, int ObjectXPos, int ObjectYPos, int rotation, int HouseXPos, int HouseYPos, int status, int onTopPos) {

        all.Add(
            new ObjectTopData {
                customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID,
                rotation = rotation,
                onTopPos = onTopPos,
                status = status,
                objectXPos = ObjectXPos,
                objectYPos = ObjectYPos,
                houseXPos = HouseXPos,
                houseYPos = HouseYPos
            }
        );

        // If it's in a house, remove it from the house
        if (HouseXPos > -1 && HouseYPos > -1 && HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos) != null) {
            ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, ObjectXPos, ObjectYPos, HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos)));
            var displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(HouseXPos, HouseYPos);
            if (displayPlayerHouseTiles && displayPlayerHouseTiles.tileObjectsInHouse[ObjectXPos, ObjectYPos]) displayPlayerHouseTiles.tileObjectsInHouse[ObjectXPos, ObjectYPos].checkOnTopInside(ObjectXPos, ObjectYPos, HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos));
        }

        // If it's not in a house, remove it from the overworld tiles
        else {
            ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, ObjectXPos, ObjectYPos, null));
            WorldManager.manageWorld.unlockClientTile(ObjectXPos, ObjectYPos);
            WorldManager.manageWorld.refreshAllChunksInUse(ObjectXPos, ObjectYPos);
        }

    }

    // TODO: we still need to test removing/restoring from a custom table with a custom item
    // TODO: we should add a condition of saving non modded items in situations where a non modded item is on top of a modded one
    public TRCustomItem Load() {
        if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
        var tmpHouseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
        ItemOnTopManager.manage.placeItemOnTop(customItem.tileObject.tileObjectId, onTopPos, status, rotation, objectXPos, objectYPos, tmpHouseDetails);
        WorldManager.manageWorld.unlockClientTile(objectXPos, objectYPos);
        WorldManager.manageWorld.refreshAllChunksInUse(objectXPos, objectYPos);

        // Must check if localChar is null because it will fail on first load since HouseManager doesn't exist
        if (NetworkMapSharer.share.localChar && tmpHouseDetails != null) {
            var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);
            house.refreshHouseTiles();
        }
        return customItem;
    }

}
