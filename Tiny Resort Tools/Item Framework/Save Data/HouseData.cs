using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    internal class HouseData : ItemSaveData {

        public static List<HouseData> all = new List<HouseData>();
        public static List<HouseData> lostAndFound = new List<HouseData>();
        public bool isWall;

        public static void LoadAll() {
            lostAndFound = (List<HouseData>)TRItems.Data.GetValue("HouseDataLostAndFound", new List<HouseData>());
            //TRTools.Log($"Loading HouseData lostAndFound: {lostAndFound.Count}");
            
            all = (List<HouseData>)TRItems.Data.GetValue("HouseData", new List<HouseData>());
            //TRTools.Log($"Loading HouseData: {all.Count}");
            foreach (var item in all) {
                try {
                    if (item.Load() == null) {
                        if (!lostAndFound.Contains(item)) { lostAndFound.Add(item); }
                    }
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
            }
        }

        public static void Save(HouseDetails house, bool isWall) {
            all.Add(new HouseData() {
                customItemID = TRItems.customItemsByItemID[isWall ? house.wall : house.floor].customItemID, 
                houseXPos = house.xPos, 
                houseYPos = house.yPos,
                isWall = isWall
            });
            if (isWall) { house.wall = 550; } else { house.floor = 546; }
        }

        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            var house = HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            if (isWall) { house.wall = customItem.inventoryItem.getItemId(); } else { house.floor = customItem.inventoryItem.getItemId(); }
            
            return customItem;
        }
        
    }

}
