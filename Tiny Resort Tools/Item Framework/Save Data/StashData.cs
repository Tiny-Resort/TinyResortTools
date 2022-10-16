using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    internal class StashData : ItemSaveData {

        public static List<StashData> all = new List<StashData>();
        public static List<StashData> lostAndFound = new List<StashData>();
        public int stashPostition;

        public static void LoadAll() {
            lostAndFound = (List<StashData>)TRItems.Data.GetValue("StashDataLostAndFound", new List<StashData>());
            TRTools.Log($"Loading StashData lostAndFound: {lostAndFound.Count}");
            
            all = (List<StashData>)TRItems.Data.GetValue("StashData", new List<StashData>());
            TRTools.Log($"Loading StashData: {all.Count}");
            foreach (var item in all) {
                if (item.Load() == null) {
                    if (!lostAndFound.Contains(item)) lostAndFound.Add(item);
                }
            }
        }

        public static void Save(int stackSize, int stashPostition, int slotNo) {
            all.Add(new StashData {
                customItemID = TRItems.customItemsByItemID[ContainerManager.manage.privateStashes[stashPostition].itemIds[slotNo]].customItemID, 
                stackSize = stackSize, stashPostition = stashPostition, slotNo = slotNo
            });
            ContainerManager.manage.privateStashes[stashPostition].itemIds[slotNo] = -1;
            ContainerManager.manage.privateStashes[stashPostition].itemStacks[slotNo] = 0;
        }

        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            ContainerManager.manage.privateStashes[stashPostition].itemIds[slotNo] = customItem.invItem.getItemId();
            ContainerManager.manage.privateStashes[stashPostition].itemStacks[slotNo] = stackSize;
            return customItem;
        }

    }

}
