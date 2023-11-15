using System;
using System.Collections.Generic;

namespace TinyResort {
    [Serializable]
    internal class PathData : ItemSaveData {

        public static List<PathData> all = new();
        public static List<PathData> lostAndFound = new();

        public static void LoadAll() {
            lostAndFound = (List<PathData>)TRItems.Data.GetValue("PathDataLostAndFound", new List<PathData>());

            //TRTools.Log($"Loading PathData lostAndFound: {lostAndFound.Count}");

            all = (List<PathData>)TRItems.Data.GetValue("PathData", new List<PathData>());

            //TRTools.Log($"Loading PathData: {all.Count}");
            foreach (var item in all)
                try {
                    if (item.Load() == null)
                        if (!lostAndFound.Contains(item))
                            lostAndFound.Add(item);
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
        }

        public static void Save(int tileType, int objectXPos, int objectYPos) {
            all.Add(
                new PathData {
                    customItemID = TRItems.customTileTypeByID[tileType].customItemID, objectXPos = objectXPos,
                    objectYPos = objectYPos
                }
            );
            WorldManager.Instance.tileTypeMap[objectXPos, objectYPos] = 0;
            WorldManager.Instance.refreshAllChunksInUse(objectXPos, objectYPos);

        }

        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            WorldManager.Instance.tileTypeMap[objectXPos, objectYPos] = customItem.inventoryItem.placeableTileType;
            WorldManager.Instance.refreshAllChunksInUse(objectXPos, objectYPos);
            return customItem;
        }
    }
}
