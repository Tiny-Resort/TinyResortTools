using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    internal class HouseData : ItemSaveData {

        public static List<HouseData> all = new List<HouseData>();
        public bool isWall;

        public static void LoadAll() {
            all = (List<HouseData>)TRItems.Data.GetValue("HouseData", new List<HouseData>());
            TRTools.Log($"Loading HouseData: {all.Count}");
            foreach (var item in all) { item.Load(); }
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

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            var house = HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            if (isWall) { house.wall = customItem.invItem.getItemId(); } else { house.floor = customItem.invItem.getItemId(); }
        }
        
    }

}
