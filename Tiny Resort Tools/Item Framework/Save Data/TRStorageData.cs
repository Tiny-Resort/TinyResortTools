using System;
using System.Collections.Generic;

namespace TinyResort {

    [Serializable]
    public class TRStorageData {

        public int maxSlots;

        // Data about each slot
        // Easy way to grab InvItem component from any slot
        // saving/loading
        // Events for when items are added or removed

        internal Dictionary<int, string> itemIDs = new Dictionary<int, string>();
        internal Dictionary<int, int> itemStacks = new Dictionary<int, int>();

        /// <returns>The InventoryItem component of the item in the specified slot.</returns>
        public InventoryItem GetItemInSlot(int slot) {
            if (slot >= maxSlots || !itemIDs.TryGetValue(slot, out var ID)) return null;
            var itemID = TRItems.GetLoadableItemID(ID);
            return Inventory.inv.allItems[itemID];
        }

        /// <returns>The quantity of the item in the specified slot.</returns>
        public int GetAmountInSlot(int slot) {
            if (slot >= maxSlots || !itemStacks.TryGetValue(slot, out var quantity)) return 0;
            return quantity;
        }

        /// <summary> Sets the item that should be in a specific slot of this storage. </summary>
        /// <param name="slot">The slot number. Must be less than the maxSlots setting. </param>
        /// <param name="item">The item that you want to put in the slot.</param>
        /// <param name="quantity">How many of the item should be in the slot now.</param>
        public void ChangeItemInSlot(int slot, InventoryItem item, int quantity) {
            if (slot >= maxSlots || item != null) return;
            var ID = TRItems.GetSaveableItemID(item.getItemId());
            if (ID == null) return;
            itemIDs[slot] = ID;
            itemStacks[slot] = quantity;
        }

        internal static TRStorageData Create(int maxSlots) {
            var storage = new TRStorageData();

            return storage;
        }

    }

    public class SlotItem {

        /// <summary> Will be 0 if its a vanilla item. Otherwise, will return the nexus ID of the mod that added it. </summary>
        public int source;

        /// <summary> Will be the base game item ID if its a vanilla item. Otherwise it will be the unique ID associate with a custom item (starting with its nexus ID).</summary>
        public string ID;

    }

}
