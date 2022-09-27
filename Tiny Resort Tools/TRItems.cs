using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                    tileObjectSettingsList.Add(item.Value.tileObjectSettings);
                    item.Value.tileObjectSettings.tileObjectId = tileObjectSettingsList.Count - 1;
                    customTileObjectByID[tileObjectList.Count - 1] = item.Value;
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

            // Might need to check for burried items
            // placeBridgeTiledObject
            // Might need to remove item from things like a furnace
            // Donate fish????
            // Mail?
            savedItemData.Clear();

            #region Inventory
            // Unloads (and saves) items from the player's inventory
            for (int i = 0; i < Inventory.inv.invSlots.Length; i++) {
                var currentSlot = Inventory.inv.invSlots[i];
                if (customItemsByID.ContainsKey(currentSlot.itemNo)) {
                    TRTools.Log($"Found Custom Item: {customItemsByID[currentSlot.itemNo].invItem.itemName}");
                    savedItemData.Add(new ItemSaveData(customItemsByID[currentSlot.itemNo], i, currentSlot.stack));
                    currentSlot.updateSlotContentsAndRefresh(-1, 0);
                }
            }
            #endregion

            #region Stashes

            for (int j = 0; j < ContainerManager.manage.privateStashes.Count; j++) {
                var stash = ContainerManager.manage.privateStashes[j];
                for (int i = 0; i < stash.itemIds.Length; i++) {
                    TRTools.Log($"Found Item: {stash.itemIds[i]}");
                    if (customItemsByID.ContainsKey(stash.itemIds[i])) {
                        savedItemData.Add(new ItemSaveData(customItemsByID[stash.itemIds[i]], ItemSaveData.ItemLocations.Stash, stash.itemStacks[i], j, i));
                        stash.itemIds[i] = -1;
                        stash.itemStacks[i] = 0;
                    }
                }
            }
            #endregion
            
            #region Equipment Slots
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
            #endregion
            // Removed or possible planned content?
            //if (customItemsByID.ContainsKey(EquipWindow.equip.idolSlot.itemNo)) { }
            
            // Go through every tile on the world map and look for objects that are custom items to unload them
            // This prevents corrupted save data if the player removes the mod and tries to load
            #region World Objects & Chests
            var allObjects = WorldManager.manageWorld.allObjects;
            var tileMap = WorldManager.manageWorld.onTileMap;
            for (int x = 0; x < tileMap.GetLength(0); x++) {
                for (int y = 0; y < tileMap.GetLength(1); y++) {
                    if (tileMap[x, y] <= -1) continue;

                    // PlaceItemOnTop is untested

                    #region OnTopOf Outside
                    var onTopOfTile = ItemOnTopManager.manage.getAllItemsOnTop(x, y, null);

                    for (int i = 0; i < onTopOfTile.Length; i++) {
                        if (customTileObjectByID.ContainsKey(onTopOfTile[i].itemId)) {
                            UnloadWorldObjectOnTop(
                                onTopOfTile[i].itemId, x, y, -1, -1, onTopOfTile[i].itemRotation, null, onTopOfTile[i].itemStatus,
                                true, onTopOfTile[i].onTopPosition
                            );
                        }
                    }
                    #endregion

                    #region Chests
                    // If the tile has a chest on it, save and unload custom items from the chest
                    if (allObjects[tileMap[x, y]].tileObjectChest) { UnloadFromChest(allObjects[tileMap[x, y]].tileObjectChest, x, y, null); }

                    #endregion
                    
                    #region Onject On Floor Outside
                    // If the tile contains a custom world object, save and unload it
                    else if (customTileObjectByID.ContainsKey(tileMap[x, y])) {
                        var rotation = WorldManager.manageWorld.rotationMap[x, y];
                        UnloadWorldObject(tileMap[x, y], -1, -1, x, y, rotation, null);
                    }
                    #endregion

                    // Remove item on top of other items
                    // If the tile is the location of a house, check what's in the house
                    else if (allObjects[tileMap[x, y]].displayPlayerHouseTiles) {
                        var houseDetails = HouseManager.manage.getHouseInfo(x, y);
                        var onTopOfTileInside = ItemOnTopManager.manage.getAllItemsOnTopInHouse(houseDetails);

                        #region OnTopOf Inside
                        // PlaceItemOnTop is untested
                        // Needs to get items on top of everything BEFORE removing any items. 
                        for (int i = 0; i < onTopOfTileInside.Length; i++) {
                            if (customTileObjectByID.ContainsKey(onTopOfTileInside[i].getTileObjectId())) {
                                TRTools.Log($"Found {customTileObjectByID[onTopOfTileInside[i].getTileObjectId()].invItem.itemName}");
                                UnloadWorldObjectOnTop(
                                    onTopOfTileInside[i].itemId, onTopOfTileInside[i].sittingOnX, onTopOfTileInside[i].sittingOnY, 
                                    onTopOfTileInside[i].houseX, onTopOfTileInside[i].houseY, onTopOfTileInside[i].itemRotation, houseDetails, onTopOfTileInside[i].itemStatus,
                                    true,
                                    onTopOfTileInside[i].onTopPosition
                                );
                            }
                        }
                        #endregion
                        
                        for (int houseX = 0; houseX < houseDetails.houseMapOnTile.GetLength(0); houseX++) {
                            for (int houseY = 0; houseY < houseDetails.houseMapOnTile.GetLength(1); houseY++) {
                                
                                // If nothing is on this tile, ignore it
                                var tileObjectID = houseDetails.houseMapOnTile[houseX, houseY]; 
                                if (tileObjectID <= 0) continue;

                                #region Chests Buildings
                                // If the object on this house tile is a chest, save and unload custom items from the chest
                                if (allObjects[tileObjectID].tileObjectChest) { UnloadFromChest(allObjects[tileObjectID].tileObjectChest, houseX, houseY, houseDetails); }

                                #endregion
                                
                                #region Objects on Ground Inside
                                // If it's a custom item, save and unload it
                                else if (customTileObjectByID.ContainsKey(tileObjectID)) {
                                    var rotation = houseDetails.houseMapRotation[houseX, houseY];
                                    UnloadWorldObject(tileObjectID, x, y, houseX, houseY, rotation, houseDetails);
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
            #endregion
        }

        internal static void UnloadFromChest(ChestPlaceable chestPlaceable, int x, int y, HouseDetails houseDetails) {
            chestPlaceable.checkIfEmpty(x, y, houseDetails);
            var chest = ContainerManager.manage.activeChests.First(p => p.xPos == x && p.yPos == y && p.inside == (houseDetails != null));
            for (int z = 0; z < chest.itemIds.Length; z++) {
                if (customItemsByID.ContainsKey(chest.itemIds[z])) {
                    savedItemData.Add(new ItemSaveData(customItemsByID[chest.itemIds[z]], z, chest.itemStacks[z], x, y, houseDetails));
                    ContainerManager.manage.changeSlotInChest(x, y, z, -1, 0, houseDetails);
                }
            }
        }

        internal static void UnloadWorldObject(int tileObjectID, int houseX, int houseY, int x, int y, int rotation, HouseDetails houseDetails) {
            savedItemData.Add(new ItemSaveData(customTileObjectByID[tileObjectID], x, y, rotation, ItemSaveData.WorldObject.OnTile, houseDetails));
            if (houseDetails != null) {
                customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObjectInside(x, y, rotation, houseDetails);
                houseDetails.houseMapOnTile[x, y] = -1;
                houseDetails.houseMapOnTileStatus[x, y] = -1;  
                var house = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
                if (house.tileObjectsInHouse[x, y].tileObjectFurniture) { house.tileObjectsInHouse[x, y].tileObjectFurniture.updateOnTileStatus(x, y, houseDetails); TRTools.Log($"Test 8.5");}
                house.refreshHouseTiles();
            }
            else {
                customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObject(x, y, rotation);
                WorldManager.manageWorld.onTileMap[x, y] = -1;
                WorldManager.manageWorld.onTileStatusMap[x, y] = -1;
                WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(x, y, false);
                NetworkNavMesh.nav.updateChunkInUse();
            }
        }

        internal static void UnloadWorldObjectOnTop(int tileObjectID, int x, int y, int houseX, int houseY, int rotation, HouseDetails houseDetails, int status, bool onTop = true, int onTopPos = -1) {
            TRTools.Log($"{customTileObjectByID[tileObjectID]}, {x}, {y}, {rotation}, {ItemSaveData.WorldObject.OnTop}, {houseDetails}, {status}, {onTopPos})"); 
            savedItemData.Add(new ItemSaveData(customTileObjectByID[tileObjectID], x, y, rotation, ItemSaveData.WorldObject.OnTop, houseDetails, status, onTopPos));
            if (houseDetails != null) {
                ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, x, y, HouseManager.manage.getHouseInfo(houseX, houseY)));
                DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
                if (displayPlayerHouseTiles && displayPlayerHouseTiles.tileObjectsInHouse[x, y]) {
                    displayPlayerHouseTiles.tileObjectsInHouse[x, y].checkOnTopInside(x, y, HouseManager.manage.getHouseInfo(houseX, houseY));
                }
            }
            else {
                ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, x, y, null));
                WorldManager.manageWorld.unlockClientTile(x, y);
                WorldManager.manageWorld.refreshAllChunksInUse(x, y, false);

            }
        }

        internal static void SaveCustomItems() {
            UnloadCustomItems();
            Data.SetValue("CurrentItemList", savedItemData);
        }

        internal static void LoadCustomItems() {

            // This should put OnTop to the end of the list and process them last.
            savedItemData = savedItemData.OrderBy(i => i.type == ItemSaveData.WorldObject.OnTop).ToList();
            // If not loaded yet, load the custom items that were in the player's inventory or chests
            if (savedItemData.Count == 0) { savedItemData = (List<ItemSaveData>)Data.GetValue("CurrentItemList", new List<ItemSaveData>()); }

            if (savedItemData.Count > 0) {
                foreach (var item in savedItemData) {
                    if (!customItems.TryGetValue(item.uniqueID, out var customItem)) continue;
                    TRTools.Log($"Item Found: {customItem.invItem.itemName}");

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

                        // TODO: This will not work if the stash window is open, so might need to do some patching?
                        case ItemSaveData.ItemLocations.Stash:
                            ContainerManager.manage.privateStashes[item.stashPostition].itemIds[item.slotNo] = customItem.invItem.getItemId();
                            ContainerManager.manage.privateStashes[item.stashPostition].itemStacks[item.slotNo] = item.stackSize;
                            break;

                        // TODO: Maybe remove particle/sound effects from objects outside
                        case ItemSaveData.ItemLocations.World:
                            switch (item.type) {
                                case ItemSaveData.WorldObject.OnTile:
                                    if (item.houseDetails != null) {
                                        customItem.tileObject.placeMultiTiledObjectInside(item.xPos, item.yPos, item.rotation, item.houseDetails);
                                        var house = HouseManager.manage.findHousesOnDisplay(item.houseDetails.xPos, item.houseDetails.yPos);
                                        item.houseDetails.houseMapOnTile[item.xPos, item.yPos] = customItem.tileObject.tileObjectId;
                                        item.houseDetails.houseMapOnTileStatus[item.xPos, item.yPos] = 0;
                                        house.refreshHouseTiles();
                                    }
                                    else {
                                        customItem.tileObject.placeMultiTiledObject(item.xPos, item.yPos, item.rotation);
                                        WorldManager.manageWorld.onTileMap[item.xPos, item.yPos] = customItem.tileObject.tileObjectId;
                                        WorldManager.manageWorld.onTileStatusMap[item.xPos, item.yPos] = 0;
                                        WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(item.xPos, item.yPos, false);
                                        NetworkNavMesh.nav.updateChunkInUse();
                                        WorldManager.manageWorld.unlockClientTile(item.xPos, item.yPos);
                                    }
                                    break;

                                // Untested
                                case ItemSaveData.WorldObject.OnTop:
                                    ItemOnTopManager.manage.placeItemOnTop(customItem.tileObject.tileObjectId, item.onTopPos, item.status, item.rotation, item.xPos, item.yPos, item.houseDetails);
                                    WorldManager.manageWorld.unlockClientTile(item.xPos, item.yPos);
                                    WorldManager.manageWorld.refreshAllChunksInUse(item.xPos, item.yPos, false);
                                    var houseOnTop = HouseManager.manage.findHousesOnDisplay(item.houseDetails.xPos, item.houseDetails.yPos);
                                    houseOnTop.refreshHouseTiles();
                                    break;

                                // TODO: Handle these below cases

                                case ItemSaveData.WorldObject.Bridge: break;

                            }
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
        public ItemSaveData(TRCustomItem customItem, int xPos, int yPos, int rotation, WorldObject type, HouseDetails houseDetails = null, int status = -1, int onTopPos = -1) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.World;
            this.type = type;
            this.houseDetails = houseDetails;
            this.rotation = rotation;
            this.xPos = xPos;
            this.yPos = yPos;
            this.type = type;
            this.onTopPos = onTopPos;
        }

        public ItemSaveData(TRCustomItem customItem, ItemLocations itemLocation, int stackSize, int stashPostition, int slotNo) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Stash;
            this.stackSize = stackSize;
            this.stashPostition = stashPostition;
            this.slotNo = slotNo;
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
        public WorldObject type;
        public int onTopPos;
        public int status;
        public int stashPostition;
        public HouseDetails houseDetails;
        public int xPos;
        public int yPos;
        public int rotation;
        public int slotNo;
        public int stackSize;

        public enum ItemLocations { Inventory, Equipped, Chest, Stash, World, HomeFloor, HomeWall }
        public enum EquipLocation { Hat, Face, Shirt, Pants, Shoes }
        public enum WorldObject { OnTile, OnTop, Bridge }
    }
}
