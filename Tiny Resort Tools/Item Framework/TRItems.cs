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

    /*
     * Adding Custom Tree Support:
     * 1. Tree final stage Tile Object
     * 2. Tree final stage Tile Object Settings
     * 3. Tree's Growing Log World Object 
     * 4. Tree's Stump Tile Object Settings
     * 5. Tree's Growing Tile Object + Tile Object Growth Stages
     * The stump has its own Tile Object ID that is referred to in #2 by its ID.
     * The Growing version has a bunch of connected stages on it that are (untested) referred to by the OnTileStatusMap
     * * On the last stage, it automatically turns into the final stage's version
     * Growing Log seems to be the falling tree animation and drops 1 of the wood specificed on the object.
     * Final Stage Object also holds the InventoryItemLoottable
     * 
     */

    /// <summary>Tools for working with the Dinkum inventory.</summary>
    public class TRItems {

        internal static TRModData Data;

        internal static readonly Dictionary<string, TRCustomItem> customItems = new Dictionary<string, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customItemsByItemID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customTileObjectByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customTileTypeByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customVehicleByID = new Dictionary<int, TRCustomItem>();
        internal static readonly Dictionary<int, TRCustomItem> customCarryableByID = new Dictionary<int, TRCustomItem>();

        internal static bool customItemsInitialized;
        internal static bool loadedStashes;

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
        private static List<Chest> privateStashesVanilla;


        internal static bool fixedRecipes;
        /// <returns>The details for an item with the given item ID.</returns>
        public static InventoryItem GetItemDetails(int itemID) {
            if (itemID >= 0 && itemID < Inventory.inv.allItems.Length) return Inventory.inv.allItems[itemID];
            TRTools.LogError("Attempting to get item details for item with ID of " + itemID + " which does not exist.");
            return null;
        }


        internal static void Initialize() {
            //TRTools.Log($"Initializing TRItems...");

            Data = TRData.Subscribe("TR.CustomItems");
            TRData.cleanDataEvent += UnloadCustomItems;
            TRData.postLoadEvent += LoadCustomMovables;
            TRData.injectDataEvent += LoadCustomItems;

            LeadPlugin.plugin.AddCommand(
                "give_item", "Gives you the specified number of any MODDED item. Does not work with vanilla items. To see the custom item IDs, use /tr list_items.",
                GivePlayerItem, "CustomItemID", "Quantity"
            );

            LeadPlugin.plugin.AddCommand("list_items", "Lists every item added by a mod.", ListItems);
            //TRTools.Log($"End Initialization TRItems...");

        }

        internal static void FixRecipes() {
            if (Inventory.inv) {
                for (int i = 0; i < Inventory.inv.allItems.Length; i++) {
                    if (Inventory.inv.allItems[i].craftable) {
                        foreach (var material in Inventory.inv.allItems[i].craftable.itemsInRecipe) TRItems.FixRecipeItemID(material);

                        if (Inventory.inv.allItems[i].craftable.altRecipes.Length > 0) {
                            foreach (var recipe in Inventory.inv.allItems[i].craftable.altRecipes) {
                                foreach (var material in recipe.itemsInRecipe) TRItems.FixRecipeItemID(material);
                            }
                        }
                    }
                }
            }
            fixedRecipes = true;
        }
        
        internal static void FixRecipeItemID(InventoryItem material) {
            if (material.getItemId() == -1) {
                foreach (var item in Inventory.inv.allItems) {
                    if (item.itemName == material.itemName) { material.setItemId(item.getItemId()); }
                }
            }
        }

        /// <summary>
        /// Use this to get an ID that can be saved for both vanilla and modded items.
        /// If you manually save items in special storage slots, then save this value instead of the itemID.
        /// Then when loading data, call GetLoadableItemID() with the saved value to get the itemID of the item in that slot.
        /// </summary>
        /// <param name="itemID">The itemID of the item. This its index in the Inventory.inv.allItems array.</param>
        /// <returns>A string that is either the vanilla itemID or the customItemID for modded items.</returns>
        public static string GetSaveableItemID(int itemID) {
            string saveableID;

            // Check if itemID is greater than the count of vanilla items -1 (accounting for 0)
            if (itemID > allItemsVanilla.Count - 1) {
                try {
                    if (customItemsByItemID.ContainsKey(itemID))
                        saveableID = customItemsByItemID[itemID].customItemID;
                    else
                        saveableID = null;
                }
                catch { saveableID = null; }
            }

            // Return the itemID as a string if its a vanilla item.s
            else { saveableID = itemID.ToString(); }
            return saveableID;
        }

        /// <summary>
        /// Use this to get the current itemID that matches a saved ID. If you manually save items in special storage slots,
        /// then use GetSaveableItemID for saving the item, and this method for loading it back in.
        /// </summary>
        /// <param name="savedID">The ID that was saved for this item.</param>
        /// <returns>
        /// An int that is the current itemID for this item. It matches the index of the item in the Inventory.inv.allItems array.
        /// Keep in mind that this value will change if a new mod is added or the mod loader has changed. So, if you want to save this value,
        /// you have to pass it through GetSaveableItemID() first and save the returned value instead.
        /// </returns>
        public static int GetLoadableItemID(string savedID) {

            // Check if it is a custom item by checking if the period exists in the string, return -1 if it fails.. 
            if (savedID.Contains(".")) {
                try {
                    if (customItems.ContainsKey(savedID))
                        return customItems[savedID].inventoryItem.getItemId();
                    else
                        return -2;
                }
                catch { return -2; }
            }

            // Try to parse the int of a vanilla item and if it passes return ID, if it fails return -2. 
            else
                return int.TryParse(savedID, out var loadableID) ? loadableID : -2;
        }

        internal static TRCustomItem AddCustomItem(TRPlugin plugin, string assetBundlePath, int uniqueItemID) {

            // If the nexusID is invalid and we got here, it must be in developer mode so use the mod name instead
            var nexusID = plugin.nexusID.Value == -1 ? plugin.plugin.Info.Metadata.Name.Replace(" ", "_").Replace(".", "_") : plugin.nexusID.Value.ToString();
            
            if (customItemsInitialized) {
                TRTools.LogError("Mod attempted to load a new item after item initialization. You need to load new items in your Awake() method!");
                return null;
            }
            
            customItems[nexusID + "." + uniqueItemID.ToString()] = TRCustomItem.Create(assetBundlePath);
            customItems[nexusID + "." + uniqueItemID.ToString()].customItemID = nexusID + "." + uniqueItemID.ToString();
            return customItems[nexusID + "." + uniqueItemID.ToString()];

        }

        internal static TRCustomItem AddCustomItem(
            TRPlugin plugin, int uniqueItemID, InventoryItem inventoryItem = null, TileObject tileObject = null,
            TileObjectSettings tileObjectSettings = null, TileTypes tileTypes = null, Vehicle vehicle = null,
            PickUpAndCarry pickUpAndCarry = null) {
            
            // If the nexusID is invalid and we got here, it must be in developer mode so use the mod name instead
            var nexusID = plugin.nexusID.Value == -1 ? plugin.plugin.Info.Metadata.Name.Replace(" ", "_").Replace(".", "_") : plugin.nexusID.Value.ToString();

            if (customItemsInitialized) {
                TRTools.LogError("Mod attempted to load a new item after item initialization. You need to load new items in your Awake() method!");
                return null;
            }

            customItems[nexusID + "." + uniqueItemID.ToString()] = TRCustomItem.Create(inventoryItem, tileObject, tileObjectSettings, tileTypes, vehicle, pickUpAndCarry);
            customItems[nexusID + "." + uniqueItemID.ToString()].customItemID = nexusID + "." + uniqueItemID.ToString();
            return customItems[nexusID + "." + uniqueItemID.ToString()];

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
                    if (Inventory.inv.invSlots[i].itemNo == customItem.inventoryItem.getItemId() && customItem.inventoryItem.isStackable) {
                        Inventory.inv.invSlots[i].updateSlotContentsAndRefresh(customItem.inventoryItem.getItemId(), Inventory.inv.invSlots[i].stack + quantity);
                        return "Successfully added " + quantity + " '" + customItem.inventoryItem.itemName + "' to your inventory.";
                    }

                    // Otherwise, look for an empty slot
                    else if (Inventory.inv.invSlots[i].itemNo == -1) {
                        emptyInvSlot = i;
                        break;
                    }

                }
            }

            // Place in an emtpy slot if available
            if (emptyInvSlot >= 0) {
                Inventory.inv.invSlots[emptyInvSlot].updateSlotContentsAndRefresh(customItem.inventoryItem.getItemId(), quantity);
                return "Successfully added " + quantity + " '" + customItem.inventoryItem.itemName + "' to your inventory.";
            }

            return "No room in inventory for requested item.";

        }

        // List all the custom items by custom item ID
        internal static string ListItems(string[] args) {
            if (customItems.Count <= 0) { return "The installed mods do not add any custom items."; }
            var str = "\nThe following items were added by installed mods:\n";
            foreach (var item in customItems) {
                if (item.Value.inventoryItem) str += item.Key + " (" + item.Value.inventoryItem.itemName + ")\n";
            }
            return str;
        }

        // Resize the array depending on the number of modded items added
        // Ignore modded items saved if it doesnt exist in customItems
        internal static void ManageAllItemArray() {
            //TRTools.Log($"Running ManageAllItemArray...");

            // Saves the default arrays for existing item lists
            allItemsVanilla = Inventory.inv.allItems.ToList();
            tileObjectsVanilla = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsVanilla = WorldManager.manageWorld.allObjectSettings.ToList();
            vehiclePrefabsVanilla = SaveLoad.saveOrLoad.vehiclePrefabs.ToList();
            carryablePrefabsVanilla = SaveLoad.saveOrLoad.carryablePrefabs.ToList();
            tileTypesVanilla = WorldManager.manageWorld.tileTypes.ToList();

            // Get existing item lists
            allItemsFull = Inventory.inv.allItems.ToList();
            tileObjectsFull = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsFull = WorldManager.manageWorld.allObjectSettings.ToList();
            tileTypesFull = WorldManager.manageWorld.tileTypes.ToList();
            vehiclePrefabsFull = SaveLoad.saveOrLoad.vehiclePrefabs.ToList();
            carryablePrefabsFull = SaveLoad.saveOrLoad.carryablePrefabs.ToList();

            // Add custom items to existing item lists
            foreach (var item in customItems) {

                if (item.Value.tileTypes) {
                    if (item.Value.tileTypes.isPath) {
                        try { var test = item.Value.tileTypes; }
                        catch {
                            TRTools.LogError($"Unable to load {item.Key}. tileTypes is not set correctly.");
                            continue;
                        }
                    }
                }
                if (item.Value.inventoryItem) {
                    try { var test = item.Value.inventoryItem; }
                    catch {
                        TRTools.LogError($"Unable to load {item.Key}. invItem is not set correctly.");
                        continue;
                    }
                }
                if (item.Value.tileObject) {
                    try { var test = item.Value.tileObject; }
                    catch {
                        TRTools.LogError($"Unable to load {item.Key}. tileObject is not set correctly.");
                        continue;
                    }
                    try { var test = item.Value.tileObjectSettings; }
                    catch {
                        TRTools.LogError($"Unable to load {item.Key}. tileObjectSettings is not set correctly.");
                        continue;
                    }
                }
                if (item.Value.vehicle) {
                    try { var test = item.Value.inventoryItem.spawnPlaceable; }
                    catch {
                        TRTools.LogError($"Unable to load {item.Key}. spawnPlaceable is not set correctly.");
                        continue;
                    }
                }
                if (item.Value.pickUpAndCarry) {
                    try { var test = item.Value.pickUpAndCarry.gameObject; }
                    catch {
                        TRTools.LogError($"Unable to load {item.Key}. carryable is not set correctly.");
                        continue;
                    }
                }

                // Add custom paths
                if (item.Value.tileTypes) {
                    if (item.Value.tileTypes.isPath) {
                        tileTypesFull.Add(item.Value.tileTypes);
                        item.Value.inventoryItem.placeableTileType = tileTypesFull.Count - 1;
                        customTileTypeByID[tileTypesFull.Count - 1] = item.Value;
                    }
                }

                // Add custom inventory item
                if (item.Value.inventoryItem) {
                    allItemsFull.Add(item.Value.inventoryItem);
                    item.Value.inventoryItem.setItemId(allItemsFull.Count - 1);
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
                    vehiclePrefabsFull.Add(item.Value.inventoryItem.spawnPlaceable);
                    item.Value.vehicle.saveId = vehiclePrefabsFull.Count - 1;
                    customVehicleByID[vehiclePrefabsFull.Count - 1] = item.Value;
                }

                // Add custom carryable items
                if (item.Value.pickUpAndCarry) {
                    carryablePrefabsFull.Add(item.Value.pickUpAndCarry.gameObject);
                    item.Value.pickUpAndCarry.prefabId = carryablePrefabsFull.Count - 1;
                    customCarryableByID[carryablePrefabsFull.Count - 1] = item.Value;
                }
                
            }

            // Set the game's arrays to match the new lists that include the custom items
            ModTheArrays();

            var cheatButton = typeof(CheatScript).GetField("cheatButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            cheatButton?.SetValue(CheatScript.cheat, new GameObject[Inventory.inv.allItems.Length]);

            customItemsInitialized = true;

            TRTools.Log($"Ending ManageAllItemArray...");

        }
        

        // This is used to restore the modded items into the lists after saving. It just takes the list of items we have
        // and adds them to the games arrays. 
        internal static void ModTheArrays() {
            TRTools.Log($"Running ModTheArrays...");
            Inventory.inv.allItems = allItemsFull.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectsFull.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsFull.ToArray();
            Array.Resize(ref CatalogueManager.manage.collectedItem, Inventory.inv.allItems.Length);
            SaveLoad.saveOrLoad.vehiclePrefabs = vehiclePrefabsFull.ToArray();
            SaveLoad.saveOrLoad.carryablePrefabs = carryablePrefabsFull.ToArray();
            WorldManager.manageWorld.tileTypes = tileTypesFull.ToArray();

            var cheatButton = typeof(CheatScript).GetField("cheatButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            cheatButton?.SetValue(CheatScript.cheat, new GameObject[Inventory.inv.allItems.Length]);

            TRTools.Log($"Ending ModTheArrays...");
        }

        // Restores the vanilla versions of item lists to avoid custom items leaking into save data and corrupting files
        internal static void UnmodTheArrays() {
            TRTools.Log($"Running UnmodTheArrays...");
            Inventory.inv.allItems = allItemsVanilla.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectsVanilla.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsVanilla.ToArray();
            Array.Resize(ref CatalogueManager.manage.collectedItem, allItemsVanilla.Count);
            SaveLoad.saveOrLoad.vehiclePrefabs = vehiclePrefabsVanilla.ToArray();
            SaveLoad.saveOrLoad.carryablePrefabs = carryablePrefabsVanilla.ToArray();
            WorldManager.manageWorld.tileTypes = tileTypesVanilla.ToArray();
            TRTools.Log($"Ending UnmodTheArrays...");
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
            PlantData.all.Clear();

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
                    //TRTools.Log($"Found a vehicle item {vehicle.saveId}");
                    VehicleData.Save(vehicle);
                }
            }

            // Unloads and saves all carrayable items out in the world
            var carryables = new List<PickUpAndCarry>(WorldManager.manageWorld.allCarriables);
            foreach (var carryable in carryables) {
                if (customCarryableByID.ContainsKey(carryable.prefabId)) {
                    //TRTools.Log($"Found a modded carryable");
                    CarryableData.Save(carryable);
                }
            }

            #endregion

            #region Inventory, Equipment Slots, Stashes, & Mail

            // Unloads (and saves) items from the player's inventory
            for (var i = 0; i < Inventory.inv.invSlots.Length; i++) {
                if (customItemsByItemID.ContainsKey(Inventory.inv.invSlots[i].itemNo)) {
                    //TRTools.Log($"Found Custom Item: {customItemsByItemID[Inventory.inv.invSlots[i].itemNo].inventoryItem.itemName}");
                    InvItemData.Save(i, Inventory.inv.invSlots[i].stack);
                }
            }

            // Unloads (and saves) items from the player's stash
            //for (var j = 0; j < ContainerManager.manage.privateStashes.Count; j++) {
            // Manually set to two until the StorageData class is completed by SlowCircuit. 
            // This is to prevent duplication when people are using ender storage. 
            for (var j = 0; j < 2; j++) {
                for (var i = 0; i < ContainerManager.manage.privateStashes[j].itemIds.Length; i++) {
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
                        if (allObjects[onTileMap[x, y]].showObjectOnStatusChange.isClothing && customItemsByItemID.ContainsKey(onTileMapStatus[x, y])) { ItemStatusData.Save(onTileMapStatus[x, y], x, y, -1, -1); }
                        else if (allObjects[onTileMap[x, y]].showObjectOnStatusChange.isSign && customItemsByItemID.ContainsKey(onTileMapStatus[x, y])) { ItemStatusData.Save(onTileMapStatus[x, y], x, y, -1, -1); }
                    }

                    #region Items on Top of Others (NOT in a house)

                    if (onTileMap[x, y] <= -1) continue;

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
                        else if (allObjects[onTileMap[x, y]].tileObjectGrowthStages) { PlantData.Save(onTileMap[x, y], x, y, onTileMapStatus[x, y]); }

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
                                if (allObjects[tileObjectID].tileObjectItemChanger) {
                                    if (houseMapOnTileStatus >= 0 && Inventory.inv.allItems[houseMapOnTileStatus] && customItemsByItemID.ContainsKey(houseMapOnTileStatus)) {
                                        var changer = WorldManager.manageWorld.allChangers.Find(i => i.xPos == houseTileX && i.yPos == houseTileY && i.houseX == x && i.houseY == y);
                                        ItemChangerData.Save(houseMapOnTileStatus, changer);
                                    }
                                }

                                if (allObjects[tileObjectID].showObjectOnStatusChange) {
                                    if (allObjects[tileObjectID].showObjectOnStatusChange.isClothing && customItemsByItemID.ContainsKey(onTileMapStatus[houseTileX, houseTileY])) { ItemStatusData.Save(houseMapOnTileStatus, houseTileX, houseTileY, x, y); }
                                    else if (allObjects[tileObjectID].showObjectOnStatusChange.isSign && customItemsByItemID.ContainsKey(onTileMapStatus[houseTileX, houseTileY])) { ItemStatusData.Save(houseMapOnTileStatus, houseTileX, houseTileY, x, y); }
                                }

                                #region Chests (INSIDE a house)

                                // If the object on this house tile is a chest, save and unload custom items from the chest
                                if (allObjects[tileObjectID].tileObjectChest) { ChestData.Save(allObjects[tileObjectID].tileObjectChest, houseTileX, houseTileY, x, y); }

                                #endregion

                                #region World Objects (INSIDE a house)

                                // If it's a custom item, save and unload it
                                if (customTileObjectByID.ContainsKey(tileObjectID)) ObjectData.Save(tileObjectID, houseTileX, houseTileY, houseDetails.houseMapRotation[houseTileX, houseTileY], x, y);

                                #endregion

                            }
                        }
                    }
                }
            }

            #endregion

            #region Save All Data (All and LostAndFound Lists)

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
            Data.SetValue("PlantData", PlantData.all);

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
            Data.SetValue("PlantDataLostAndFound", PlantData.lostAndFound);

            /*TRTools.Log($"Saving InvItemData: {InvItemData.all.Count}");
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
            TRTools.Log($"Saving PlantData: {PlantData.all.Count}"); */

            #endregion

            // Goes through the catalogue to find any custom items that have been unlocked
            var SavedCatalogue = new List<string>();
            for (var i = allItemsVanilla.Count; i < CatalogueManager.manage.collectedItem.Length; i++)
                if (CatalogueManager.manage.collectedItem[i]) { SavedCatalogue.Add(customItemsByItemID[i].customItemID); }
            Data.SetValue("CatalogueData", SavedCatalogue);

            UnmodTheArrays();

        }

        // Called whenever loading or after saving
        internal static void LoadCustomItems() {
            if (!loadedStashes) {
                TRTools.Log($"Loading all Stashes: Required to parse them later.");
                ContainerManager.manage.loadStashes();
                loadedStashes = true;

            }
            privateStashesVanilla = ContainerManager.manage.privateStashes.ToList();

            TRTools.Log($"Start adding in all Saved Custom Items...");
            TRTools.Log($"Making sure the array sizes are the appropriate size...");
            ModTheArrays();

            // If just re-injecting data, then these only need to be added back to lists
            if (!TRTools.InMainMenu) {
                TRTools.Log($"Loading in Vehicle and Carryables...");
                VehicleData.LoadAll(false);
                CarryableData.LoadAll(false);
            }

            // Loads all other items normally
            TRTools.Log($"Loading in the rest of the data...");
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
            PlantData.LoadAll();

            // Loads saved unlocks for custom items in the catalogue
            TRTools.Log($"Loading in the saved Catalogue data and adding to list...");
            var SavedCatalogue = (List<string>)TRItems.Data.GetValue("CatalogueData", new List<string>());
            for (var i = allItemsVanilla.Count; i < CatalogueManager.manage.collectedItem.Length; i++)
                if (SavedCatalogue.Contains(customItemsByItemID[i].customItemID)) { CatalogueManager.manage.collectedItem[i] = true; }

        }

        // Only called when loading a save slot, not when sleeping
        internal static void LoadCustomMovables() {
            TRTools.Log($"Loading in custom movables...");
            VehicleData.LoadAll(true);
            CarryableData.LoadAll(true); // Do we need to check if this is on a scale? o.o
        }

    }

    /// <summary> Contains various references to components important to your custom item. </summary>
    public class TRCustomItem {

        /// <returns>Returns the unique ID of your custom item. This will be a combination of your nexus ID and the item ID you gave when adding it.</returns>
        public string GetUniqueID() => customItemID;

        internal string customItemID;
        internal bool isQuickItem;

        // TODO: Implement events
        //public delegate void TileObjectEvent();
        //public TileObjectEvent interactEvent;

        public InventoryItem inventoryItem;
        public TileObject tileObject;
        public TileObjectSettings tileObjectSettings;
        public TileTypes tileTypes;
        public Vehicle vehicle;
        public PickUpAndCarry pickUpAndCarry;

        internal static TRCustomItem Create(string assetBundlePath) {

            TRTools.Log("Attemping Load: Asset Bundle.");
            var newItem = new TRCustomItem();
            var bundle = TRAssets.LoadBundle(assetBundlePath);
            TRTools.Log($"Loaded: Asset Bundle -- {bundle}.");

            var AllAssets = bundle.LoadAllAssets<GameObject>();
            for (var i = 0; i < AllAssets.Length; i++) {
                if (newItem.inventoryItem == null) { newItem.inventoryItem = AllAssets[i].GetComponent<InventoryItem>(); }
                if (newItem.tileObject == null) { newItem.tileObject = AllAssets[i].GetComponent<TileObject>(); }
                if (newItem.tileObjectSettings == null) { newItem.tileObjectSettings = AllAssets[i].GetComponent<TileObjectSettings>(); }
                if (newItem.tileTypes == null) { newItem.tileTypes = AllAssets[i].GetComponent<TileTypes>(); }
                if (newItem.vehicle == null) { newItem.vehicle = AllAssets[i].GetComponent<Vehicle>(); }
                if (newItem.pickUpAndCarry == null) { newItem.pickUpAndCarry = AllAssets[i].GetComponent<PickUpAndCarry>(); }
            }

            bundle.Unload(false);
            return newItem;

        }

        internal static TRCustomItem Create(
            InventoryItem inventoryItem = null, TileObject tileObject = null, TileObjectSettings tileObjectSettings = null,
            TileTypes tileTypes = null, Vehicle vehicle = null, PickUpAndCarry pickUpAndCarry = null
        ) {

            if (inventoryItem == null && tileObject == null && tileObjectSettings == null && tileTypes == null && vehicle == null && pickUpAndCarry == null) { return null; }

            var newItem = new TRCustomItem();

            newItem.inventoryItem = inventoryItem;
            newItem.tileObject = tileObject;
            newItem.tileObjectSettings = tileObjectSettings;
            newItem.tileTypes = tileTypes;
            newItem.vehicle = vehicle;
            newItem.pickUpAndCarry = pickUpAndCarry;

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
