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
        private static Dictionary<int, TRCustomItem> customTileObjectByID = new Dictionary<int, TRCustomItem>();
        private static List<ItemSaveData> savedItemData = new List<ItemSaveData>();
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

        internal static void AddCustomItem(TRPlugin plugin, string assetBundlePath, string uniqueItemID) {

            if (customItemsInitialized) {
                TRTools.LogError($"Mod attempted to load a new item after item initialization. You need to load new items in your Awake() method!");
                return;
            }

            customItems[plugin.nexusID + uniqueItemID] = new TRCustomItem(assetBundlePath);
            customItems[plugin.nexusID + uniqueItemID].uniqueID = plugin.nexusID + uniqueItemID;

        }

        internal static void ManageAllItemArray() {

            // Make sure this runs only after the other items are added into the dictionary

            // Get existing item lists
            var itemList = Inventory.inv.allItems.ToList();
            var tileObjectList = WorldManager.manageWorld.allObjects.ToList();
            var tileObjectSettingsList = WorldManager.manageWorld.allObjectSettings.ToList();

            // Add custom items to existing item lists
            foreach (var item in customItems) {

                // Add custom inventory item
                itemList.Add(item.Value.invItem);
                item.Value.invItem.setItemId(itemList.Count - 1);
                customItemsByID[itemList.Count - 1] = item.Value;

                // Add tile object and settings if they exist
                if (item.Value.tileObject) {
                    tileObjectList.Add(item.Value.tileObject);
                    item.Value.tileObject.tileObjectId = tileObjectList.Count - 1;
                    customTileObjectByID[tileObjectList.Count - 1] = item.Value;
                    tileObjectSettingsList.Add(item.Value.tileObjectSettings);
                    item.Value.tileObjectSettings.tileObjectId = tileObjectSettingsList.Count - 1;
                }

            }

            // Set the game's arrays to match the new lists that include the custom items
            Inventory.inv.allItems = itemList.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectList.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsList.ToArray();

            // TODO:
            // Resize and/or add mod save data of if they have obtained it or not
            // In a pre-save event, go through all items in catalogue array past vanilla items and check which are true and set them. 
            // set on inject data event (after saving and loading)
            CatalogueManager.manage.collectedItem = new bool[Inventory.inv.allItems.Length];

            //CheatScript.cheat.cheatButtons = new GameObject[Inventory.inv.allItems.Length];
            // This works and doesn't get saved so oh well
            var cheatButton = typeof(CheatScript).GetField("cheatButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            cheatButton.SetValue(CheatScript.cheat, new GameObject[Inventory.inv.allItems.Length]);

            customItemsInitialized = true;
            InitializeItemDetails();

        }

        internal static void UnloadCustomItems() {

            // Unloads (and saves) items from the player's inventory
            for (int i = 0; i < Inventory.inv.invSlots.Length; i++) {
                var currentSlot = Inventory.inv.invSlots[i];
                if (customItemsByID.ContainsKey(currentSlot.itemNo)) {
                    TRTools.Log($"Found Custom Item: {customItemsByID[currentSlot.itemNo].invItem.itemName}");
                    savedItemData.Add(new ItemSaveData(customItemsByID[currentSlot.itemNo], i, currentSlot.stack));
                    currentSlot.updateSlotContentsAndRefresh(-1, 0);
                }
            }
            
            if (customItemsByID.ContainsKey(EquipWindow.equip.hatSlot.itemNo)) {
                var hatSlot = EquipWindow.equip.hatSlot;
                savedItemData.Add(new ItemSaveData(customItemsByID[hatSlot.itemNo], ItemSaveData.EquipLocation.Hat, hatSlot.stack));
                hatSlot.updateSlotContentsAndRefresh(-1, 0);
            }
            if (customItemsByID.ContainsKey(EquipWindow.equip.faceSlot.itemNo)) {
                var faceSlot = EquipWindow.equip.faceSlot;
                savedItemData.Add(new ItemSaveData(customItemsByID[faceSlot.itemNo], ItemSaveData.EquipLocation.Face, faceSlot.stack));
                faceSlot.updateSlotContentsAndRefresh(-1, 0);
            }
            if (customItemsByID.ContainsKey(EquipWindow.equip.shirtSlot.itemNo)) {
                var shirtSlot = EquipWindow.equip.shirtSlot;
                savedItemData.Add(new ItemSaveData(customItemsByID[shirtSlot.itemNo], ItemSaveData.EquipLocation.Shirt, shirtSlot.stack));
                shirtSlot.updateSlotContentsAndRefresh(-1, 0);
            }
            if (customItemsByID.ContainsKey(EquipWindow.equip.pantsSlot.itemNo)) {
                var pantsSlot = EquipWindow.equip.pantsSlot;
                savedItemData.Add(new ItemSaveData(customItemsByID[pantsSlot.itemNo], ItemSaveData.EquipLocation.Pants, pantsSlot.stack));
                pantsSlot.updateSlotContentsAndRefresh(-1, 0);
            }
            if (customItemsByID.ContainsKey(EquipWindow.equip.shoeSlot.itemNo)) {
                var shoeSlot = EquipWindow.equip.shoeSlot;
                savedItemData.Add(new ItemSaveData(customItemsByID[shoeSlot.itemNo], ItemSaveData.EquipLocation.Shoes, shoeSlot.stack));
                shoeSlot.updateSlotContentsAndRefresh(-1, 0);
            }
            
            // Removed or possible planned content?
            //if (customItemsByID.ContainsKey(EquipWindow.equip.idolSlot.itemNo)) { }
            

            // Go through every tile on the world map and look for objects that are custom items to unload them
            // This prevents corrupted save data if the player removes the mod and tries to load
            var allObjects = WorldManager.manageWorld.allObjects;
            var tileMap = WorldManager.manageWorld.onTileMap;
            for (int x = 0; x < tileMap.GetLength(0); x++) {
                for (int y = 0; y < tileMap.GetLength(1); y++) {
                    if (tileMap[x, y] <= -1) continue;

                    // If the tile has a chest on it, save and unload custom items from the chest
                    if (allObjects[tileMap[x, y]].tileObjectChest) { UnloadFromChest(allObjects[tileMap[x, y]].tileObjectChest, x, y, null); }

                    // If the tile contains a custom world object, save and unload it
                    else if (customTileObjectByID.ContainsKey(tileMap[x,y])) {
                        var rotation = WorldManager.manageWorld.rotationMap[x, y];
                        UnloadWorldObject(tileMap[x, y], x, y, rotation, null);
                    }
                    // Might need to check for burried items
                    // removeMultiTiledObject
                    // removeMultiTiledObjectInside
                    // remmoveItemOnTop
                    // removeItemOnTopInside
                    // PlaceMultiTiledObject
                    // placeBridgeTiledObject
                    // placeItemOnTop
                    // Might need to remove item from things like a furnace
                    // Donate fish????
                    // Mail?
                    
                    // Remove item on top of other items
                    // If the tile is the location of a house, check what's in the house
                    else if (allObjects[tileMap[x, y]].displayPlayerHouseTiles) {
                        var houseDetails = HouseManager.manage.getHouseInfo(x, y);
                        for (int houseX = 0; houseX < houseDetails.houseMapOnTile.GetLength(0); houseX++) {
                            for (int houseY = 0; houseY < houseDetails.houseMapOnTile.GetLength(1); houseY++) {

                                // If nothing is on this tile, ignore it
                                var tileObjectID = houseDetails.houseMapOnTile[houseX, houseY];
                                if (tileObjectID <= 0) continue;
                                
                                // If the object on this house tile is a chest, save and unload custom items from the chest
                                if (allObjects[tileObjectID].tileObjectChest) { UnloadFromChest(allObjects[tileObjectID].tileObjectChest, houseX, houseY, houseDetails); }

                                // If it's a custom item, save and unload it
                                else if (customTileObjectByID.ContainsKey(tileObjectID)) {
                                    var rotation = WorldManager.manageWorld.rotationMap[houseX, houseY];
                                    UnloadWorldObject(tileObjectID, houseX, houseY, rotation, houseDetails);
                                }

                            }
                        }
                    }
                    
                }
            }
        }

        internal static void UnloadFromChest(ChestPlaceable chestPlaceable, int x, int y, HouseDetails houseDetails) {
            chestPlaceable.checkIfEmpty(x, y, houseDetails);
            var chest = ContainerManager.manage.activeChests.First(p => p.xPos == x && p.yPos == y && p.inside == (houseDetails != null));
            for (int z = 0; z < chest.itemIds.Length; z++) {
                if (customItemsByID.ContainsKey(chest.itemIds[z])) {
                    //TRTools.Log($"Found Custom Item: {customItemsByID[chestOutside.itemIds[z]].invItem.itemName}");
                    savedItemData.Add(new ItemSaveData(customItemsByID[chest.itemIds[z]], z, chest.itemStacks[z], x, y, houseDetails));
                    ContainerManager.manage.changeSlotInChest(x, y, z, -1, 0, houseDetails);
                }
            }
        }
        
        internal static void UnloadWorldObject(int tileObjectID, int x, int y, int rotation, HouseDetails houseDetails) {
            savedItemData.Add(new ItemSaveData(customTileObjectByID[tileObjectID], x, y, rotation, houseDetails));
            if (houseDetails != null) { customItemsByID[tileObjectID].tileObject.removeMultiTiledObjectInside(x, y, rotation, houseDetails); }
            else { customItemsByID[tileObjectID].tileObject.removeMultiTiledObject(x, y, rotation); }
        }

        internal static void SaveCustomItems() {
            savedItemData.Clear();
            UnloadCustomItems();
            Data.SetValue("CurrentItemList", savedItemData);
        }

        internal static void LoadCustomItems() {

            // If not loaded yet, load the custom items that were in the player's inventory or chests
            if (savedItemData.Count == 0) { savedItemData = (List<ItemSaveData>)Data.GetValue("CurrentItemList", new List<ItemSaveData>()); }

            if (savedItemData.Count > 0) {
                foreach (var item in savedItemData) {

                    if (!customItems.TryGetValue(item.uniqueID, out var customItem)) continue;

                    switch (item.location) {

                        case ItemSaveData.ItemLocations.Inventory:
                            Inventory.inv.invSlots[item.slotNo].updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                            break;

                        case ItemSaveData.ItemLocations.Chest:
                            ContainerManager.manage.changeSlotInChest(item.xPos, item.yPos, item.slotNo, customItem.invItem.getItemId(), item.stackSize, item.houseDetails);
                            break;

                        case ItemSaveData.ItemLocations.Equipped:
                            switch (item.equipment) {
                                case ItemSaveData.EquipLocation.Hat:
                                    EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                                    break;
                                case ItemSaveData.EquipLocation.Face:
                                    EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                                    break;
                                case ItemSaveData.EquipLocation.Shirt:
                                    EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                                    break;
                                case ItemSaveData.EquipLocation.Pants:
                                    EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                                    break;
                                case ItemSaveData.EquipLocation.Shoes:
                                    EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                                    break;
                            }
                            break;

                        // TODO: Handle these below cases
                        case ItemSaveData.ItemLocations.Stash: break;
                        case ItemSaveData.ItemLocations.World:
                            if (item.houseDetails != null) { customItem.tileObject.removeMultiTiledObjectInside(item.xPos, item.yPos, item.rotation, item.houseDetails); }
                            else { customItem.tileObject.removeMultiTiledObject(item.xPos, item.yPos, item.rotation); }
                            // starting x, starting y, rotation
                            // place inside vs outside vs underground
                            break;
                        case ItemSaveData.ItemLocations.HomeFloor: break;
                        case ItemSaveData.ItemLocations.HomeWall: break;

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

        // TODO: Implement events
        public delegate void TileObjectEvent();
        public TileObjectEvent interactEvent;

    }

    internal class ItemSaveData {

        // For saving items that were in the player's inventory
        public ItemSaveData(TRCustomItem customItem, int slotNo, int stackSize) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Inventory;
            this.slotNo = slotNo;
            this.stackSize = stackSize;
        }

        // For saving items that were in a chest
        public ItemSaveData(TRCustomItem customItem, int slotNo, int stackSize, int xPos, int yPos, HouseDetails houseDetails = null) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Chest;
            this.houseDetails = houseDetails;
            this.stackSize = stackSize;
            this.slotNo = slotNo;
            this.xPos = xPos;
            this.yPos = yPos;
        }

        // For saving items that out in the world
        public ItemSaveData(TRCustomItem customItem, int xPos, int yPos, int rotation, HouseDetails houseDetails = null) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.World;
            this.houseDetails = houseDetails;
            this.rotation = rotation;
            this.xPos = xPos;
            this.yPos = yPos;
        }

        public ItemSaveData(TRCustomItem customItem, EquipLocation equipment, int stackSize) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Equipped;
            this.equipment = equipment;
            this.stackSize = stackSize;
        }
        
        public string uniqueID;
        public ItemLocations location;
        public EquipLocation equipment;
        public HouseDetails houseDetails;
        public int xPos;
        public int yPos;
        public int rotation;
        public int slotNo;
        public int stackSize;

        public enum ItemLocations { Inventory, Equipped, Chest, Stash, World, HomeFloor, HomeWall }
        public enum EquipLocation { Hat, Face, Shirt, Pants, Shoes }

    }
}
