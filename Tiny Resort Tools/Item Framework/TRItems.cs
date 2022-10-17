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
        * Handle case where a non-modded item is on top of modded furniture. -- Semi Handled

     MID PRIORITY
        * Add ability to put custom items on enemy drop tables.
        * Add ability to put custom items in mine chest loot tables.
        * Add ability to put custom items in loot that's buried underground, both in boxes and on its own.
        * Add ability to add a custom item recipe to Franklyn's crafting shop.
        * Add ability to make a recipe for a custom item and to give that recipe to the player.
        * Add ability to add custom item to items NPCs give you, as well as to the recycle bin.
        
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

            LeadPlugin.plugin.AddCommand(
                "give_item", "Gives you the specified number of any MODDED item. Does not work with vanilla items. To see the custom item IDs, use /tr list_items.",
                GivePlayerItem, "CustomItemID", "Quantity"
            );
            
            LeadPlugin.plugin.AddCommand("list_items", "Lists every item added by a mod.", ListItems);
            
        }

        internal static TRCustomItem AddCustomItem(TRPlugin plugin, string assetBundlePath, int uniqueItemID) {

            if (customItemsInitialized) {
                TRTools.LogError("Mod attempted to load a new item after item initialization. You need to load new items in your Awake() method!");
                return null;
            }

            customItems[plugin.nexusID.Value + uniqueItemID.ToString()] = TRCustomItem.Create(assetBundlePath);
            customItems[plugin.nexusID.Value + uniqueItemID.ToString()].customItemID = plugin.nexusID.Value + uniqueItemID.ToString();
            return customItems[plugin.nexusID.Value + uniqueItemID.ToString()];

        }

        internal static string GivePlayerItem(string[] args) {
            
            // Makes sure any arguments were supplied
            if (args.Length <= 0) return "No arguments provided.";
            var customItemID = args[0];

            // If no matching custom
            TRCustomItem customItem = null;
            foreach (var item in customItems) {
                if (item.Key.ToLower() == customItemID) {
                    customItem = item.Value;
                    break;
                }
            }
            
            // If no matching item was found, return so
            if (customItem == null) { return "No matching item found."; }
            
            // If no quantity was supplied or it was below 1, set the quantity to 1
            var quantity = 1;
            if (args.Length > 1) { int.TryParse(args[1], out quantity); }
            if (quantity <= 0) { quantity = 1; }

            // Go through player inventory and place custom item in empty slot if one is available
            var emptyInvSlot = -1;
            for (var i = 0; i < Inventory.inv.invSlots.Length; i++) {
                if (Inventory.inv.invSlots[i].slotUnlocked) {
                    
                    // If the item is already in inventory and stackable, place it there
                    if (Inventory.inv.invSlots[i].itemNo == customItem.invItem.getItemId() && customItem.invItem.isStackable) {
                        Inventory.inv.invSlots[i].updateSlotContentsAndRefresh(customItem.invItem.getItemId(), Inventory.inv.invSlots[i].stack + quantity);
                        return "Successfully added " + quantity + " '" + customItem.invItem.itemName + "' to your inventory.";
                    }
                    
                    // Otherwise, look for an empty slot
                    else if (Inventory.inv.invSlots[i].itemNo == -1) { emptyInvSlot = i; break; }
                    
                }
            }
            
            // Place in an emtpy slot if available
            if (emptyInvSlot >= 0) {
                Inventory.inv.invSlots[emptyInvSlot].updateSlotContentsAndRefresh(customItem.invItem.getItemId(), quantity);
                return "Successfully added " + quantity + " '" + customItem.invItem.itemName + "' to your inventory.";
            }
            
            return "No room in inventory for requested item.";
            
        }

        // List all the custom items by custom item ID
        internal static string ListItems(string[] args) {
            TRTools.Log($"Test 1");
            if (customItems.Count <= 0) { return "The installed mods do not add any custom items."; }
            TRTools.Log($"Test 2");
            var str = "\nThe following items were added by installed mods:\n";
            TRTools.Log($"Test 3");
            foreach (var item in customItems) {
                TRTools.Log($"Test 4");
                if (item.Value.isQuickItem) {
                    TRTools.Log($"Test 5");
                    str += item.Key + "\n"; }
                else {
                    TRTools.Log($"Test 6");
                    if (item.Value.invItem) str += item.Key + "(" + item.Value.invItem.itemName + ")\n"; }
            }
            TRTools.Log($"Test 7");
            return str;
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
            ItemChangerData.all.Clear();

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
            var onTileMapStatus = WorldManager.manageWorld.onTileStatusMap;
            for (var x = 0; x < onTileMap.GetLength(0); x++) { 
                for (var y = 0; y < onTileMap.GetLength(1); y++) {

                    // If the tile is empty, ignore it
                    if (onTileMap[x, y] <= -1) continue;

                    if (allObjects[onTileMap[x, y]].showObjectOnStatusChange) {
                        if (allObjects[onTileMap[x, y]].showObjectOnStatusChange.isClothing && customItemsByItemID.ContainsKey(onTileMapStatus[x,y])) {
                            TRTools.Log($"MOD Found Clothing Item... {allObjects[onTileMap[x, y]]} | {onTileMap[x, y]} | ID: {onTileMapStatus[x, y]}");
                            ItemStatusData.Save(onTileMapStatus[x, y], x, y, -1, -1);
                        }
                        else if (allObjects[onTileMap[x, y]].showObjectOnStatusChange.isSign && customItemsByItemID.ContainsKey(onTileMapStatus[x, y])) {
                            TRTools.Log($"MOD Found Sign Item... {allObjects[onTileMap[x, y]]} | {onTileMap[x, y]} | ID: {onTileMapStatus[x, y]}");
                            ItemStatusData.Save(onTileMapStatus[x, y], x, y, -1, -1);
                        }
                    }

                    #region Items on Top of Others (NOT in a house)

                    // Removes items that are on top of other items.
                    // Does this before removing ground items to prevent issues
                    var onTopOfTile = ItemOnTopManager.manage.getAllItemsOnTop(x, y, null);
                    for (var i = 0; i < onTopOfTile.Length; i++)
                        if (customTileObjectByID.ContainsKey(onTopOfTile[i].itemId))
                            ObjectTopData.Save(onTopOfTile[i].itemId, x, y, onTopOfTile[i].itemRotation, -1, -1, onTopOfTile[i].itemStatus, onTopOfTile[i].onTopPosition);

                    #endregion

                    #region Chests (NOT in a house)

                    // If the tile has a chest on it, save and unload custom items from the chest
                    if (allObjects[onTileMap[x, y]].tileObjectChest) { ChestData.Save(allObjects[onTileMap[x, y]].tileObjectChest, x, y, -1, -1); }

                    #endregion

                    #region World Object & Bridges (NOT in a house)

                    // If the tile contains a custom world object, unload and save it
                    if (customTileObjectByID.ContainsKey(onTileMap[x, y])) {
                        // If not a bridge, save as an overworld object
                        var rotation = WorldManager.manageWorld.rotationMap[x, y];

                        if (allObjects[onTileMap[x, y]].tileObjectItemChanger) {
                            if (onTileMapStatus[x, y] >= 0 && Inventory.inv.allItems[onTileMapStatus[x, y]] && customItemsByItemID.ContainsKey(onTileMapStatus[x, y])) {
                                var changer = WorldManager.manageWorld.allChangers.Find(i => i.xPos == x && i.yPos == y && i.houseX == -1 && i.houseY == -1);
                                TRTools.Log($"Cyles: {changer.cycles} | Seconds: {changer.counterSeconds} | Days: {changer.counterDays} | House: ({changer.houseX}, {changer.houseY}) | Cycle Time: {changer.timePerCycles}");
                                ItemChangerData.Save(onTileMapStatus[x, y], changer);
                            }
                        }

                        // If it is a bridge or tileObjectItemChanger, find the length and save the bridge
                        else if (allObjects[onTileMap[x, y]].tileObjectBridge) {
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
                        else { ObjectData.Save(onTileMap[x, y], x, y, rotation, -1, -1); }

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
                                var houseMapOnTileStatus = houseDetails.houseMapOnTileStatus[houseTileX, houseTileY];
                                if (tileObjectID <= 0) continue;
                                
                                if (allObjects[tileObjectID].showObjectOnStatusChange) {
                                    if (allObjects[tileObjectID].showObjectOnStatusChange.isClothing && customItemsByItemID.ContainsKey(onTileMapStatus[houseTileX, houseTileY])) {
                                        TRTools.Log($"MOD Found Clothing Item... {allObjects[tileObjectID]} | {tileObjectID} | ID: {houseMapOnTileStatus}");
                                        ItemStatusData.Save(houseMapOnTileStatus, houseTileX, houseTileY, x, y);
                                    }
                                    else if (allObjects[tileObjectID].showObjectOnStatusChange.isSign && customItemsByItemID.ContainsKey(onTileMapStatus[houseTileX, houseTileY])) {
                                        TRTools.Log($"MOD Found Sign Item... {allObjects[tileObjectID]} | {tileObjectID} | ID: {houseMapOnTileStatus}");
                                        ItemStatusData.Save(houseMapOnTileStatus, houseTileX, houseTileY, x, y);
                                    }
                                }
                          
                                #region Chests (INSIDE a house)

                                // If the object on this house tile is a chest, save and unload custom items from the chest
                                if (allObjects[tileObjectID].tileObjectChest) { ChestData.Save(allObjects[tileObjectID].tileObjectChest, houseTileX, houseTileY, x, y); }

                                #endregion

                                #region World Objects (INSIDE a house)

                                // If it's a custom item, save and unload it
                                if (customTileObjectByID.ContainsKey(tileObjectID)) ObjectData.Save(tileObjectID, houseTileX, houseTileY, houseDetails.houseMapRotation[houseTileX, houseTileY], x, y);

                                #endregion

                                if (allObjects[tileObjectID].tileObjectItemChanger) {
                                    if (houseMapOnTileStatus >= 0 && Inventory.inv.allItems[houseMapOnTileStatus] && customItemsByItemID.ContainsKey(houseMapOnTileStatus)) {
                                        var changer = WorldManager.manageWorld.allChangers.Find(i => i.xPos == houseTileX && i.yPos == houseTileY && i.houseX == x && i.houseY == y);
                                        ItemChangerData.Save(houseMapOnTileStatus, changer);
                                    }
                                }

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
            Data.SetValue("ItemChangerData", ItemChangerData.all);
            Data.SetValue("ItemStatusData", ItemStatusData.all);

            // Save all the new lost and found data
            Data.SetValue("InvItemDataLostAndFound", InvItemData.lostAndFound);
            Data.SetValue("ChestDataLostAndFound", ChestData.lostAndFound);
            Data.SetValue("EquipDataLostAndFound", EquipData.lostAndFound);
            Data.SetValue("LetterDataLostAndFound", LetterData.lostAndFound);
            Data.SetValue("StashDataLostAndFound", StashData.lostAndFound);
            Data.SetValue("HouseDataLostAndFound", HouseData.lostAndFound);
            Data.SetValue("VehicleDataLostAndFound", VehicleData.lostAndFound);
            Data.SetValue("CarryableDataLostAndFound", CarryableData.lostAndFound);
            Data.SetValue("ObjectDataLostAndFound", ObjectData.lostAndFound);
            Data.SetValue("ObjectTopDataLostAndFound", ObjectTopData.lostAndFound);
            Data.SetValue("BridgeDataLostAndFound", BridgeData.lostAndFound);
            Data.SetValue("PathDataLostAndFound", PathData.lostAndFound);
            Data.SetValue("BuriedObjectDataLostAndFound", BuriedObjectData.lostAndFound);
            Data.SetValue("ItemChangerDataLostAndFound", ItemChangerData.lostAndFound);
            Data.SetValue("ItemStatusDataLostAndFound", ItemStatusData.lostAndFound);

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
            TRTools.Log($"Saving ItemChangerData: {ItemChangerData.all.Count}");

            // Goes through the catalogue to find any custom items that have been unlocked
            var SavedCatalogue = new List<string>();
            for (var i = allItemsVanilla.Count; i < CatalogueManager.manage.collectedItem.Length; i++)
                if (CatalogueManager.manage.collectedItem[i]) { SavedCatalogue.Add(customItemsByItemID[i].customItemID); }
            Data.SetValue("CatalogueData", SavedCatalogue);

            UnmodTheArrays();

        }

        internal static void CurrentSaveInfo() {
            var test = (List<InvItemData>)Data.GetValue("InvItemData", new List<InvItemData>());
            var test1 = (List<StashData>)Data.GetValue("StashData", new List<StashData>());
            var test2 = (List<HouseData>)Data.GetValue("HouseData", new List<HouseData>());
            var test3 = (List<VehicleData>)Data.GetValue("VehicleData", new List<VehicleData>());
            var test4 = (List<CarryableData>)Data.GetValue("CarryableData", new List<CarryableData>());
            var test5 = (List<ObjectData>)Data.GetValue("ObjectData", new List<ObjectData>());
            var test6 = (List<ObjectTopData>)Data.GetValue("ObjectTopData", new List<ObjectTopData>());
            var test7 = (List<BridgeData>)Data.GetValue("BridgeData", new List<BridgeData>());
            var test8 = (List<PathData>)Data.GetValue("PathData", new List<PathData>());
            var test9 = (List<BuriedObjectData>)Data.GetValue("BuriedObjectData", new List<BuriedObjectData>());
            var test10 = (List<ChestData>)Data.GetValue("ChestData", new List<ChestData>());
            var test11 = (List<EquipData>)Data.GetValue("EquipData", new List<EquipData>());
            var test12 = (List<LetterData>)Data.GetValue("LetterData", new List<LetterData>());
            var test13 = (List<BridgeData>)Data.GetValue("BridgeDataLostAndFound", new List<BridgeData>());
            TRTools.Log($"StashData Data: {test1.Count}");
            TRTools.Log($"CurrentInv Data: {test.Count}");
            TRTools.Log($"HouseData Data: {test2.Count}");
            TRTools.Log($"VehicleData Data: {test3.Count}");
            TRTools.Log($"CarryableData Data: {test4.Count}");
            TRTools.Log($"ObjectData Data: {test5.Count}");
            TRTools.Log($"ObjectTopData Data: {test6.Count}");
            TRTools.Log($"BridgeData Data: {test7.Count}");
            TRTools.Log($"PathData Data: {test8.Count}");
            TRTools.Log($"BuriedObjectData Data: {test9.Count}");
            TRTools.Log($"ChestData Data: {test10.Count}");
            TRTools.Log($"EquipData Data: {test11.Count}");
            TRTools.Log($"LetterData Data: {test12.Count}");
            TRTools.Log($"BridgeDataLostAndFound Data: {test13.Count}");
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
            ItemChangerData.LoadAll();
            ItemStatusData.LoadAll();
            
            // Loads saved unlocks for custom items in the catalogue
            var SavedCatalogue = (List<string>)TRItems.Data.GetValue("CatalogueData", new List<string>());
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
        internal bool isQuickItem;

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
