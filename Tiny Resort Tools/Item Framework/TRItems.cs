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
    
    /* TODO
     HIGH PRIORITY
        * John: Add a chat command for giving yourself a custom item.
        * Add try catches when loading or unloading any item to prevent one badly modded item breaking everything.
        * If a custom item can't be loaded in, add it to a Lost and Found save file. 

     MID PRIORITY
        * Add ability to put custom items on enemy drop tables.
        * Add ability to put custom items in mine chest loot tables.
        * Add ability to put custom items in loot that's buried underground, both in boxes and on its own.
        * Add ability to add a custom item recipe to Franklyn's crafting shop.
        * Add ability to make a recipe for a custom item and to give that recipe to the player.
        * Add ability to add custom item to items NPCs give you, as well as to the recycle bin.
        * Handle case where a non-modded item is on top of modded furniture. 

     LOW PRIORITY
        * Stephen: Quick creating paths, house floors and wallpapers.
        * Expand mail system so that mod authors can be make highly custom letters.
        * Add recovering items from the lost and found if mods are reinstalled. 
     
     */

    /// <summary>Tools for working with the Dinkum inventory.</summary>
    public class TRItems {

        #region Vanilla Item Tools

        internal static readonly Dictionary<int, InventoryItem> itemDetails = new Dictionary<int, InventoryItem>();

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

        #endregion

        internal static TRModData Data;
        internal static readonly Dictionary<string, TRCustomItem> customItems = new Dictionary<string, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customItemsByItemID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customTileObjectByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customTileTypeByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customVehicleByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customCarryableByID = new Dictionary<int, TRCustomItem>();

        internal static bool customItemsInitialized;

        private static List<InventoryItem> allItemsVanilla;
        private static List<InventoryItem> allItemsFull;
        private static List<TileObject> tileObjectsVanilla;
        private static List<TileObject> tileObjectsFull;
        private static List<TileObjectSettings> tileObjectSettingsVanilla;
        private static List<TileObjectSettings> tileObjectSettingsFull;
        private static List<TileTypes> tileTypesVanilla;
        private static List<TileTypes> tileTypesFull;
        private static List<GameObject> vehiclePrefabsVanilla;
        private static List<GameObject> vehiclePrefabsFull;
        private static List<GameObject> carryablePrefabsVanilla;
        private static List<GameObject> carryablePrefabsFull;
        private static List<bool> CatalogueVanilla;

        internal static void Initialize() {
            Data = TRData.Subscribe("TR.CustomItems");
            TRData.cleanDataEvent += UnloadCustomItems;
            TRData.initialLoadEvent += LoadCustomMovables;
            TRData.injectDataEvent += LoadCustomItems;
        }

        internal static TRCustomItem AddCustomItem(TRPlugin plugin, string assetBundlePath, string uniqueItemID) {

            if (customItemsInitialized) {
                TRTools.LogError("Mod attempted to load a new item after item initialization. You need to load new items in your Awake() method!");
                return null;
            }

            customItems[plugin.nexusID + uniqueItemID] = TRCustomItem.Create(assetBundlePath);
            customItems[plugin.nexusID + uniqueItemID].customItemID = plugin.nexusID + uniqueItemID;
            return customItems[plugin.nexusID + uniqueItemID];

        }

        // Adds a new custom item to the dictionary but allows it to be created somewhere else
        internal static void AddCustomItem(TRCustomItem item, string uniqueID) {
            item.customItemID = uniqueID;
            customItems[uniqueID] = item;
        }

        // Resize the array depending on the number of modded items added
        // Ignore modded items saved if it doesnt exist in customItems
        internal static void ManageAllItemArray() {

            // Saves the default arrays for existing item lists
            allItemsVanilla = Inventory.inv.allItems.ToList();
            tileObjectsVanilla = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsVanilla = WorldManager.manageWorld.allObjectSettings.ToList();
            vehiclePrefabsVanilla = SaveLoad.saveOrLoad.vehiclePrefabs.ToList();
            carryablePrefabsVanilla = SaveLoad.saveOrLoad.carryablePrefabs.ToList();
            tileTypesVanilla = WorldManager.manageWorld.tileTypes.ToList();
            CatalogueVanilla = CatalogueManager.manage.collectedItem.ToList();

            // Get existing item lists
            allItemsFull = Inventory.inv.allItems.ToList();
            tileObjectsFull = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsFull = WorldManager.manageWorld.allObjectSettings.ToList();
            tileTypesFull = WorldManager.manageWorld.tileTypes.ToList();
            vehiclePrefabsFull = SaveLoad.saveOrLoad.vehiclePrefabs.ToList();
            carryablePrefabsFull = SaveLoad.saveOrLoad.carryablePrefabs.ToList();

            // Add custom items to existing item lists
            foreach (var item in customItems) {

                // Add custom paths
                if (item.Value.tileTypes) {
                    if (item.Value.tileTypes.isPath) {
                        tileTypesFull.Add(item.Value.tileTypes);
                        item.Value.invItem.placeableTileType = tileTypesFull.Count - 1;
                        customTileTypeByID[tileTypesFull.Count - 1] = item.Value;
                    }
                }

                // Add custom inventory item
                if (item.Value.invItem) {
                    allItemsFull.Add(item.Value.invItem);
                    item.Value.invItem.setItemId(allItemsFull.Count - 1);
                    customItemsByItemID[allItemsFull.Count - 1] = item.Value;
                }

                // Add tile object and settings if they exist
                if (item.Value.tileObject) {
                    tileObjectsFull.Add(item.Value.tileObject);
                    item.Value.tileObject.tileObjectId = tileObjectsFull.Count - 1;
                    tileObjectSettingsFull.Add(item.Value.tileObjectSettings);
                    item.Value.tileObjectSettings.tileObjectId = tileObjectSettingsFull.Count - 1;
                    customTileObjectByID[tileObjectsFull.Count - 1] = item.Value;
                }

                // Add custom vehicles
                if (item.Value.vehicle) {
                    vehiclePrefabsFull.Add(item.Value.invItem.spawnPlaceable);
                    item.Value.vehicle.saveId = vehiclePrefabsFull.Count - 1;
                    customVehicleByID[vehiclePrefabsFull.Count - 1] = item.Value;
                }

                // Add custom carryable items
                if (item.Value.carryable) {
                    carryablePrefabsFull.Add(item.Value.carryable.gameObject);
                    item.Value.carryable.prefabId = carryablePrefabsFull.Count - 1;
                    customCarryableByID[carryablePrefabsFull.Count - 1] = item.Value;
                }

            }

            // Set the game's arrays to match the new lists that include the custom items
            ModTheArrays();

            // TODO:
            // Resize and/or add mod save data of if they have obtained it or not
            // In a pre-save event, go through all items in catalogue array past vanilla items and check which are true and set them. 
            // set on inject data event (after saving and loading)
            //CatalogueManager.manage.collectedItem = new bool[Inventory.inv.allItems.Length];

            //CheatScript.cheat.cheatButtons = new GameObject[Inventory.inv.allItems.Length];
            // This works and doesn't get saved so oh well
            var cheatButton = typeof(CheatScript).GetField("cheatButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            cheatButton?.SetValue(CheatScript.cheat, new GameObject[Inventory.inv.allItems.Length]);

            customItemsInitialized = true;
            InitializeItemDetails();

        }

        // This is used to restore the modded items into the lists after saving. It just takes the list of items we have
        // and adds them to the games arrays. 
        internal static void ModTheArrays() {
            Inventory.inv.allItems = allItemsFull.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectsFull.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsFull.ToArray();
            Array.Resize(ref CatalogueManager.manage.collectedItem, Inventory.inv.allItems.Length);
            SaveLoad.saveOrLoad.vehiclePrefabs = vehiclePrefabsFull.ToArray();
            SaveLoad.saveOrLoad.carryablePrefabs = carryablePrefabsFull.ToArray();
            WorldManager.manageWorld.tileTypes = tileTypesFull.ToArray();
        }

        // Restores the vanilla versions of item lists to avoid custom items leaking into save data and corrupting files
        internal static void UnmodTheArrays() {
            TRTools.Log($"Items: {Inventory.inv.allItems.Length} | Objects: {WorldManager.manageWorld.allObjects.Length} & {WorldManager.manageWorld.allObjectSettings.Length} | Catalogue: {CatalogueManager.manage.collectedItem.Length}");
            Inventory.inv.allItems = allItemsVanilla.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectsVanilla.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsVanilla.ToArray();
            CatalogueManager.manage.collectedItem = CatalogueVanilla.ToArray();
            Array.Resize(ref CatalogueManager.manage.collectedItem, CatalogueVanilla.Count);
            SaveLoad.saveOrLoad.vehiclePrefabs = vehiclePrefabsVanilla.ToArray();
            SaveLoad.saveOrLoad.carryablePrefabs = carryablePrefabsVanilla.ToArray();
            WorldManager.manageWorld.tileTypes = tileTypesVanilla.ToArray();
            TRTools.Log($"Items: {Inventory.inv.allItems.Length} | Objects: {WorldManager.manageWorld.allObjects.Length} & {WorldManager.manageWorld.allObjectSettings.Length} | Catalogue: {CatalogueManager.manage.collectedItem.Length}");
        }

        // One large method to go through all modded items in the game and remove them. 
        // This might be worth breaking up. Specifically, anything that is done before the Loops of the tiles can be put into
        // their own methods. 
        internal static void UnloadCustomItems() {

            TRTools.Log("Removing Items");

            // Clears all item data lists
            InvItemData.all.Clear(); 
            ChestData.all.Clear();
            EquipData.all.Clear();
            LetterData.all.Clear();
            StashData.all.Clear();
            HouseData.all.Clear();
            VehicleData.all.Clear();
            CarryableData.all.Clear();
            ObjectData.all.Clear();
            ObjectTopData.all.Clear();
            BridgeData.all.Clear();
            PathData.all.Clear();
            BuriedObjectData.all.Clear();

            #region House Wallpaper/Flooring

            // This goes through the allHouse array, finds any wall/floor that is custom and unloads them. 
            foreach (var house in HouseManager.manage.allHouses) {
                if (customItemsByItemID.ContainsKey(house.floor)) { HouseData.Save(house, false); }
                if (customItemsByItemID.ContainsKey(house.wall)) { HouseData.Save(house, true); }
            }

            #endregion

            #region Movable Items (Vehicles, Carryables)

            // Unloads and saves all vehicles out in the world
            var vehicles = new List<Vehicle>(SaveLoad.saveOrLoad.vehiclesToSave);
            foreach (var vehicle in vehicles) {
                if (customVehicleByID.ContainsKey(vehicle.saveId)) {
                    TRTools.Log($"Found a vehicle item {vehicle.saveId}");
                    VehicleData.Save(vehicle);
                }
            }

            // Unloads and saves all carrayable items out in the world
            var carryables = new List<PickUpAndCarry>(WorldManager.manageWorld.allCarriables);
            foreach (var carryable in carryables) {
                if (customCarryableByID.ContainsKey(carryable.prefabId)) {
                    TRTools.Log($"Found a modded carryable");
                    CarryableData.Save(carryable);
                }
            }

            #endregion

            #region Inventory, Equipment Slots, Stashes, & Mail

            // Unloads (and saves) items from the player's inventory
            for (var i = 0; i < Inventory.inv.invSlots.Length; i++) {
                if (customItemsByItemID.ContainsKey(Inventory.inv.invSlots[i].itemNo)) {
                    TRTools.Log($"Found Custom Item: {customItemsByItemID[Inventory.inv.invSlots[i].itemNo].invItem.itemName}");
                    InvItemData.Save(i, Inventory.inv.invSlots[i].stack);
                }
            }

            // Unloads (and saves) items from the player's stash
            for (var j = 0; j < ContainerManager.manage.privateStashes.Count; j++) {
                for (var i = 0; i < ContainerManager.manage.privateStashes[j].itemIds.Length; i++) {
                    TRTools.Log($"Found Item: {ContainerManager.manage.privateStashes[j].itemIds[i]}");
                    if (customItemsByItemID.ContainsKey(ContainerManager.manage.privateStashes[j].itemIds[i])) { StashData.Save(ContainerManager.manage.privateStashes[j].itemStacks[i], j, i); }
                }
            }

            // Unloads and saves all equipped clothing
            if (customItemsByItemID.ContainsKey(EquipWindow.equip.hatSlot.itemNo)) { EquipData.Save(EquipWindow.equip.hatSlot.stack, EquipData.EquipLocations.Hat); }
            if (customItemsByItemID.ContainsKey(EquipWindow.equip.faceSlot.itemNo)) { EquipData.Save(EquipWindow.equip.faceSlot.stack, EquipData.EquipLocations.Face); }
            if (customItemsByItemID.ContainsKey(EquipWindow.equip.shirtSlot.itemNo)) { EquipData.Save(EquipWindow.equip.shirtSlot.stack, EquipData.EquipLocations.Shirt); }
            if (customItemsByItemID.ContainsKey(EquipWindow.equip.pantsSlot.itemNo)) { EquipData.Save(EquipWindow.equip.pantsSlot.stack, EquipData.EquipLocations.Pants); }
            if (customItemsByItemID.ContainsKey(EquipWindow.equip.shoeSlot.itemNo)) { EquipData.Save(EquipWindow.equip.shoeSlot.stack, EquipData.EquipLocations.Shoes); }

            // Removes mail from the mail box if it contains a custom item
            var inMailBox = new List<Letter>(MailManager.manage.mailInBox);
            foreach (var letter in inMailBox) {
                if (customItemsByItemID.ContainsKey(letter.itemOriginallAttached) || customItemsByItemID.ContainsKey(letter.itemAttached)) LetterData.Save(letter, false);
            }

            // Removes mail that would be sent the next night if it contains a custom item
            var tomorrowMail = new List<Letter>(MailManager.manage.tomorrowsLetters);
            foreach (var letter in tomorrowMail) {
                if (customItemsByItemID.ContainsKey(letter.itemOriginallAttached) || customItemsByItemID.ContainsKey(letter.itemAttached)) LetterData.Save(letter, true);
            }

            #endregion

            #region Buried Items & Paths

            // Unload and save all custom buried items
            var buriedItems = new List<BuriedItem>(BuriedManager.manage.allBuriedItems);
            foreach (var item in buriedItems) {
                if (customItemsByItemID.ContainsKey(item.itemId)) BuriedObjectData.Save(item);
            }

            // Unload and save all custom paths
            var tileTypeMap = WorldManager.manageWorld.tileTypeMap;
            for (var x = 0; x < tileTypeMap.GetLength(0); x++) {
                for (var y = 0; y < tileTypeMap.GetLength(1); y++)
                    if (tileTypeMap[x, y] > -1 && WorldManager.manageWorld.tileTypes[tileTypeMap[x, y]].isPath && customTileTypeByID.ContainsKey(tileTypeMap[x, y])) {
                        TRTools.Log($"Found Mod Tile: {tileTypeMap[x, y]} | ({x},{y})");
                        PathData.Save(tileTypeMap[x, y], x, y);
                    }
            }

            #endregion

            #region World Objects & Chests

            // Go through every tile in the overworld and in the house to unload all custom items
            var allObjects = WorldManager.manageWorld.allObjects;
            var onTileMap = WorldManager.manageWorld.onTileMap;
            for (var x = 0; x < onTileMap.GetLength(0); x++) {
                for (var y = 0; y < onTileMap.GetLength(1); y++) {

                    // If the tile is empty, ignore it
                    if (onTileMap[x, y] <= -1) continue;

                    #region Items on Top of Others (NOT in a house)

                    // Removes items that are on top of other items.
                    // Does this before removing ground items to prevent issues
                    // TODO: Test stacked custom items
                    var onTopOfTile = ItemOnTopManager.manage.getAllItemsOnTop(x, y, null);
                    for (var i = 0; i < onTopOfTile.Length; i++)
                        if (customTileObjectByID.ContainsKey(onTopOfTile[i].itemId))
                            ObjectTopData.Save(onTopOfTile[i].itemId, x, y, onTopOfTile[i].itemRotation, -1, -1, onTopOfTile[i].itemStatus, onTopOfTile[i].onTopPosition);

                    #endregion

                    #region Chests (NOT in a house)

                    // If the tile has a chest on it, save and unload custom items from the chest
                    // TODO: Make also unload custom chest itself, if it is custom
                    // TODO: It's possible that we need to also remove vanilla items from custom chests
                    // Chest.cs -> Data structure that holds the items it has and stuff
                    // ChestPlaceable.cs -> Actual object placed in the world.
                    if (allObjects[onTileMap[x, y]].tileObjectChest) { ChestData.Save(allObjects[onTileMap[x, y]].tileObjectChest, x, y, -1, -1); }

                    #endregion

                    #region World Object & Bridges (NOT in a house)

                    // If the tile contains a custom world object, unload and save it
                    else if (customTileObjectByID.ContainsKey(onTileMap[x, y])) {

                        // If not a bridge, save as an overworld object
                        var rotation = WorldManager.manageWorld.rotationMap[x, y];
                        if (!allObjects[onTileMap[x, y]].tileObjectBridge) { ObjectData.Save(onTileMap[x, y], x, y, rotation, -1, -1); }

                        // If it is a bridge, find the length and save the bridge
                        else {
                            var bridgeLength = -1;
                            if (rotation == 1)
                                bridgeLength = customTileObjectByID[onTileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 0, -1);
                            else if (rotation == 2)
                                bridgeLength = customTileObjectByID[onTileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, -1);
                            else if (rotation == 3)
                                bridgeLength = customTileObjectByID[onTileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 0, 1);
                            else if (rotation == 4) bridgeLength = customTileObjectByID[onTileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 1);
                            BridgeData.Save(onTileMap[x, y], x, y, rotation, bridgeLength);
                        }

                    }

                    #endregion

                    // Check for objects within houses
                    else if (allObjects[onTileMap[x, y]].displayPlayerHouseTiles) {
                        var houseDetails = HouseManager.manage.getHouseInfo(x, y);

                        #region Items on Top of Others (INSIDE a house)

                        // Removes custom items that are on top of objects inside a house
                        var onTopOfTileInside = ItemOnTopManager.manage.getAllItemsOnTopInHouse(houseDetails);
                        for (var i = 0; i < onTopOfTileInside.Length; i++) {
                            if (customTileObjectByID.ContainsKey(onTopOfTileInside[i].getTileObjectId())) {
                                TRTools.Log($"Found {customTileObjectByID[onTopOfTileInside[i].getTileObjectId()].invItem.itemName}");
                                ObjectTopData.Save(
                                    onTopOfTileInside[i].itemId, onTopOfTileInside[i].sittingOnX, onTopOfTileInside[i].sittingOnY,
                                    onTopOfTileInside[i].itemRotation, x, y, onTopOfTileInside[i].itemStatus, onTopOfTileInside[i].onTopPosition
                                );
                            }
                        }

                        #endregion

                        // Checks every tile inside a house to find custom objects
                        for (var houseTileX = 0; houseTileX < houseDetails.houseMapOnTile.GetLength(0); houseTileX++) {
                            for (var houseTileY = 0; houseTileY < houseDetails.houseMapOnTile.GetLength(1); houseTileY++) {

                                // If nothing is on this tile, ignore it
                                var tileObjectID = houseDetails.houseMapOnTile[houseTileX, houseTileY];
                                if (tileObjectID <= 0) continue;

                                #region Chests (INSIDE a house)

                                // If the object on this house tile is a chest, save and unload custom items from the chest
                                // TODO: Make also unload custom chest itself, if it is custom
                                // TODO: It's possible that we need to also remove vanilla items from custom chests
                                // Chest.cs -> Data structure that holds the items it has and stuff
                                // ChestPlaceable.cs -> Actual object placed in the world.
                                if (allObjects[tileObjectID].tileObjectChest) { ChestData.Save(allObjects[tileObjectID].tileObjectChest, houseTileX, houseTileY, x, y); }

                                #endregion

                                #region World Objects (INSIDE a house)

                                // If it's a custom item, save and unload it
                                else if (customTileObjectByID.ContainsKey(tileObjectID)) ObjectData.Save(tileObjectID, houseTileX, houseTileY, houseDetails.houseMapRotation[houseTileX, houseTileY], x, y);

                                #endregion

                            }
                        }
                    }
                }
            }

            #endregion

            // Saves all the new data
            Data.SetValue("InvItemData", InvItemData.all);
            Data.SetValue("ChestData", ChestData.all);
            Data.SetValue("EquipData", EquipData.all);
            Data.SetValue("LetterData", LetterData.all);
            Data.SetValue("StashData", StashData.all);
            Data.SetValue("HouseData", HouseData.all);
            Data.SetValue("VehicleData", VehicleData.all);
            Data.SetValue("CarryableData", CarryableData.all);
            Data.SetValue("ObjectData", ObjectData.all);
            Data.SetValue("ObjectTopData", ObjectTopData.all);
            Data.SetValue("BridgeData", BridgeData.all);
            Data.SetValue("PathData", PathData.all);
            Data.SetValue("BuriedObjectData", BuriedObjectData.all);
            TRTools.Log($"Saving InvItemData: {InvItemData.all.Count}");
            TRTools.Log($"Saving ChestData: {ChestData.all.Count}");
            TRTools.Log($"Saving EquipData: {EquipData.all.Count}");
            TRTools.Log($"Saving LetterData: {LetterData.all.Count}");
            TRTools.Log($"Saving StashData: {StashData.all.Count}");
            TRTools.Log($"Saving HouseData: {HouseData.all.Count}");
            TRTools.Log($"Saving VehicleData: {VehicleData.all.Count}");
            TRTools.Log($"Saving CarryableData: {CarryableData.all.Count}");
            TRTools.Log($"Saving ObjectData: {ObjectData.all.Count}");
            TRTools.Log($"Saving ObjectTopData: {ObjectTopData.all.Count}");
            TRTools.Log($"Saving BridgeData: {BridgeData.all.Count}");
            TRTools.Log($"Saving PathData: {PathData.all.Count}");
            TRTools.Log($"Saving BuriedObjectData: {BuriedObjectData.all.Count}");

            // Goes through the catalogue to find any custom items that have been unlocked
            var SavedCatalogue = new List<string>();
            for (var i = allItemsVanilla.Count; i < CatalogueManager.manage.collectedItem.Length; i++)
                if (CatalogueManager.manage.collectedItem[i]) { SavedCatalogue.Add(customItemsByItemID[i].customItemID); }
            Data.SetValue("CatalogueData", SavedCatalogue);

            UnmodTheArrays();
            
        }

        internal static void CurrentSaveInfo() {
            var test = (List<InvItemData>)Data.GetValue("InvItemData", new List<InvItemData>());
            TRTools.Log($"CurrentInv Data: {test.Count}");
            var test1 = (List<StashData>)Data.GetValue("StashData", new List<StashData>());
            TRTools.Log($"StashData Data: {test1.Count}");
            var test2 = (List<HouseData>)Data.GetValue("HouseData", new List<HouseData>());
            TRTools.Log($"HouseData Data: {test2.Count}");
            var test3 = (List<VehicleData>)Data.GetValue("VehicleData", new List<VehicleData>());
            TRTools.Log($"VehicleData Data: {test3.Count}");
            var test4 = (List<CarryableData>)Data.GetValue("CarryableData", new List<CarryableData>());
            TRTools.Log($"CarryableData Data: {test4.Count}");
            var test5 = (List<ObjectData>)Data.GetValue("ObjectData", new List<ObjectData>());
            TRTools.Log($"ObjectData Data: {test5.Count}");
            var test6 = (List<ObjectTopData>)Data.GetValue("ObjectTopData", new List<ObjectTopData>());
            TRTools.Log($"ObjectTopData Data: {test6.Count}");
            var test7 = (List<BridgeData>)Data.GetValue("BridgeData", new List<BridgeData>());
            TRTools.Log($"BridgeData Data: {test7.Count}");
            var test8 = (List<PathData>)Data.GetValue("PathData", new List<PathData>());
            TRTools.Log($"PathData Data: {test8.Count}");
            var test9 = (List<BuriedObjectData>)Data.GetValue("BuriedObjectData", new List<BuriedObjectData>());
            TRTools.Log($"BuriedObjectData Data: {test9.Count}");
            var test10 = (List<ChestData>)Data.GetValue("ChestData", new List<ChestData>());
            TRTools.Log($"ChestData Data: {test10.Count}");
            var test11 = (List<EquipData>)Data.GetValue("EquipData", new List<EquipData>());
            TRTools.Log($"EquipData Data: {test11.Count}");
            var test12 = (List<LetterData>)Data.GetValue("LetterData", new List<LetterData>());
            TRTools.Log($"LetterData Data: {test12.Count}");
        }

        // Called whenever loading or after saving
        internal static void LoadCustomItems() {

            TRTools.Log("Re-adding Items");
            ModTheArrays();

            // If just re-injecting data, then these only need to be added back to lists
            if (!TRTools.InMainMenu) {
                VehicleData.LoadAll(false);
                CarryableData.LoadAll(false);
            }

            // Loads all other items normally
            InvItemData.LoadAll();
            ChestData.LoadAll();
            EquipData.LoadAll();
            LetterData.LoadAll();
            StashData.LoadAll();
            HouseData.LoadAll();
            ObjectData.LoadAll();
            ObjectTopData.LoadAll();
            BridgeData.LoadAll();
            PathData.LoadAll();
            BuriedObjectData.LoadAll();

            // Loads saved unlocks for custom items in the catalogue
            var SavedCatalogue = (List<string>) TRItems.Data.GetValue("CatalogueData", new List<string>());
            for (var i = allItemsVanilla.Count; i < CatalogueManager.manage.collectedItem.Length; i++)
                if (SavedCatalogue.Contains(customItemsByItemID[i].customItemID)) { CatalogueManager.manage.collectedItem[i] = true; }

        }

        // Only called when loading a save slot, not when sleeping
        internal static void LoadCustomMovables() {
            VehicleData.LoadAll(true);
            CarryableData.LoadAll(true); // Do we need to check if this is on a scale? o.o
        }

    }

    public class TRCustomItem {

        public string customItemID;

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

/*#region ItemsOnGround

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

#endregion*/
