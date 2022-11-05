using System;
using System.Collections.Generic;

namespace TinyResort; 

[Serializable]
internal class ItemChangerData : ItemSaveData {

    public static List<ItemChangerData> all = new();
    public static List<ItemChangerData> lostAndFound = new();

    public int counterSeconds;
    public int counterDays;
    public int timePerCycles;
    public int cycles;
    public bool startedUnderground;

    public static void LoadAll() {
        lostAndFound = (List<ItemChangerData>)TRItems.Data.GetValue("ItemChangerDataLostAndFound", new List<ItemChangerData>());

        //TRTools.Log($"Loading ItemChangerData lostAndFound: {lostAndFound.Count}");

        all = (List<ItemChangerData>)TRItems.Data.GetValue("ItemChangerData", new List<ItemChangerData>());

        //TRTools.Log($"Loading ItemChangerData: {all.Count}");
        foreach (var item in all)
            try {
                if (item.Load() == null)
                    if (!lostAndFound.Contains(item))
                        lostAndFound.Add(item);
            }
            catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
    }

    public static void Save(int itemID, CurrentChanger changer) {
        all.Add(
            new ItemChangerData {
                customItemID = TRItems.customItemsByItemID[itemID].customItemID,
                objectXPos = changer.xPos,
                objectYPos = changer.yPos,
                counterSeconds = changer.counterSeconds,
                counterDays = changer.counterDays,
                timePerCycles = changer.timePerCycles,
                cycles = changer.cycles,
                startedUnderground = changer.startedUnderground,
                houseXPos = changer.houseX,
                houseYPos = changer.houseY
            }
        );

        if (changer.houseX <= 0 && changer.houseY <= 0)
            WorldManager.manageWorld.onTileStatusMap[changer.xPos, changer.yPos] = -2;
        else
            HouseManager.manage.getHouseInfo(changer.houseX, changer.houseY).houseMapOnTileStatus[changer.xPos, changer.yPos] = -2;
        WorldManager.manageWorld.allChangers.Remove(changer);

    }

    public TRCustomItem Load() {
        if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;

        if (houseXPos <= 0 && houseYPos <= 0)
            WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = customItem.inventoryItem.getItemId();
        else
            HouseManager.manage.getHouseInfo(houseXPos, houseYPos).houseMapOnTileStatus[objectXPos, objectYPos] = customItem.inventoryItem.getItemId();

        var restoreChanger = new CurrentChanger(objectXPos, objectYPos);
        restoreChanger.cycles = cycles;
        restoreChanger.counterSeconds = counterSeconds;
        restoreChanger.counterDays = counterDays;
        restoreChanger.houseX = houseXPos;
        restoreChanger.houseY = houseYPos;
        restoreChanger.timePerCycles = timePerCycles;

        TRTools.Log($"Cyles: {cycles} | Seconds: {counterSeconds} | Days: {counterDays} | House: ({houseXPos}, {houseYPos}) | Cycle Time: {timePerCycles}");

        //WorldManager.manageWorld.allChangers.Add(restoreChanger);
        WorldManager.manageWorld.loadCountDownForTile(restoreChanger);
        return customItem;
    }
}
