using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

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
        private static Dictionary<string, ModItemData> customItems = new Dictionary<string, ModItemData>();

        internal static bool CustomItemsInitialized;

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

        public static void InitializeNewItem(string uniqueID, string relativePath) {
            if (CustomItemsInitialized) {
                TRTools.LogError($"Mod attempted to load a new item after item initialization. You need to load new items in the Awake() method.");
                return;
            }
            customItems[uniqueID] = new ModItemData(relativePath);
            customItems[uniqueID].UniqueID = uniqueID;
        }

        internal static void ManageAllItemArray() {
            // Make sure this runs only after the other items are added into the dictionary
            var customItemCount = customItems.Count;
            //TRTools.Log($"2---CustomItemsInitialized: {CustomItemsInitialized} -- Items: {customItems.Count}");

            Array.Resize<InventoryItem>(ref Inventory.inv.allItems, Inventory.inv.allItems.Length + customItemCount);
            var ItemCount = Inventory.inv.allItems.Length - 1;
            //TRTools.Log($"ItemCount: {ItemCount} -- Items: {customItems.Count}");

            Array.Resize<TileObject>(ref WorldManager.manageWorld.allObjects, WorldManager.manageWorld.allObjects.Length + customItemCount);
            var TileObjectCount = WorldManager.manageWorld.allObjects.Length - 1;
            //TRTools.Log($"TileObjectCount: {TileObjectCount} -- Items: {customItems.Count}");

            Array.Resize<TileObjectSettings>(ref WorldManager.manageWorld.allObjectSettings, WorldManager.manageWorld.allObjectSettings.Length + customItemCount);
            var TileObjectSettingsCount = WorldManager.manageWorld.allObjectSettings.Length - 1;
            //TRTools.Log($"TileObjectSettingsCount: {TileObjectSettingsCount} -- Items: {customItems.Count}");

            int loopCount = 0;
            foreach (var item in customItems) {

                Inventory.inv.allItems[ItemCount - loopCount] = item.Value.InvItem;
                item.Value.InvItem.setItemId(ItemCount - loopCount);

                if (item.Value.TileObject && item.Value.TileObjectSettings) {
                    WorldManager.manageWorld.allObjects[TileObjectCount - loopCount] = item.Value.TileObject;
                    item.Value.TileObject.tileObjectId = TileObjectCount - loopCount;

                    WorldManager.manageWorld.allObjectSettings[TileObjectSettingsCount - loopCount] = item.Value.TileObjectSettings;
                    item.Value.TileObjectSettings.tileObjectId = TileObjectSettingsCount - loopCount;
                }
                
                loopCount += 1;
            }
            if (loopCount == customItems.Count) FixCheatCatalogue();

        }

        internal static void FixCheatCatalogue() {

            // Resize and/or add mod save data of if they have obtained it or not
            // In a pre-save event, go through all items in catalogue array past vanilla items and check which are true and set them. 
            // set on inject data event (after saving and loading)
            CatalogueManager.manage.collectedItem = new bool[Inventory.inv.allItems.Length];

            //CheatScript.cheat.cheatButtons = new GameObject[Inventory.inv.allItems.Length];
            // This works and doesn't get saved so oh well
            var cheatButton = typeof(CheatScript).GetField("cheatButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            cheatButton.SetValue(CheatScript.cheat, new GameObject[Inventory.inv.allItems.Length]);
            CustomItemsInitialized = true;
        }

    }

    internal class ModItemData {

        public ModItemData(string assetBundlePath) {
            TRTools.Log("Attemping Load: Asset Bundle.");
            this.AssetBundle = TRAssets.LoadBundle(assetBundlePath);
            TRTools.Log($"Loaded: Asset Bundle -- {this.AssetBundle}.");

            var AllAssets = this.AssetBundle.LoadAllAssets<GameObject>();

            for (int i = 0; i < AllAssets.Length; i++) {

                if (this.InvItem == null) {
                    TRTools.Log("Attemping Load: InvItem.");
                    this.InvItem = AllAssets[i].GetComponent<InventoryItem>();
                    TRTools.Log($"Loaded: InvItem -- {this.InvItem}.");
                }
                if (this.TileObject == null) {
                    TRTools.Log("Attemping Load: TileObject.");
                    this.TileObject = AllAssets[i].GetComponent<TileObject>();
                    TRTools.Log($"Loaded: TileObject -- {this.TileObject}.");
                }
                if (this.TileObjectSettings == null) {
                    TRTools.Log("Attemping Load: TileObjectSettings.");
                    this.TileObjectSettings = AllAssets[i].GetComponent<TileObjectSettings>();
                    TRTools.Log($"Loaded: TileObjectSettings -- {this.TileObjectSettings}.");
                }
            }

            this.AssetBundle.Unload(false);
        }

        public AssetBundle AssetBundle;
        public InventoryItem InvItem;
        public TileObject TileObject;
        public TileObjectSettings TileObjectSettings;
        public string UniqueID;

    }

    // updateSlotCOntentsAndRefresh Patch to not care about itemNo > arraysize of allitems
    // bad cause... Based on itemID number, might not correspond to the appropriate item and might miss if too many items added
    // clean and save and load save data

    // Order of code to do
    // Get Inventory
    // Get Chests
    // Get Placeables

    // Using items    

    // Maybe Asset for creations
    // Backend:
    // Always track where custom items are (prob unique key with dictionary)
    // AllLicensesArray -> AllItemsArray
    // CustomItems -> Add to AllItems -> refrence to inv item

    // subscribe to TRData.cleandata event
    // What inventory slot are stuff in and how many there are, remove it all before saving
    // on injectdata event
    // add all items back to inventory.

    // Cleaned up Placed Objects

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

}
