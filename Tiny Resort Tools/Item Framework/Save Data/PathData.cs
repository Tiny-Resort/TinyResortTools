using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    internal class PathData : ItemSaveData {

        public static List<PathData> all = new List<PathData>();

        public static void LoadAll() {
            all = (List<PathData>)TRItems.Data.GetValue("PathData", new List<PathData>());
            TRTools.Log($"Loading PathData: {all.Count}");
            foreach (var item in all) { item.Load(); }
        }

        public static void Save(int tileType, int objectXPos, int objectYPos) {
           all.Add(new PathData { customItemID = TRItems.customTileTypeByID[tileType].customItemID, objectXPos = objectXPos, objectYPos = objectYPos });
           WorldManager.manageWorld.tileTypeMap[objectXPos, objectYPos] = 0;
           WorldManager.manageWorld.refreshAllChunksInUse(objectXPos, objectYPos);

        }

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return; 
            WorldManager.manageWorld.tileTypeMap[objectXPos, objectYPos] = customItem.invItem.placeableTileType;
            WorldManager.manageWorld.refreshAllChunksInUse(objectXPos, objectYPos);
        }

    }

}
