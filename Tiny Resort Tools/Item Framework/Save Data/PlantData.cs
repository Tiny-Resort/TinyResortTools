using System;
using System.Collections.Generic;

namespace TinyResort
{
    [Serializable]
    internal class PlantData : ItemSaveData {

        public static List<PlantData> all = new();
        public static List<PlantData> lostAndFound = new();

        public int growthStatus;

        public static void LoadAll() {
            lostAndFound = (List<PlantData>)TRItems.Data.GetValue("PlantDataLostAndFound", new List<PlantData>());

            //TRTools.Log($"Loading PlantData lostAndFound: {lostAndFound.Count}");

            all = (List<PlantData>)TRItems.Data.GetValue("PlantData", new List<PlantData>());

            //TRTools.Log($"Loading PlantData: {all.Count}");

            foreach (var item in all)
                try {
                    if (item.Load() == null)
                        if (!lostAndFound.Contains(item))
                            lostAndFound.Add(item);
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
        }

        public static void Save(int tileObjectID, int objectXPos, int objectYPos, int growthStatus) {
            all.Add(
                new PlantData {
                    customItemID = TRItems.customTileObjectByID[tileObjectID].customItemID, objectXPos = objectXPos,
                    objectYPos = objectYPos, growthStatus = growthStatus
                }
            );

            WorldManager.Instance.onTileMap[objectXPos, objectYPos] = -1;
            WorldManager.Instance.onTileStatusMap[objectXPos, objectYPos] = -1;
        }

        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;

            WorldManager.Instance.onTileMap[objectXPos, objectYPos] = customItem.tileObject.tileObjectId;
            WorldManager.Instance.onTileStatusMap[objectXPos, objectYPos] = growthStatus;
            WorldManager.Instance.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
            NetworkNavMesh.nav.updateChunkInUse();

            return customItem;
        }
    }
}
