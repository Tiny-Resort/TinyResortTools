using System.Collections.Generic;
using UnityEngine;

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

        private static List<InventoryItem> modItemList = new List<InventoryItem>();
        
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
                TRTools.LogError("Attempting to get item details for item with ID of " + itemID + " which does not exist.");
                return null;
            }
            return itemDetails[itemID];
        }

        internal static void InitializeNewItem(AssetBundle asset) {
            
        } 
        
    }

    
    
    // InventoryItem Script
    // Name, Description, Value, Sprite, Prefab, Alt Drop Prefab (dropping on floor prefab)
    
    // Special Options: Change To When Used (inventoryItem), Change to and still use fuel, Hide Highlighter
    
    // Animation Settings: Has Use Animation Stance, Use Right Hand Anim, My Anim Type (enum)
    
    // Placeable: Placeable (TileObject), Burried palceable, Ignore on Tile Object, Can be Place onto Tile Type, Placeable Tile Type
    
    // Placeable on to other tile objects: Can be Placed On To Tile Object (list), Status to Change to When placed On Top 
    
    // Item Type: Stackable, Max Stack, Is a tool, is power tool, is furniture,
    // can be placed in house, can be used in shops, is requestable, is unique item
    
    // Fuel and Stamina Options: Stamina Use type, Has fuel, Fuel Max, Fuel on Use, Custom Fuel Color
    
    // Weapon Info: Weapon Damage, Weapon Knockback, Can Block
    
    // Damage Tile Object Info: Damage Per Attack, Damage Wood, Damage Hard Wood, Damage Metal, Damage Stone, Damage Hard Stone
    // Damage Small Plants, Change to Height Tiles, Only Change Height Paths, Any Height
    
    // Damage Tile Types: Grass Growable, Can Damage Path, Can Damage Dirt, Can Damage Stone, Can Damage Stone, Can Damage Tilled Dirt
    // Can Damage Wet Tilled Dirt, Can Damage Dertilized Soil, Can Damage Wet Fertilized Soil, Resulting Tile Type, Ignore Two Arm Anim, Is Deed
    
    // Other Settings: Has Color Variation, Is Repairable, Can use Under Water
    
    // Spawn A World Object Or Vehicle: Spawn Placeable (game object)
    
    // Milestone & License: Required to Buy, Required License Level, Associated Task, Task When Sold
    
    // Other Scripts: Equipable, Craftable (Recipe), Consumeable, Item Change, Bug (Bug Identity), Fish, Underwater Creature, Relic
    
    
    internal class ModItemData {
        
        public ModItemData(string invItem, int id, bool outOfDate) {

        }
        
        public InventoryItem invItem;
        public string itemName;
        public string uniqueID;

    }

}
