using System.Collections.Generic;

namespace TinyResort {

    /// <summary>Tools for working with the Dinkum inventory.</summary>
    public class TRItems {
        
        // TODO: Add Item Framework for adding new items. 
        // Save item using Mod Save Data. 
        // Resize the array (keep track of old array size). 
        // Update the array with all the new items and keep track of item IDs
        // Before saving, save status of all items and remove them from....
        // 1. Inventory
        // 2. Chests
        // 3. Placed World Objects
        // 4. The Item Array
        // 5. Tile Object Array
        // Restore positions of all items after saving to their approproate places
        
        private static Dictionary<int, InventoryItem> itemDetails = new Dictionary<int, InventoryItem>();
        private static void InitializeItemDetails() {
            foreach (var item in Inventory.inv.allItems) {
                var id = item.getItemId();
                itemDetails[id] = item;
            }
        }

        /// <returns>Whether or not the item exists.</returns>
        public static bool DoesItemExist(int itemID) {
            if (itemDetails.Count <= 0) { InitializeItemDetails(); }
            return itemID >= 0 && itemDetails.ContainsKey(itemID);
        }

        /// <returns>The details for an item with the given item ID.</returns>
        public static InventoryItem GetItemDetails(int itemID) {
            if (itemDetails.Count <= 0) { InitializeItemDetails(); }
            if (itemID < 0) {
                TRTools.Log("Attempting to get item details for item with ID of " + itemID + " which does not exist.", LogSeverity.Error, false);
                return null;
            }
            return itemDetails[itemID];
        }
        
    }

}
