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
        
        public static void Save(int itemID, CurrentChanger changer) {
            all.Add(new ItemChangerData { customItemID = TRItems.customItemsByItemID[itemID].customItemID, 
                        objectXPos = changer.xPos, objectYPos = changer.yPos, counterSeconds = changer.counterSeconds, counterDays = changer.counterDays,
                        timePerCycles = changer.timePerCycles, cycles = changer.cycles, startedUnderground = changer.startedUnderground, 
                        houseXPos = changer.houseX, houseYPos = changer.houseY
                    });

            if (changer.houseX <= 0 && changer.houseY <= 0) {
                WorldManager.manageWorld.onTileStatusMap[changer.xPos, changer.yPos] = -2;
            }
            else { HouseManager.manage.getHouseInfo(changer.houseX, changer.houseY).houseMapOnTileStatus[changer.xPos, changer.yPos] = -2; }
            WorldManager.manageWorld.allChangers.Remove(changer);

        }


        public static void LoadAll() {
            all = (List<ItemChangerData>)TRItems.Data.GetValue("ItemChangerData", new List<ItemChangerData>());
            TRTools.Log($"Loading ItemChangerData: {all.Count}");
            foreach (var item in all) { item.Load(); }
        }

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            TRTools.Log($"Before House Check: {houseXPos}, {houseYPos}");
            if (houseXPos <= 0 && houseYPos <= 0) {
                TRTools.Log($"Inside outside Check");

                WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = customItem.invItem.getItemId();
            }
            else {
                TRTools.Log($"Before House restore");

                HouseManager.manage.getHouseInfo(houseXPos, houseYPos).houseMapOnTileStatus[objectXPos, objectYPos] = customItem.invItem.getItemId();
            }

            CurrentChanger restoreChanger = new CurrentChanger(objectXPos, objectYPos);
            restoreChanger.cycles = cycles;
            restoreChanger.counterSeconds = counterSeconds;
            restoreChanger.counterDays = counterDays;
            restoreChanger.houseX = houseXPos;
            restoreChanger.houseY = houseYPos;
            restoreChanger.timePerCycles = timePerCycles;
            WorldManager.manageWorld.allChangers.Add(restoreChanger);
        }
        /*
        public static void Save(int tileType, int objectXPos, int objectYPos) {
           all.Add(new ItemChangerData { customItemID = TRItems.customTileTypeByID[tileType].customItemID, objectXPos = objectXPos, objectYPos = objectYPos });
           WorldManager.manageWorld.tileTypeMap[objectXPos, objectYPos] = 0;
           WorldManager.manageWorld.refreshAllChunksInUse(objectXPos, objectYPos);

        }

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return; 
            WorldManager.manageWorld.tileTypeMap[objectXPos, objectYPos] = customItem.invItem.placeableTileType;
            WorldManager.manageWorld.refreshAllChunksInUse(objectXPos, objectYPos);
        }*/

    }

}
