using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using UnityEngine;
using UnityEngine.Events;
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
        private static Dictionary<string, TRCustomItem> customItems = new Dictionary<string, TRCustomItem>();
        private static Dictionary<int, TRCustomItem> customItemsByID = new Dictionary<int, TRCustomItem>();
        private static List<CurrentItems> currentItemList = new List<CurrentItems>();
        private static TRModData Data;

        internal static void Initialize() {
            Data = TRData.Subscribe("TR.CustomItems");
            /*TRData.cleanDataEvent += UnloadLicenses;
            TRData.preSaveEvent += SaveLicenseData;
            TRData.postLoadEvent += LoadLicenseData;
            TRData.injectDataEvent += LoadLicenses;*/
        }

        internal static bool customItemsInitialized;

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

        internal static void AddCustomItem(TRPlugin plugin, string relativePath, string uniqueItemID) {
            if (customItemsInitialized) {
                TRTools.LogError($"Mod attempted to load a new item after item initialization. You need to load new items in the Awake() method.");
                return;
            }
            customItems[plugin.nexusID + uniqueItemID] = new TRCustomItem(relativePath);
            customItems[plugin.nexusID + uniqueItemID].uniqueID = plugin.nexusID + uniqueItemID;
        }

        internal static void ManageAllItemArray() {
            // Make sure this runs only after the other items are added into the dictionary
            var customItemCount = customItems.Count;

            //TRTools.Log($"2---CustomItemsInitialized: {CustomItemsInitialized} -- Items: {customItems.Count}");

            Array.Resize<InventoryItem>(ref Inventory.inv.allItems, Inventory.inv.allItems.Length + customItemCount);
            var itemCount = Inventory.inv.allItems.Length - 1;

            //TRTools.Log($"ItemCount: {ItemCount} -- Items: {customItems.Count}");

            Array.Resize<TileObject>(ref WorldManager.manageWorld.allObjects, WorldManager.manageWorld.allObjects.Length + customItemCount);
            var tileObjectCount = WorldManager.manageWorld.allObjects.Length - 1;

            //TRTools.Log($"TileObjectCount: {TileObjectCount} -- Items: {customItems.Count}");

            Array.Resize<TileObjectSettings>(ref WorldManager.manageWorld.allObjectSettings, WorldManager.manageWorld.allObjectSettings.Length + customItemCount);
            var tileObjectSettingsCount = WorldManager.manageWorld.allObjectSettings.Length - 1;

            //TRTools.Log($"TileObjectSettingsCount: {TileObjectSettingsCount} -- Items: {customItems.Count}");

            int loopCount = 0;
            foreach (var item in customItems) {

                Inventory.inv.allItems[itemCount - loopCount] = item.Value.invItem;
                item.Value.invItem.setItemId(itemCount - loopCount);
                if (item.Value.tileObject && item.Value.tileObjectSettings) {
                    WorldManager.manageWorld.allObjects[tileObjectCount - loopCount] = item.Value.tileObject;
                    item.Value.tileObject.tileObjectId = tileObjectCount - loopCount;

                    WorldManager.manageWorld.allObjectSettings[tileObjectSettingsCount - loopCount] = item.Value.tileObjectSettings;
                    item.Value.tileObjectSettings.tileObjectId = tileObjectSettingsCount - loopCount;
                }
                customItemsByID[itemCount - loopCount] = item.Value;

                loopCount += 1;
            }

            // Resize and/or add mod save data of if they have obtained it or not
            // In a pre-save event, go through all items in catalogue array past vanilla items and check which are true and set them. 
            // set on inject data event (after saving and loading)
            CatalogueManager.manage.collectedItem = new bool[Inventory.inv.allItems.Length];

            //CheatScript.cheat.cheatButtons = new GameObject[Inventory.inv.allItems.Length];
            // This works and doesn't get saved so oh well
            var cheatButton = typeof(CheatScript).GetField("cheatButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            cheatButton.SetValue(CheatScript.cheat, new GameObject[Inventory.inv.allItems.Length]);
            customItemsInitialized = true;

        }

        internal static void UnloadPlayerInventory() {

            for (int i = 0; i < Inventory.inv.invSlots.Length; i++) {
                var currentSlot = Inventory.inv.invSlots[i];

                if (customItemsByID.ContainsKey(currentSlot.itemNo)) {
                    TRTools.Log($"Found Custom Item: {customItemsByID[currentSlot.itemNo].invItem.itemName}");
                    currentItemList.Add(new CurrentItems(customItemsByID[currentSlot.itemNo], i, currentSlot.stack));
                    currentSlot.updateSlotContentsAndRefresh(-1, 0);
                }
            }
        }

        internal static void UnloadChests() {
            TRTools.Log($"0: {WorldManager.manageWorld.onTileMap.GetLength(0)} | 1: {WorldManager.manageWorld.onTileMap.GetLength(1)}");
            for (int i = 0; i < WorldManager.manageWorld.onTileMap.GetLength(0); i++) {
                for (int j = 0; j < WorldManager.manageWorld.onTileMap.GetLength(1); j++) {

                    if (WorldManager.manageWorld.onTileMap[i, j] > -1) {
                        if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[i, j]].tileObjectChest) {

                            var currentChestOutside = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[i, j]].tileObjectChest;
                            currentChestOutside.checkIfEmpty(i, j, null);
                            var chestOutside = ContainerManager.manage.activeChests.First(p => p.xPos == i && p.yPos == j && !p.inside);
                            for (int z = 0; z < chestOutside.itemIds.Length; z++) {
                                if (customItemsByID.ContainsKey(chestOutside.itemIds[z])) {
                                    //TRTools.Log($"Found Custom Item: {customItemsByID[chestOutside.itemIds[z]].invItem.itemName}");
                                    currentItemList.Add(new CurrentItems(customItemsByID[chestOutside.itemIds[z]], z, chestOutside.itemStacks[z], i, j, null));
                                    ContainerManager.manage.changeSlotInChest(i, j, z, -1, 0, null);
                                }
                            }
                        }
                        else if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[i, j]].displayPlayerHouseTiles) {
                            var houseDetails = HouseManager.manage.getHouseInfo(i, j);
                            for (int n = 0; n < houseDetails.houseMapOnTile.GetLength(0); n++) {
                                for (int o = 0; o < houseDetails.houseMapOnTile.GetLength(1); o++) {
                                    if (houseDetails.houseMapOnTile[n, o] > 0) {

                                        if (WorldManager.manageWorld.allObjects[houseDetails.houseMapOnTile[n, o]].tileObjectChest) {
                                            var currentChestInside = WorldManager.manageWorld.allObjects[houseDetails.houseMapOnTile[n, o]].tileObjectChest;
                                            currentChestInside.checkIfEmpty(n, o, houseDetails);
                                            var chestInside = ContainerManager.manage.activeChests.First(p => p.xPos == n && p.yPos == o && p.inside);

                                            for (int z = 0; z < chestInside.itemIds.Length; z++) {
                                                if (customItemsByID.ContainsKey(chestInside.itemIds[z])) {
                                                    //TRTools.Log($"Found Custom Item: {customItemsByID[chestInside.itemIds[z]].invItem.itemName}");
                                                    currentItemList.Add(new CurrentItems(customItemsByID[chestInside.itemIds[z]], z, chestInside.itemStacks[z], n, o, houseDetails));
                                                    ContainerManager.manage.changeSlotInChest(n, o, z, -1, 0, houseDetails);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void SaveCustomItems() {
            currentItemList.Clear();
            UnloadPlayerInventory();
            UnloadChests();
            Data.SetValue("CurrentItemList", currentItemList);
        }

        internal static void LoadCustomItems() {

            if (currentItemList.Count == 0) {
                currentItemList = (List<CurrentItems>)Data.GetValue("CurrentItemList", new List<CurrentItems>());

                // Might need to compare currentItemList with customItems to get new ItemIDs if this is the first load.
                // This might work??? 
                foreach (var item in currentItemList) {
                    if (customItems.ContainsKey(item.customItem.uniqueID)) {
                        item.customItem.invItem.setItemId(item.customItem.invItem.getItemId());
                    }
                }
            }
            
            if (currentItemList.Count > 0) {
                for (int i = 0; i < currentItemList.Count; i++) {
                    if (currentItemList[i].playerInventory) {
                        
                        Inventory.inv.invSlots[currentItemList[i].slotNo].updateSlotContentsAndRefresh(currentItemList[i].customItem.invItem.getItemId(), currentItemList[i].stackSize);
                    }
                    
                    else if (currentItemList[i].chestInventory) {
                        var chestX = currentItemList[i].chestX;
                        var chestY = currentItemList[i].chestY;
                        var slotNo = currentItemList[i].slotNo;
                        var stackNo = currentItemList[i].stackSize;
                        var houseDetails = currentItemList[i].playerHouse;

                        ContainerManager.manage.changeSlotInChest(chestX, chestY, slotNo, currentItemList[i].customItem.invItem.getItemId(), stackNo, houseDetails);
                    }
                }
            }
        }
    }

    internal class TRCustomItem {

        public TRCustomItem(string assetBundlePath) {
            TRTools.Log("Attemping Load: Asset Bundle.");
            this.assetBundle = TRAssets.LoadBundle(assetBundlePath);
            TRTools.Log($"Loaded: Asset Bundle -- {this.assetBundle}.");

            var AllAssets = this.assetBundle.LoadAllAssets<GameObject>();

            for (int i = 0; i < AllAssets.Length; i++) {

                if (this.invItem == null) {
                    TRTools.Log("Attemping Load: InvItem.");
                    this.invItem = AllAssets[i].GetComponent<InventoryItem>();
                    TRTools.Log($"Loaded: InvItem -- {this.invItem}.");
                }
                if (this.tileObject == null) {
                    TRTools.Log("Attemping Load: TileObject.");
                    this.tileObject = AllAssets[i].GetComponent<TileObject>();
                    TRTools.Log($"Loaded: TileObject -- {this.tileObject}.");
                }
                if (this.tileObjectSettings == null) {
                    TRTools.Log("Attemping Load: TileObjectSettings.");
                    this.tileObjectSettings = AllAssets[i].GetComponent<TileObjectSettings>();
                    TRTools.Log($"Loaded: TileObjectSettings -- {this.tileObjectSettings}.");
                }
            }

            this.assetBundle.Unload(false);
        }

        public AssetBundle assetBundle;
        public InventoryItem invItem;
        public TileObject tileObject;
        public TileObjectSettings tileObjectSettings;
        public string uniqueID;

        // Dictionary<int, int> of slots of the item is in and how many -- player inventory
        // need for inv and chest

        public delegate void TileObjectEvent();
        public TileObjectEvent interactEvent;

    }

    internal class CurrentItems {

        public CurrentItems(TRCustomItem customItem, int slotNo, int stackSize) {
            this.customItem = customItem;
            this.slotNo = slotNo;
            this.stackSize = stackSize;
            this.playerInventory = true;
        }

        public CurrentItems(TRCustomItem customItem, int slotNo, int stackSize, int chestX, int chestY, HouseDetails playerHouse = null) {
            this.customItem = customItem;
            this.slotNo = slotNo;
            this.stackSize = stackSize;
            this.chestX = chestX;
            this.chestY = chestY;
            this.playerHouse = playerHouse;
            this.chestInventory = true;
        }

        public TRCustomItem customItem;
        public int slotNo;
        public int stackSize;
        public bool playerInventory;
        public bool chestInventory;
        public int chestX;
        public int chestY;
        public HouseDetails playerHouse;

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
