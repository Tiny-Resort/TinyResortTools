using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    internal class ItemChangerData : ItemSaveData {

        public static List<ItemChangerData> all = new List<ItemChangerData>();

        public int counterSeconds;
        public int counterDays;
        public int timePerCycles;
        public int cycles;
        public bool startedUnderground;
        
        public static void LoadAll() {
            all = (List<ItemChangerData>)TRItems.Data.GetValue("ItemChangerData", new List<ItemChangerData>());
            TRTools.Log($"Loading ItemChangerData: {all.Count}");
            foreach (var item in all) { item.Load(); }
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

            if (changer.houseX <= 0 && changer.houseY <= 0) { WorldManager.manageWorld.onTileStatusMap[changer.xPos, changer.yPos] = -2; }
            else { HouseManager.manage.getHouseInfo(changer.houseX, changer.houseY).houseMapOnTileStatus[changer.xPos, changer.yPos] = -2; }
            WorldManager.manageWorld.allChangers.Remove(changer);

        }
        
        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            if (houseXPos <= 0 && houseYPos <= 0) { WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = customItem.invItem.getItemId(); }
            else { HouseManager.manage.getHouseInfo(houseXPos, houseYPos).houseMapOnTileStatus[objectXPos, objectYPos] = customItem.invItem.getItemId();}

            CurrentChanger restoreChanger = new CurrentChanger(objectXPos, objectYPos);
            restoreChanger.cycles = cycles;
            restoreChanger.counterSeconds = counterSeconds;
            restoreChanger.counterDays = counterDays;
            restoreChanger.houseX = houseXPos;
            restoreChanger.houseY = houseYPos;
            restoreChanger.timePerCycles = timePerCycles;
            WorldManager.manageWorld.allChangers.Add(restoreChanger);
        }
    }
}
