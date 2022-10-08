using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using I2.Loc;
using Mirror;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace TinyResort {

    /// <summary>Tools for working with the Dinkum inventory.</summary>
    public class TRItems {

        /*
        TODO: (SLOW) Refactor code.
        TODO: (SLOW) A chat command for giving yourself a custom item. 
        TODO: Be able to add it to enemy drops and chests including buried underground.
        TODO: Be able to add it to list of items that NPCs can give you, or you can find in the recycle bin.
        TODO: Be able to add it to Franklyn's crafting list.
        TODO: Be able to make a recipe that creates the item, and give the recipe to the player.
        TODO: (TANY) Quick creating tiles, floors and walls with just textures.
        TODO: Fix catalogue history
        TODO: (TANY) Update Sending Mail to be useable for other developers. It is too basic right now.
        TODO: (TANY) Double check adding icons works for modded items
        TODO: Add try{}catch{} statements to protect different points of the code from modders breaking it
        TODO: We should add a condition of saving non modded items in situations where a non modded item is on top of a modded one
        */

        /*
         * Note: Currently I am patching loadCarriables to do `TRItems.LoadCustomItemPostLoad();`. This is because I couldn't get the save data
         * to load for me correctly me when I moved it there. Since you understand the Save Data better I think it would be best for you to experiment.
         * Let me know if you want my save data with all the modded items to test stuff with or you can spawn the items in yourself. 
         */
        private static readonly Dictionary<int, InventoryItem> itemDetails = new Dictionary<int, InventoryItem>();
        internal static readonly Dictionary<string, TRCustomItem> customItems = new Dictionary<string, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customItemsByID = new Dictionary<int, TRCustomItem>();
        private static readonly Dictionary<int, TRCustomItem> customTileObjectByID = new Dictionary<int, TRCustomItem>();
        private static readonly Dictionary<int, TRCustomItem> customTileTypeByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customVehicleByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customCarryableByID = new Dictionary<int, TRCustomItem>();

        private static List<ItemSaveData> savedItemData = new List<ItemSaveData>();
        internal static List<ItemSaveData> savedDroppedItems = new List<ItemSaveData>();
        internal static List<ItemSaveData> savedItemDataLate = new List<ItemSaveData>();

        internal static TRModData Data;

        private static List<InventoryItem> itemList;
        private static List<TileObject> tileObjectList;
        private static List<TileObjectSettings> tileObjectSettingsList;
        private static List<TileTypes> tileTypesList;
        private static List<GameObject> vehiclePrefabList;
        private static List<GameObject> carryablePrefabList;

        private static List<bool> CatalogueDefaultList;
        private static List<InventoryItem> allItemsDefaultList;
        private static List<GameObject> vehiclePrefabDefaultList;
        private static List<GameObject> carryablePrefabDefaultList;

        private static List<TileObject> tileObjectDefaultList;
        private static List<TileObjectSettings> tileObjectSettingsDefaultList;

        internal static bool customItemsInitialized;
        internal static bool customItemsLoaded;

        internal static void Initialize() {
            Data = TRData.Subscribe("TR.CustomItems");
            TRData.cleanDataEvent += UnloadCustomItems;
            TRData.injectDataEvent += LoadCustomItems;
        }

        private static void InitializeItemDetails() {
            foreach (var item in Inventory.inv.allItems) {
                var id = item.getItemId();
                itemDetails[id] = item;
            }
        }

        /// <returns>Whether or not the item exists.</returns>
        public static bool DoesItemExist(int itemID) {
            if (itemDetails.Count <= 0) InitializeItemDetails();
            return itemID >= 0 && itemDetails.ContainsKey(itemID);
        }

        /// <returns>The details for an item with the given item ID.</returns>
        public static InventoryItem GetItemDetails(int itemID) {
            if (itemDetails.Count <= 0) InitializeItemDetails();
            if (itemID < 0) {
                TRTools.LogError("Attempting to get item details for item with ID of " + itemID + " which does not exist.");
                return null;
            }
            return itemDetails[itemID];
        }

        internal static TRCustomItem AddCustomItem(TRPlugin plugin, string assetBundlePath, string uniqueItemID) {

            if (customItemsInitialized) {
                TRTools.LogError("Mod attempted to load a new item after item initialization. You need to load new items in your Awake() method!");
                return null;
            }

            customItems[plugin.nexusID + uniqueItemID] = TRCustomItem.Create(assetBundlePath);
            customItems[plugin.nexusID + uniqueItemID].uniqueID = plugin.nexusID + uniqueItemID;
            return customItems[plugin.nexusID + uniqueItemID];
            
        }

        // Adds a new custom item to the dictionary but allows it to be created somewhere else
        internal static void AddCustomItem(TRCustomItem item, string uniqueID) {
            item.uniqueID = uniqueID;
            customItems[uniqueID] = item;
        }
        
        // Resize the array depending on the number of modded items added
        // Ignore modded items saved if it doesnt exist in customItems
        internal static void ManageAllItemArray() {
            // Default lengths of all the arrays
            allItemsDefaultList = Inventory.inv.allItems.ToList();
            tileObjectDefaultList = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsDefaultList = WorldManager.manageWorld.allObjectSettings.ToList();
            CatalogueDefaultList = CatalogueManager.manage.collectedItem.ToList();
            vehiclePrefabDefaultList = SaveLoad.saveOrLoad.vehiclePrefabs.ToList();
            carryablePrefabDefaultList = SaveLoad.saveOrLoad.carryablePrefabs.ToList();
            TRTools.Log($"Catalogue size: {CatalogueManager.manage.collectedItem.Length}");

            // Get existing item lists
            itemList = Inventory.inv.allItems.ToList();
            tileObjectList = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsList = WorldManager.manageWorld.allObjectSettings.ToList();
            tileTypesList = WorldManager.manageWorld.tileTypes.ToList();
            vehiclePrefabList = SaveLoad.saveOrLoad.vehiclePrefabs.ToList();
            carryablePrefabList = SaveLoad.saveOrLoad.carryablePrefabs.ToList();

            // Add custom items to existing item lists
            foreach (var item in customItems) {
                if (item.Value.tileTypes) {
                    if (item.Value.tileTypes.isPath) {
                        tileTypesList.Add(item.Value.tileTypes);
                        item.Value.invItem.placeableTileType = tileTypesList.Count - 1;
                        customTileTypeByID[tileTypesList.Count - 1] = item.Value;
                    }
                }

                // Add custom inventory item
                if (item.Value.invItem) {
                    itemList.Add(item.Value.invItem);
                    item.Value.invItem.setItemId(itemList.Count - 1);
                    customItemsByID[itemList.Count - 1] = item.Value;
                }

                // Add tile object and settings if they exist
                if (item.Value.tileObject) {
                    tileObjectList.Add(item.Value.tileObject);
                    item.Value.tileObject.tileObjectId = tileObjectList.Count - 1;
                    tileObjectSettingsList.Add(item.Value.tileObjectSettings);
                    item.Value.tileObjectSettings.tileObjectId = tileObjectSettingsList.Count - 1;
                    customTileObjectByID[tileObjectList.Count - 1] = item.Value;
                }
                if (item.Value.vehicle) {
                    vehiclePrefabList.Add(item.Value.invItem.spawnPlaceable);
                    item.Value.vehicle.saveId = vehiclePrefabList.Count - 1;
                    customVehicleByID[vehiclePrefabList.Count - 1] = item.Value;
                }
                if (item.Value.carryable) {
                    carryablePrefabList.Add(item.Value.carryable.gameObject);
                    item.Value.carryable.prefabId = carryablePrefabList.Count - 1;
                    customCarryableByID[carryablePrefabList.Count - 1] = item.Value;
                }

            }

            // Set the game's arrays to match the new lists that include the custom items
            Inventory.inv.allItems = itemList.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectList.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsList.ToArray();
            WorldManager.manageWorld.tileTypes = tileTypesList.ToArray();
            SaveLoad.saveOrLoad.vehiclePrefabs = vehiclePrefabList.ToArray();
            TRTools.Log($"Size of Old: {SaveLoad.saveOrLoad.carryablePrefabs.Length} | Size of New Carryable: {carryablePrefabList.Count}");
            SaveLoad.saveOrLoad.carryablePrefabs = carryablePrefabList.ToArray();

            foreach (var carry in SaveLoad.saveOrLoad.carryablePrefabs) { TRTools.Log($"Carry: {carry.name}"); }

            // TODO:
            // Resize and/or add mod save data of if they have obtained it or not
            // In a pre-save event, go through all items in catalogue array past vanilla items and check which are true and set them. 
            // set on inject data event (after saving and loading)
            //CatalogueManager.manage.collectedItem = new bool[Inventory.inv.allItems.Length];

            //CheatScript.cheat.cheatButtons = new GameObject[Inventory.inv.allItems.Length];
            // This works and doesn't get saved so oh well
            var cheatButton = typeof(CheatScript).GetField("cheatButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            cheatButton.SetValue(CheatScript.cheat, new GameObject[Inventory.inv.allItems.Length]);

            customItemsInitialized = true;
            InitializeItemDetails();

        }

        // This is used to restore the modded items into the lists after saving. It just takes the list of items we have
        // and adds them to the games arrays. 
        internal static void RestoreModSize() {
            Inventory.inv.allItems = itemList.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectList.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsList.ToArray();
            CatalogueManager.manage.collectedItem = new bool[Inventory.inv.allItems.Length];
            SaveLoad.saveOrLoad.vehiclePrefabs = vehiclePrefabList.ToArray();
            SaveLoad.saveOrLoad.carryablePrefabs = carryablePrefabList.ToArray();
        }

        // I am not positive this is required. I resize to restore the original size depending on the default list I create in the ManageAllItemsArray method
        // but... we might not need to resize it. If setting an array to a new array overwrites the whole thing, then we probably don't need to resize it. 
        internal static bool DefaultSize() {
            TRTools.Log($"Items: {Inventory.inv.allItems.Length} | Objects: {WorldManager.manageWorld.allObjects.Length} & {WorldManager.manageWorld.allObjectSettings.Length} | Catalogue: {CatalogueManager.manage.collectedItem.Length}");

            Array.Resize(ref Inventory.inv.allItems, allItemsDefaultList.Count);
            Array.Resize(ref WorldManager.manageWorld.allObjects, tileObjectDefaultList.Count);
            Array.Resize(ref WorldManager.manageWorld.allObjectSettings, tileObjectSettingsDefaultList.Count);
            Array.Resize(ref CatalogueManager.manage.collectedItem, allItemsDefaultList.Count);
            Array.Resize(ref SaveLoad.saveOrLoad.vehiclePrefabs, vehiclePrefabDefaultList.Count);
            Array.Resize(ref SaveLoad.saveOrLoad.carryablePrefabs, carryablePrefabDefaultList.Count);

            Inventory.inv.allItems = allItemsDefaultList.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectDefaultList.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsDefaultList.ToArray();
            CatalogueManager.manage.collectedItem = CatalogueDefaultList.ToArray();
            SaveLoad.saveOrLoad.vehiclePrefabs = vehiclePrefabDefaultList.ToArray();
            SaveLoad.saveOrLoad.carryablePrefabs = carryablePrefabDefaultList.ToArray();

            TRTools.Log($"Items: {Inventory.inv.allItems.Length} | Objects: {WorldManager.manageWorld.allObjects.Length} & {WorldManager.manageWorld.allObjectSettings.Length} | Catalogue: {CatalogueManager.manage.collectedItem.Length}");
            return true;
        }

        // One large method to go through all modded items in the game and remove them. 
        // This might be worth breaking up. Specifically, anything that is done before the Loops of the tiles can be put into
        // their own methods. 
        internal static void UnloadCustomItems() {

            TRTools.Log("Remvoing Items");

            // SavedItemData is the list of all item data that can be restored at the current load point
            savedItemData.Clear();

            // savedItemDataLate is the list of all item data that needs to wait for the map to be available (on first load). 
            savedItemDataLate.Clear();

            // The dropped item list can be incorporated into the savedItemDataLate list. It wasn't done because we dont restore them right now
            // The dropped items for now are set to not save and will always be deleted. This is to protect against James's changes in the future
            // that might break the API due to saving items in houses out of nowhere (its a b ug that they arent saved). 
            savedDroppedItems.Clear();

            #region House Wallpaper/Flooring

            // This goes through the allHouse array, finds any wall/floor that is custom and unloads them. 
            foreach (var house in HouseManager.manage.allHouses) {
                if (customItemsByID.ContainsKey(house.floor)) { UnloadFlooring(house); }
                if (customItemsByID.ContainsKey(house.wall)) { UnloadWallpaper(house); }
            }

            #endregion

            #region Vehicles

            var listOfVehicles = SaveLoad.saveOrLoad.vehiclesToSave;
            List<Vehicle> tmplistOfVehicles = new List<Vehicle>();
            foreach (var item in listOfVehicles) {
                if (customVehicleByID.ContainsKey(item.saveId)) {
                    tmplistOfVehicles.Add(item);
                    TRTools.Log($"Found a vehicle item {item.saveId}");
                }
            }
            UnloadVehicle(tmplistOfVehicles);

            #endregion

            #region ItemsOnGround

            var floatingItems = WorldManager.manageWorld.itemsOnGround;
            List<DroppedItem> tmpDroppedItems = new List<DroppedItem>();
            foreach (var item in floatingItems) {
                if (customItemsByID.ContainsKey(item.myItemId) && !item.saveDrop) { TRTools.Log($"Found Modded Item Not Scheduled For Save"); }
                if (customItemsByID.ContainsKey(item.myItemId) && item.saveDrop) {
                    tmpDroppedItems.Add(item);
                    TRTools.Log($"Found a dropped item {item.myItemId} | House: {item.inside}");
                }
            }
            if (tmpDroppedItems.Count > 0) UnloadFromDropped(tmpDroppedItems);

            #endregion

            #region Carryables

            var allCarryables = WorldManager.manageWorld.allCarriables;
            List<PickUpAndCarry> tmpCarryables = new List<PickUpAndCarry>();
            foreach (var carry in allCarryables) {
                if (customCarryableByID.ContainsKey(carry.prefabId)) {
                    tmpCarryables.Add(carry);
                    TRTools.Log($"Found a modded carryable");
                }
            }
            if (tmpCarryables.Count > 0) UnloadCarryable(tmpCarryables);

            #endregion

            #region Inventory

            // Unloads (and saves) items from the player's inventory
            for (var i = 0; i < Inventory.inv.invSlots.Length; i++) {
                var currentSlot = Inventory.inv.invSlots[i];
                if (customItemsByID.ContainsKey(currentSlot.itemNo)) {
                    TRTools.Log($"Found Custom Item: {customItemsByID[currentSlot.itemNo].invItem.itemName}");
                    savedItemData.Add(new ItemSaveData(customItemsByID[currentSlot.itemNo], i, currentSlot.stack));
                    currentSlot.updateSlotContentsAndRefresh(-1, 0);
                }
            }

            #endregion

            #region Stashes

            for (var j = 0; j < ContainerManager.manage.privateStashes.Count; j++) {
                var stash = ContainerManager.manage.privateStashes[j];
                for (var i = 0; i < stash.itemIds.Length; i++)

                    //TRTools.Log($"Found Item: {stash.itemIds[i]}");
                    if (customItemsByID.ContainsKey(stash.itemIds[i])) {
                        savedItemData.Add(new ItemSaveData(customItemsByID[stash.itemIds[i]], stash.itemStacks[i], j, i));
                        stash.itemIds[i] = -1;
                        stash.itemStacks[i] = 0;
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

            #region Mail

            var CurrentMail = MailManager.manage.mailInBox;
            var TomorrowsMail = MailManager.manage.tomorrowsLetters;
            var tmpMailBox = new List<Letter>();
            var tmpTomorrowMailBox = new List<Letter>();

            foreach (var letter in CurrentMail) {
                if (customItemsByID.ContainsKey(letter.itemOriginallAttached) || customItemsByID.ContainsKey(letter.itemAttached)) { tmpMailBox.Add(letter); }
            }
            foreach (var letter in TomorrowsMail) {
                if (customItemsByID.ContainsKey(letter.itemOriginallAttached) || customItemsByID.ContainsKey(letter.itemAttached)) { tmpTomorrowMailBox.Add(letter); }
            }
            if (tmpMailBox.Count > 0) UnloadFromMail(tmpMailBox, false);
            if (tmpTomorrowMailBox.Count > 0) UnloadFromMail(tmpTomorrowMailBox, true);

            #endregion

            #region Buried Items

            var buriedItems = BuriedManager.manage.allBuriedItems;
            List<BuriedItem> tmpBuriedItems = new List<BuriedItem>();
            foreach (var item in buriedItems) {
                if (customItemsByID.ContainsKey(item.itemId)) { tmpBuriedItems.Add(item); }
            }
            UnloadFromBuried(tmpBuriedItems);

            #endregion

            // Removed or possible planned content?
            //if (customItemsByID.ContainsKey(EquipWindow.equip.idolSlot.itemNo)) { }

            #region Tile/Paths

            // Go through every tile on the world map and look for objects that are custom items to unload them
            // This prevents corrupted save data if the player removes the mod and tries to load
            var tileTypeMap = WorldManager.manageWorld.tileTypeMap;
            for (var x = 0; x < tileTypeMap.GetLength(0); x++) {
                for (var y = 0; y < tileTypeMap.GetLength(1); y++)
                    if (tileTypeMap[x, y] > -1) {
                        if (WorldManager.manageWorld.tileTypes[tileTypeMap[x, y]].isPath) {
                            if (customTileTypeByID.ContainsKey(tileTypeMap[x, y])) {
                                UnloadFromTile(x, y, tileTypeMap[x, y]);
                                tileTypeMap[x, y] = 0;
                            }
                        }
                    }
            }

            #endregion

            #region World Objects & Chests

            var allObjects = WorldManager.manageWorld.allObjects;
            var tileMap = WorldManager.manageWorld.onTileMap;
            for (var x = 0; x < tileMap.GetLength(0); x++) {
                for (var y = 0; y < tileMap.GetLength(1); y++) {
                    if (tileMap[x, y] <= -1) continue;

                    #region OnTopOf Outside

                    var onTopOfTile = ItemOnTopManager.manage.getAllItemsOnTop(x, y, null);

                    for (var i = 0; i < onTopOfTile.Length; i++)
                        if (customTileObjectByID.ContainsKey(onTopOfTile[i].itemId))
                            UnloadWorldObjectOnTop(
                                onTopOfTile[i].itemId, x, y,
                                -1, -1,
                                onTopOfTile[i].itemRotation,
                                onTopOfTile[i].itemStatus,
                                true, onTopOfTile[i].onTopPosition
                            );

                    #endregion

                    #region Chests

                    // If the tile has a chest on it, save and unload custom items from the chest
                    if (allObjects[tileMap[x, y]].tileObjectChest) { UnloadFromChest(allObjects[tileMap[x, y]].tileObjectChest, x, y, -1, -1); }

                    #endregion

                    #region Onject On Floor Outside

                    // If the tile contains a custom world object, save and unload it
                    else if (customTileObjectByID.ContainsKey(tileMap[x, y])) {
                        var rotation = WorldManager.manageWorld.rotationMap[x, y];
                        var type = ItemSaveData.WorldObject.OnTile;
                        var bridgeLength = -1;
                        if (allObjects[tileMap[x, y]].tileObjectBridge) {
                            type = ItemSaveData.WorldObject.Bridge;
                            if (rotation == 1)
                                bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 0, -1);
                            else if (rotation == 2)
                                bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, -1);
                            else if (rotation == 3)
                                bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 0, 1);
                            else if (rotation == 4) bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 1);
                        }

                        UnloadWorldObject(tileMap[x, y], x, y, -1, -1, rotation, type, bridgeLength);

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
                        for (var i = 0; i < onTopOfTileInside.Length; i++)
                            if (customTileObjectByID.ContainsKey(onTopOfTileInside[i].getTileObjectId())) {
                                TRTools.Log($"Found {customTileObjectByID[onTopOfTileInside[i].getTileObjectId()].invItem.itemName}");

                                //(int tileObjectID, int OutsideX, int OutsideY, int insideX, int insideY, int rotation, int status, bool onTop = true, int onTopPos = -1) {
                                UnloadWorldObjectOnTop(
                                    onTopOfTileInside[i].itemId,
                                    onTopOfTileInside[i].sittingOnX,
                                    onTopOfTileInside[i].sittingOnY,
                                    x, y,
                                    onTopOfTileInside[i].itemRotation,
                                    onTopOfTileInside[i].itemStatus,
                                    true, onTopOfTileInside[i].onTopPosition
                                );
                            }

                        #endregion

                        for (var houseX = 0; houseX < houseDetails.houseMapOnTile.GetLength(0); houseX++) {
                            for (var houseY = 0; houseY < houseDetails.houseMapOnTile.GetLength(1); houseY++) {

                                // If nothing is on this tile, ignore it
                                var tileObjectID = houseDetails.houseMapOnTile[houseX, houseY];
                                if (tileObjectID <= 0) continue;

                                #region Chests Buildings

                                // If the object on this house tile is a chest, save and unload custom items from the chest
                                if (allObjects[tileObjectID].tileObjectChest) { UnloadFromChest(allObjects[tileObjectID].tileObjectChest, houseX, houseY, x, y); }

                                #endregion

                                #region Objects on Ground Inside

                                // If it's a custom item, save and unload it
                                else if (customTileObjectByID.ContainsKey(tileObjectID)) {
                                    var rotation = houseDetails.houseMapRotation[houseX, houseY];

                                    // houseX and houseY are the location inside the house where the object is
                                    // x, y are where the house location is
                                    UnloadWorldObject(tileObjectID, houseX, houseY, x, y, rotation);
                                }

                                #endregion

                            }
                        }
                    }
                }
            }

            #endregion

            DefaultSize();
            Data.SetValue("CurrentItemList", savedItemData);
            //if (_droppedItemsEnabled) Data.SetValue("DroppedItemList", savedDroppedItems);

            // We shoudl rename the save name here because its not accurate. I just didn't want to change it while I was testing code
            // since it will break the current save data I had. 
            Data.SetValue("CurrentVehicles", savedItemDataLate);

        }

        // Runs StoreCarry and then removes the carryable from the allCarriables list. 
        // The allCarriables list is used by the game to iterate through and save all of its contents. 
        // We remove it so that its not included in the save. This means that it will not delete the item from the world, but only remove from a list
        // You don't want to restore the carryable between sleeping and only when loading the save. 
        internal static void UnloadCarryable(List<PickUpAndCarry> toRemove) {
            foreach (var carry in toRemove) {
                TRTools.Log($"Attempting to remove: {carry.name}");
                savedItemDataLate.Add(new ItemSaveData().StoreCarry(carry, customCarryableByID[carry.prefabId].uniqueID));
                WorldManager.manageWorld.allCarriables.Remove(carry);
            }
        }

        // Wallpaper and Flooring are temporarily set to Rattan Wall/Floor. This should probably be updated to the basic wall/floor
        internal static void UnloadWallpaper(HouseDetails house) {
            savedItemData.Add(new ItemSaveData().StoreWallpaper(house, customItemsByID[house.wall].uniqueID));
            house.wall = 550; // Rattan Wall
        }

        internal static void UnloadFlooring(HouseDetails house) {
            savedItemData.Add(new ItemSaveData().StoreFlooring(house, customItemsByID[house.floor].uniqueID));
            house.floor = 546; // Rattan Floor
        }

        // This works similar to Carryables. It won't delete the item, so only restore it on the load of the save. 
        internal static void UnloadVehicle(List<Vehicle> toRemove) {
            foreach (var item in toRemove) {
                savedItemDataLate.Add(new ItemSaveData().StoreVehicle(item, customVehicleByID[item.saveId].uniqueID));
                SaveLoad.saveOrLoad.vehiclesToSave.Remove(item);
            }
        }

        // This iterates through all of the drops found that are modded and sets them to false. I am not positive this would get overwritten though...
        // I also patch loadDrops to restore these items, but it is current disabled by a bool. 
        // I also patch getDropsToSave to make sure everything that is modded is set to false. 
        internal static void UnloadFromDropped(List<DroppedItem> toRemove) {
            foreach (var item in toRemove) {
                savedDroppedItems.Add(new ItemSaveData().StoreDroppedItem(item, customItemsByID[item.myItemId].uniqueID));
                item.saveDrop = false;
            }
        }

        // This takes in a list of items, and removes it from the allBurriedItems list and sets the the tileMap to -1. 
        // We need to set to -1 because otherwise it will add new items into the ground. He has a method that if it can't
        // find the item in the ground and the tileMap is set to 30, then put a random item inside the ground. 
        internal static void UnloadFromBuried(List<BuriedItem> toRemove) {
            foreach (var item in toRemove) {
                savedItemData.Add(new ItemSaveData().StoreBuriedItem(item, customItemsByID[item.itemId].uniqueID));
                BuriedManager.manage.allBuriedItems.Remove(item);
                WorldManager.manageWorld.onTileMap[item.xPos, item.yPos] = -1;
            }
        }

        // This goes through the list and unloads each mail by removing it form the appropriate list. 
        internal static void UnloadFromMail(List<Letter> toRemove, bool tomorrow = false) {

            if (!tomorrow) {
                foreach (var letter in toRemove) {
                    savedItemData.Add(new ItemSaveData().StoreLetter(letter, tomorrow, customItemsByID[letter.itemAttached].uniqueID));
                    MailManager.manage.mailInBox.Remove(letter);
                }
            }
            else {
                foreach (var letter in toRemove) {
                    savedItemData.Add(new ItemSaveData().StoreLetter(letter, tomorrow, customItemsByID[letter.itemAttached].uniqueID));
                    MailManager.manage.tomorrowsLetters.Remove(letter);
                }
            }
        }

        // This takes the tile and unloads the path. 
        internal static void UnloadFromTile(int objectXPos, int objectYPos, int tileType) => savedItemData.Add(new ItemSaveData(customTileTypeByID[tileType], ItemSaveData.WorldObject.Path, objectXPos, objectYPos, tileType));

        //This will iterate through the chest given (by location) and remove any times that are modded
        internal static void UnloadFromChest(ChestPlaceable chestPlaceable, int objectXPos, int objectYPos, int houseXPos, int houseYPos) {
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            chestPlaceable.checkIfEmpty(objectXPos, objectYPos, houseDetails);
            var chest = ContainerManager.manage.activeChests.First(p => p.xPos == objectXPos && p.yPos == objectYPos && p.inside == (houseDetails != null));
            for (var z = 0; z < chest.itemIds.Length; z++)
                if (customItemsByID.ContainsKey(chest.itemIds[z])) {
                    savedItemData.Add(new ItemSaveData(customItemsByID[chest.itemIds[z]], z, chest.itemStacks[z], objectXPos, objectYPos, houseXPos, houseYPos));
                    ContainerManager.manage.changeSlotInChest(objectXPos, objectYPos, z, -1, 0, houseDetails);
                }
        }

        // Unload all tile objects in the world (furniture, traps, furnances, etc). I didn't test furnances, but dont see why it wouldn't work. 
        // This does both inside the house and outside the house. 
        internal static void UnloadWorldObject(int tileObjectID, int objectXPos, int objectYPos, int houseXPos, int houseYPos, int rotation, ItemSaveData.WorldObject type = ItemSaveData.WorldObject.OnTile, int bridgeLength = -1) {
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            savedItemData.Add(new ItemSaveData(customTileObjectByID[tileObjectID], objectXPos, objectYPos, rotation, type, houseXPos, houseYPos, bridgeLength: bridgeLength));
            if (houseDetails != null) {
                customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObjectInside(objectXPos, objectYPos, rotation, houseDetails);
                houseDetails.houseMapOnTile[objectXPos, objectYPos] = -1;
                houseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = -1;
                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);

                if (house && house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture) {
                    house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture.updateOnTileStatus(objectXPos, objectYPos, houseDetails);
                    house.refreshHouseTiles();
                }

            }
            else {
                customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObject(objectXPos, objectYPos, rotation);
                WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = -1;
                WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = -1;
                WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos);
                NetworkNavMesh.nav.updateChunkInUse();
            }
        }

        // This gets rid of all of the items on Top. This shoudl be done BEFORE the other tile objects otherwise we risk a floating item and probably an error. 
        internal static void UnloadWorldObjectOnTop(int tileObjectID, int objectXPos, int objectYPos, int HouseXPos, int HouseYPos, int rotation, int status, bool onTop = true, int onTopPos = -1) {
            var houseDetails = HouseXPos == -1 ? null : HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos);
            savedItemData.Add(
                new ItemSaveData(
                    customTileObjectByID[tileObjectID], objectXPos, objectYPos, rotation, ItemSaveData.WorldObject.OnTop, HouseXPos, HouseYPos, status,
                    onTopPos
                )
            );
            if (houseDetails != null) {
                ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, objectXPos, objectYPos, HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos)));
                var displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(HouseXPos, HouseYPos);
                if (displayPlayerHouseTiles && displayPlayerHouseTiles.tileObjectsInHouse[objectXPos, objectYPos]) displayPlayerHouseTiles.tileObjectsInHouse[objectXPos, objectYPos].checkOnTopInside(objectXPos, objectYPos, HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos));
            }
            else {
                ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, objectXPos, objectYPos, null));
                WorldManager.manageWorld.unlockClientTile(objectXPos, objectYPos);
                WorldManager.manageWorld.refreshAllChunksInUse(objectXPos, objectYPos);

            }
        }

        internal enum ToLoad { Main, AfterNetwork, All }

        // TODO: Reaname Current Vehicles to be generic for all after network loaded items
        // This loads all mod save data and has a flag set so we can load all at once and/or load the specific data we need at the time. 
        // We probably don't need All. (or we can probably only use All..? There was a reason I didnt use it but I cant remember why)
        internal static void LoadModSavedData(ToLoad toLoad, bool bypass = false) {
            switch (toLoad) {
                case ToLoad.All:
                    savedItemData = (List<ItemSaveData>)Data.GetValue("CurrentItemList", new List<ItemSaveData>());
                    savedItemData = savedItemData.OrderBy(i => i.type == ItemSaveData.WorldObject.OnTop).ToList();
                    savedItemDataLate = (List<ItemSaveData>)Data.GetValue("CurrentVehicles", new List<ItemSaveData>());
                    break;

                case ToLoad.Main:
                    savedItemData = (List<ItemSaveData>)Data.GetValue("CurrentItemList", new List<ItemSaveData>());
                    savedItemData = savedItemData.OrderBy(i => i.type == ItemSaveData.WorldObject.OnTop).ToList();
                    break;

                case ToLoad.AfterNetwork:
                    savedItemDataLate = (List<ItemSaveData>)Data.GetValue("CurrentVehicles", new List<ItemSaveData>());
                    break;
            }
        }

        // Method for loading stuff after the world has loaded for thigns like carryables, dropped items, and vehicles. 
        internal static void LoadCustomItemPostLoad() {

            if (NetworkMapSharer.share.localChar) return;

            TRItems.LoadModSavedData(ToLoad.AfterNetwork, true);

            foreach (var item in savedItemDataLate) {
                if (!customItems.TryGetValue(item.uniqueID, out var customItem)) continue;

                switch (item.location) {
                    // DOne in LoadChangerPatch
                    case ItemSaveData.ItemLocations.Vehicle:
                        item.vehicle.Restore();
                        break;

                    case ItemSaveData.ItemLocations.Carry:
                        item.carry.Restore();
                        break;
                }
            }
        }

        // Method for loading everything else
        internal static void LoadCustomItems() {
            RestoreModSize();
            TRTools.Log("Re-adding Items");
            LoadModSavedData(ToLoad.Main, true);

            if (savedItemData.Count > 0) {
                var tmpHouseDetails = new HouseDetails();
                tmpHouseDetails = null;
                foreach (var item in savedItemData) {
                    if (!customItems.TryGetValue(item.uniqueID, out var customItem)) continue;

                    switch (item.location) {

                        // Straightforward....
                        case ItemSaveData.ItemLocations.Inventory:
                            Inventory.inv.invSlots[item.slotNo].updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                            break;

                        // Straight forward since we alrady did the Craft From Storage Mod. 
                        case ItemSaveData.ItemLocations.Chest:
                            tmpHouseDetails = item.HouseXPos == -1 ? null : HouseManager.manage.getHouseInfo(item.HouseXPos, item.HouseYPos);
                            ContainerManager.manage.changeSlotInChest(item.ObjectXPos, item.ObjectYPos, item.slotNo, customItem.invItem.getItemId(), item.stackSize, tmpHouseDetails);
                            break;

                        // Takes the equipment slot and runs the change item on it. This has been tested now and works. 
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
                        // Goes through the World Tiles and separates them into various types. 
                        case ItemSaveData.ItemLocations.World:
                            switch (item.type) {
                                // OnTile is for anytihng directly on a tile
                                case ItemSaveData.WorldObject.OnTile:
                                    tmpHouseDetails = item.HouseXPos == -1 ? null : HouseManager.manage.getHouseInfo(item.HouseXPos, item.HouseYPos);
                                    if (tmpHouseDetails != null) {
                                        customItem.tileObject.placeMultiTiledObjectInside(item.ObjectXPos, item.ObjectYPos, item.rotation, tmpHouseDetails);

                                        tmpHouseDetails.houseMapOnTile[item.ObjectXPos, item.ObjectYPos] = customItem.tileObject.tileObjectId;
                                        tmpHouseDetails.houseMapOnTileStatus[item.ObjectXPos, item.ObjectYPos] = 0;

                                        // Must check if localChar is null because it will fail on first load since HouseManager doesn't exist
                                        if (NetworkMapSharer.share.localChar) {
                                            var house = HouseManager.manage.findHousesOnDisplay(item.HouseXPos, item.HouseYPos);
                                            house.refreshHouseTiles();
                                        }

                                    }
                                    else {
                                        customItem.tileObject.placeMultiTiledObject(item.ObjectXPos, item.ObjectYPos, item.rotation);
                                        WorldManager.manageWorld.onTileMap[item.ObjectXPos, item.ObjectYPos] = customItem.tileObject.tileObjectId;
                                        WorldManager.manageWorld.onTileStatusMap[item.ObjectXPos, item.ObjectYPos] = 0;
                                        WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(item.ObjectXPos, item.ObjectYPos);

                                        //NetworkNavMesh.nav.updateChunkInUse();
                                        WorldManager.manageWorld.unlockClientTile(item.ObjectXPos, item.ObjectYPos);
                                    }
                                    break;

                                // OnTop is for anything that needs to be loaded on top of another item
                                // TODO: we still need to test removing/restoring from a custom table with a custom item
                                // TODO: we should add a condition of saving non modded items in situations where a non modded item is on top of a modded one
                                case ItemSaveData.WorldObject.OnTop:
                                    tmpHouseDetails = item.HouseXPos == -1 ? null : HouseManager.manage.getHouseInfo(item.HouseXPos, item.HouseYPos);
                                    ItemOnTopManager.manage.placeItemOnTop(customItem.tileObject.tileObjectId, item.onTopPos, item.status, item.rotation, item.ObjectXPos, item.ObjectYPos, tmpHouseDetails);
                                    WorldManager.manageWorld.unlockClientTile(item.ObjectXPos, item.ObjectYPos);
                                    WorldManager.manageWorld.refreshAllChunksInUse(item.ObjectXPos, item.ObjectYPos);

                                    // Must check if localChar is null because it will fail on first load since HouseManager doesn't exist
                                    if (NetworkMapSharer.share.localChar) {
                                        var house = HouseManager.manage.findHousesOnDisplay(item.HouseXPos, item.HouseYPos);
                                        house.refreshHouseTiles();
                                    }
                                    break;

                                // TODO: Handle these below cases

                                // Restores the bridges in the world. 
                                case ItemSaveData.WorldObject.Bridge:
                                    TRTools.Log($"Starting Position: ({item.ObjectXPos}, {item.ObjectYPos}) | Rotation: {item.rotation} | Length: {item.bridgeLength}");
                                    customItem.tileObject.placeBridgeTiledObject(item.ObjectXPos, item.ObjectYPos, item.rotation, item.bridgeLength);
                                    WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(item.ObjectXPos, item.ObjectYPos);
                                    WorldManager.manageWorld.unlockClientTile(item.ObjectXPos, item.ObjectYPos);

                                    break;

                                // Restores the paths (cemenet path, etc)
                                // We set the TileTypeMpa to the tileType ID
                                case ItemSaveData.WorldObject.Path:
                                    WorldManager.manageWorld.tileTypeMap[item.ObjectXPos, item.ObjectYPos] = item.tileType;
                                    WorldManager.manageWorld.refreshAllChunksInUse(item.ObjectXPos, item.ObjectYPos);
                                    break;
                            }
                            break;

                        // This restores items from today and tomorrow's mail
                        case ItemSaveData.ItemLocations.Letter:
                            if (!item.letter.tomorrow) { MailManager.manage.mailInBox.Add(item.letter.Restore()); }
                            else { MailManager.manage.tomorrowsLetters.Add(item.letter.Restore()); }
                            break;

                        // Buried Items, We have to set the TileMap to 30 and then restore buried items otherwise the item wont be there. 
                        case ItemSaveData.ItemLocations.Buried:
                            WorldManager.manageWorld.onTileMap[item.buriedItem.xP, item.buriedItem.yP] = 30;
                            BuriedManager.manage.allBuriedItems.Add(item.buriedItem.Restore());
                            break;

                        // Restores the homefloor and home walls
                        case ItemSaveData.ItemLocations.HomeFloor:
                            item.flooring.Restore();
                            break;

                        case ItemSaveData.ItemLocations.HomeWall:
                            item.wallpaper.Restore();
                            break;

                    }
                }
            }
        }
    }

    public class TRCustomItem {
        
        public string uniqueID;

        // TODO: Implement events
        public delegate void TileObjectEvent();
        public TileObjectEvent interactEvent;

        public InventoryItem invItem;
        public TileObject tileObject;
        public TileObjectSettings tileObjectSettings;
        public TileTypes tileTypes;
        public Vehicle vehicle;
        public PickUpAndCarry carryable;

        internal static TRCustomItem Create(string assetBundlePath) {

            TRTools.Log("Attemping Load: Asset Bundle.");
            var newItem = new TRCustomItem();
            var bundle = TRAssets.LoadBundle(assetBundlePath);
            TRTools.Log($"Loaded: Asset Bundle -- {bundle}.");

            var AllAssets = bundle.LoadAllAssets<GameObject>();
            for (var i = 0; i < AllAssets.Length; i++) {
                if (newItem.invItem == null) { newItem.invItem = AllAssets[i].GetComponent<InventoryItem>(); }
                if (newItem.tileObject == null) { newItem.tileObject = AllAssets[i].GetComponent<TileObject>(); }
                if (newItem.tileObjectSettings == null) { newItem.tileObjectSettings = AllAssets[i].GetComponent<TileObjectSettings>(); }
                if (newItem.tileTypes == null) { newItem.tileTypes = AllAssets[i].GetComponent<TileTypes>(); }
                if (newItem.vehicle == null) { newItem.vehicle = AllAssets[i].GetComponent<Vehicle>(); }
                if (newItem.carryable == null) { newItem.carryable = AllAssets[i].GetComponent<PickUpAndCarry>(); }
            }

            bundle.Unload(false);
            return newItem;

        }
        
    }

}
