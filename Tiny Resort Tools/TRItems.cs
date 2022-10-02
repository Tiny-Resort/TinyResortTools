using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TinyResort {

    /// <summary>Tools for working with the Dinkum inventory.</summary>
    public class TRItems {

        /*
        * To Do List:
        * Quick creating tiles, clothing, floors and walls with just textures.
        * Make it so the better icons code also checks for icons for custom items automatically(if it does already, we need to test it).
        * (Some Work Done)Catalogue support(Mostly need to keep track of catalogue, its safe already)
        * (Maybe) Quick creating furniture.
        * (No Work Done)Carryable Support
        * (No Work Done)Mail Support -- Need to remove item from mailbox if it is a modded item.How do we test this?
        * (No Work Done)Trap Support(maybe?)
        * (No Work Done)Vehicle Support
        * (No Work Done)Fences Support - Replace modded fence with a base game fence to make sure the next day fence calculations run correctly.
        * (No Work Done)Buried Objects     
        */

        private static Dictionary<int, InventoryItem> itemDetails = new Dictionary<int, InventoryItem>();
        private static Dictionary<string, TRCustomItem> customItems = new Dictionary<string, TRCustomItem>();
        private static Dictionary<int, TRCustomItem> customItemsByID = new Dictionary<int, TRCustomItem>();
        private static Dictionary<int, TRCustomItem> customTileObjectByID = new Dictionary<int, TRCustomItem>();
        private static Dictionary<int, TRCustomItem> customTileTypeByID = new Dictionary<int, TRCustomItem>();

        private static List<ItemSaveData> savedItemData = new List<ItemSaveData>();
        private static TRModData Data;

        private static List<InventoryItem> itemList;
        private static List<TileObject> tileObjectList;
        private static List<TileObjectSettings> tileObjectSettingsList;
        private static List<TileTypes> tileTypesList;

        private static List<bool> CatalogueDefaultList;
        private static List<InventoryItem> allItemsDefaultList;

        private static List<TileObject> tileObjectDefaultList;
        private static List<TileObjectSettings> tileObjectSettingsDefaultList;

        internal static bool customItemsInitialized;

        // TODO: Create LetterType template for Mods
        // TODO: Create more robust system for developers to send user mail
        // TODO: Look at ShowLetter function
        public static void SendMail(TRCustomItem itemToSend) { MailManager.manage.mailInBox.Add(new Letter(1, Letter.LetterType.AnimalTrapReturn, itemToSend.invItem.getItemId(), 5)); }

        public static void SendMailTomorrow(TRCustomItem itemToSend) { MailManager.manage.tomorrowsLetters.Add(new Letter(1, Letter.LetterType.AnimalTrapReturn, itemToSend.invItem.getItemId(), 5)); }

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

        internal static TRCustomItem AddCustomItem(TRPlugin plugin, string assetBundlePath, string uniqueItemID) {

            if (customItemsInitialized) {
                TRTools.LogError($"Mod attempted to load a new item after item initialization. You need to load new items in your Awake() method!");
                return null;
            }

            customItems[plugin.nexusID + uniqueItemID] = new TRCustomItem(assetBundlePath);
            customItems[plugin.nexusID + uniqueItemID].uniqueID = plugin.nexusID + uniqueItemID;

            return customItems[plugin.nexusID + uniqueItemID];
        }

        // Resize the array depending on the number of modded items added
        // Ignore modded items saved if it doesnt exist in customItems

        internal static void ManageAllItemArray() {
            // Default lengths of all the arrays
            allItemsDefaultList = Inventory.inv.allItems.ToList();
            tileObjectDefaultList = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsDefaultList = WorldManager.manageWorld.allObjectSettings.ToList();
            CatalogueDefaultList = CatalogueManager.manage.collectedItem.ToList();
            TRTools.Log($"Catalogue size: {CatalogueManager.manage.collectedItem.Length}");

            // Get existing item lists
            itemList = Inventory.inv.allItems.ToList();
            tileObjectList = WorldManager.manageWorld.allObjects.ToList();
            tileObjectSettingsList = WorldManager.manageWorld.allObjectSettings.ToList();
            tileTypesList = WorldManager.manageWorld.tileTypes.ToList();

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
            WorldManager.manageWorld.tileTypes = tileTypesList.ToArray();

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

        internal static void RestoreModSize() {
            Inventory.inv.allItems = itemList.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectList.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsList.ToArray();
            CatalogueManager.manage.collectedItem = new bool[Inventory.inv.allItems.Length];
        }

        internal static bool DefaultSize() {
            TRTools.Log($"Items: {Inventory.inv.allItems.Length} | Objects: {WorldManager.manageWorld.allObjects.Length} & {WorldManager.manageWorld.allObjectSettings.Length} | Catalogue: {CatalogueManager.manage.collectedItem.Length}");

            Array.Resize<InventoryItem>(ref Inventory.inv.allItems, allItemsDefaultList.Count);
            Array.Resize<TileObject>(ref WorldManager.manageWorld.allObjects, tileObjectDefaultList.Count);
            Array.Resize<TileObjectSettings>(ref WorldManager.manageWorld.allObjectSettings, tileObjectSettingsDefaultList.Count);
            Array.Resize(ref CatalogueManager.manage.collectedItem, allItemsDefaultList.Count);

            Inventory.inv.allItems = allItemsDefaultList.ToArray();
            WorldManager.manageWorld.allObjects = tileObjectDefaultList.ToArray();
            WorldManager.manageWorld.allObjectSettings = tileObjectSettingsDefaultList.ToArray();
            CatalogueManager.manage.collectedItem = CatalogueDefaultList.ToArray();
            TRTools.Log($"Items: {Inventory.inv.allItems.Length} | Objects: {WorldManager.manageWorld.allObjects.Length} & {WorldManager.manageWorld.allObjectSettings.Length} | Catalogue: {CatalogueManager.manage.collectedItem.Length}");
            return true;
        }

        internal static void UnloadCustomItems() {

            TRTools.Log($"Remvoing Items");
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
                    //TRTools.Log($"Found Item: {stash.itemIds[i]}");
                    if (customItemsByID.ContainsKey(stash.itemIds[i])) {
                        savedItemData.Add(new ItemSaveData(customItemsByID[stash.itemIds[i]], stash.itemStacks[i], j, i));
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

            #region Mail

            var CurrentMail = MailManager.manage.mailInBox;
            var TomorrowsMail = MailManager.manage.tomorrowsLetters;

            foreach (var letter in CurrentMail) {
                if (customItemsByID.ContainsKey(letter.itemOriginallAttached) || customItemsByID.ContainsKey(letter.itemAttached)) { TRTools.Log($"Found mail with a custom Item {customItemsByID[letter.itemOriginallAttached].invItem.itemName}"); }
            }

            foreach (var letter in TomorrowsMail) {
                if (customItemsByID.ContainsKey(letter.itemOriginallAttached) || customItemsByID.ContainsKey(letter.itemAttached)) { TRTools.Log($"Found mail for tomorrow with a custom Item {customItemsByID[letter.itemOriginallAttached].invItem.itemName}"); }
            }

            #endregion

            // Removed or possible planned content?
            //if (customItemsByID.ContainsKey(EquipWindow.equip.idolSlot.itemNo)) { }

            #region Tile/Paths

            // Go through every tile on the world map and look for objects that are custom items to unload them
            // This prevents corrupted save data if the player removes the mod and tries to load
            var tileTypeMap = WorldManager.manageWorld.tileTypeMap;
            for (int x = 0; x < tileTypeMap.GetLength(0); x++) {
                for (int y = 0; y < tileTypeMap.GetLength(1); y++) {
                    if (tileTypeMap[x, y] > -1) {
                        if (WorldManager.manageWorld.tileTypes[tileTypeMap[x, y]].isPath) {
                            if (customTileTypeByID.ContainsKey(tileTypeMap[x, y])) {
                                UnloadFromTile(x, y, tileTypeMap[x, y]);
                                tileTypeMap[x, y] = 0;
                            }
                        }
                    }
                }
            }

            #endregion

            #region World Objects & Chests

            var allObjects = WorldManager.manageWorld.allObjects;
            var tileMap = WorldManager.manageWorld.onTileMap;
            for (int x = 0; x < tileMap.GetLength(0); x++) {
                for (int y = 0; y < tileMap.GetLength(1); y++) {
                    if (tileMap[x, y] <= -1) continue;

                    #region OnTopOf Outside

                    var onTopOfTile = ItemOnTopManager.manage.getAllItemsOnTop(x, y, null);

                    for (int i = 0; i < onTopOfTile.Length; i++) {
                        if (customTileObjectByID.ContainsKey(onTopOfTile[i].itemId)) {
                            UnloadWorldObjectOnTop(
                                onTopOfTile[i].itemId, x, y,
                                -1, -1,
                                onTopOfTile[i].itemRotation,
                                onTopOfTile[i].itemStatus,
                                true, onTopOfTile[i].onTopPosition
                            );
                        }
                    }

                    #endregion

                    #region Chests

                    // If the tile has a chest on it, save and unload custom items from the chest
                    if (allObjects[tileMap[x, y]].tileObjectChest) { UnloadFromChest(allObjects[tileMap[x, y]].tileObjectChest, x, y, -1, -1); }

                    #endregion

                    #region Bridges

                    //TRTools.Log($"Tile Map: {tileMap[x, y]}");
                    //else if (allObjects[tileMap[x, y]].tileObjectBridge && customTileObjectByID.ContainsKey(tileMap[x, y])) { TRTools.Log($"FOUND A BRIDGE YAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAY\n\n\n\n\n\n"); }

                    #endregion

                    #region Onject On Floor Outside

                    // If the tile contains a custom world object, save and unload it
                    else if (customTileObjectByID.ContainsKey(tileMap[x, y])) {
                        var rotation = WorldManager.manageWorld.rotationMap[x, y];
                        var type = ItemSaveData.WorldObject.OnTile;
                        var bridgeLength = -1;
                        if (allObjects[tileMap[x, y]].tileObjectBridge) {
                            type = ItemSaveData.WorldObject.Bridge;
                            if (rotation == 1) { bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 0, -1); }
                            else if (rotation == 2) { bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, -1, 0); }
                            else if (rotation == 3) { bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 0, 1); }
                            else if (rotation == 4) { bridgeLength = customTileObjectByID[tileMap[x, y]].tileObjectSettings.checkBridgLenth(x, y, 1, 0); }
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
                        for (int i = 0; i < onTopOfTileInside.Length; i++) {
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
                        }

                        #endregion

                        for (int houseX = 0; houseX < houseDetails.houseMapOnTile.GetLength(0); houseX++) {
                            for (int houseY = 0; houseY < houseDetails.houseMapOnTile.GetLength(1); houseY++) {

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

            var done = DefaultSize();
            if (done) Data.SetValue("CurrentItemList", savedItemData);
        }

        //        public ItemSaveData(TRCustomItem customItem, WorldObject type, int objectXPos, int objectYPos, int tileType) {

        internal static void UnloadFromMail() { }

        internal static void UnloadFromTile(int objectXPos, int objectYPos, int tileType) { savedItemData.Add(new ItemSaveData(customTileTypeByID[tileType], ItemSaveData.WorldObject.Path, objectXPos, objectYPos, tileType)); }

        internal static void UnloadFromChest(ChestPlaceable chestPlaceable, int objectXPos, int objectYPos, int houseXPos, int houseYPos) {
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            chestPlaceable.checkIfEmpty(objectXPos, objectYPos, houseDetails);
            var chest = ContainerManager.manage.activeChests.First(p => p.xPos == objectXPos && p.yPos == objectYPos && p.inside == (houseDetails != null));
            for (int z = 0; z < chest.itemIds.Length; z++) {
                if (customItemsByID.ContainsKey(chest.itemIds[z])) {
                    savedItemData.Add(new ItemSaveData(customItemsByID[chest.itemIds[z]], z, chest.itemStacks[z], objectXPos, objectYPos, houseXPos, houseYPos));
                    ContainerManager.manage.changeSlotInChest(objectXPos, objectYPos, z, -1, 0, houseDetails);
                }
            }
        }

        internal static void UnloadWorldObject(int tileObjectID, int objectXPos, int objectYPos, int houseXPos, int houseYPos, int rotation, ItemSaveData.WorldObject type = ItemSaveData.WorldObject.OnTile, int bridgeLength = -1) {
            var houseDetails = houseXPos == -1 ? null : HouseManager.manage.getHouseInfo(houseXPos, houseYPos);
            savedItemData.Add(new ItemSaveData(customTileObjectByID[tileObjectID], objectXPos, objectYPos, rotation, type, houseXPos, houseYPos, bridgeLength: bridgeLength));
            if (houseDetails != null) {
                customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObjectInside(objectXPos, objectYPos, rotation, houseDetails);
                houseDetails.houseMapOnTile[objectXPos, objectYPos] = -1;
                houseDetails.houseMapOnTileStatus[objectXPos, objectYPos] = -1;
                var house = HouseManager.manage.findHousesOnDisplay(houseXPos, houseYPos);

                //TRTools.Log($"House: -- {house} -- ");
                if (house && house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture) {
                    house.tileObjectsInHouse[objectXPos, objectYPos].tileObjectFurniture.updateOnTileStatus(objectXPos, objectYPos, houseDetails);
                    house.refreshHouseTiles();
                }

                //HouseManager.manage.updateAllHouseFurniturePos();

            }
            else {
                customTileObjectByID[tileObjectID].tileObject.removeMultiTiledObject(objectXPos, objectYPos, rotation);
                WorldManager.manageWorld.onTileMap[objectXPos, objectYPos] = -1;
                WorldManager.manageWorld.onTileStatusMap[objectXPos, objectYPos] = -1;
                WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(objectXPos, objectYPos, false);
                NetworkNavMesh.nav.updateChunkInUse();
            }
        }

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
                DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(HouseXPos, HouseYPos);
                if (displayPlayerHouseTiles && displayPlayerHouseTiles.tileObjectsInHouse[objectXPos, objectYPos]) { displayPlayerHouseTiles.tileObjectsInHouse[objectXPos, objectYPos].checkOnTopInside(objectXPos, objectYPos, HouseManager.manage.getHouseInfo(HouseXPos, HouseYPos)); }
            }
            else {
                ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, objectXPos, objectYPos, null));
                WorldManager.manageWorld.unlockClientTile(objectXPos, objectYPos);
                WorldManager.manageWorld.refreshAllChunksInUse(objectXPos, objectYPos, false);

            }
        }

        internal static void LoadCustomItems() {
            RestoreModSize();
            TRTools.Log($"Re-adding Items");

            // This should put OnTop to the end of the list and process them last.
            savedItemData = savedItemData.OrderBy(i => i.type == ItemSaveData.WorldObject.OnTop).ToList();

            // If not loaded yet, load the custom items that were in the player's inventory or chests
            if (savedItemData.Count == 0) { savedItemData = (List<ItemSaveData>)Data.GetValue("CurrentItemList", new List<ItemSaveData>()); }

            if (savedItemData.Count > 0) {
                var tmpHouseDetails = new HouseDetails();
                tmpHouseDetails = null;
                foreach (var item in savedItemData) {
                    if (!customItems.TryGetValue(item.uniqueID, out var customItem)) continue;
                    TRTools.Log($"Item Found: {customItem.invItem.itemName}");

                    switch (item.location) {

                        case ItemSaveData.ItemLocations.Inventory:
                            Inventory.inv.invSlots[item.slotNo].updateSlotContentsAndRefresh(customItem.invItem.getItemId(), item.stackSize);
                            TRTools.Log($"Add back in {customItem.invItem.itemName}");
                            break;

                        case ItemSaveData.ItemLocations.Chest:
                            tmpHouseDetails = item.HouseXPos == -1 ? null : HouseManager.manage.getHouseInfo(item.HouseXPos, item.HouseYPos);
                            ContainerManager.manage.changeSlotInChest(item.ObjectXPos, item.ObjectYPos, item.slotNo, customItem.invItem.getItemId(), item.stackSize, tmpHouseDetails);
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
                                        WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(item.ObjectXPos, item.ObjectYPos, false);

                                        //NetworkNavMesh.nav.updateChunkInUse();
                                        WorldManager.manageWorld.unlockClientTile(item.ObjectXPos, item.ObjectYPos);
                                    }
                                    break;

                                // Untested
                                case ItemSaveData.WorldObject.OnTop:
                                    tmpHouseDetails = item.HouseXPos == -1 ? null : HouseManager.manage.getHouseInfo(item.HouseXPos, item.HouseYPos);
                                    ItemOnTopManager.manage.placeItemOnTop(customItem.tileObject.tileObjectId, item.onTopPos, item.status, item.rotation, item.ObjectXPos, item.ObjectYPos, tmpHouseDetails);
                                    WorldManager.manageWorld.unlockClientTile(item.ObjectXPos, item.ObjectYPos);
                                    WorldManager.manageWorld.refreshAllChunksInUse(item.ObjectXPos, item.ObjectYPos, false);

                                    // Must check if localChar is null because it will fail on first load since HouseManager doesn't exist
                                    if (NetworkMapSharer.share.localChar) {
                                        var house = HouseManager.manage.findHousesOnDisplay(item.HouseXPos, item.HouseYPos);
                                        house.refreshHouseTiles();
                                    }
                                    break;

                                // TODO: Handle these below cases

                                case ItemSaveData.WorldObject.Bridge:
                                    TRTools.Log($"Starting Position: ({item.ObjectXPos}, {item.ObjectYPos}) | Rotation: {item.rotation} | Length: {item.bridgeLength}");
                                    customItem.tileObject.placeBridgeTiledObject(item.ObjectXPos, item.ObjectYPos, item.rotation, item.bridgeLength);
                                    WorldManager.manageWorld.refreshTileObjectsOnChunksInUse(item.ObjectXPos, item.ObjectYPos, false);
                                    WorldManager.manageWorld.unlockClientTile(item.ObjectXPos, item.ObjectYPos);

                                    break;

                                case ItemSaveData.WorldObject.Path:
                                    WorldManager.manageWorld.tileTypeMap[item.ObjectXPos, item.ObjectYPos] = item.tileType;
                                    WorldManager.manageWorld.refreshAllChunksInUse(item.ObjectXPos, item.ObjectYPos, false);
                                    break;
                            }
                            break;

                        case ItemSaveData.ItemLocations.HomeFloor: break;
                        case ItemSaveData.ItemLocations.HomeWall: break;

                    }
                }
            }
        }
    }

    [Serializable]
    public class TRCustomItem {

        // TODO: Implement events
        public delegate void TileObjectEvent();

        public AssetBundle assetBundle;
        public InventoryItem invItem;
        public TileObject tileObject;
        public TileObjectSettings tileObjectSettings;
        public TileTypes tileTypes;
        public string uniqueID;
        public TileObjectEvent interactEvent;

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
                if (this.tileTypes == null) {
                    TRTools.Log("Attemping Load: tileTypes.");
                    this.tileTypes = AllAssets[i].GetComponent<TileTypes>();
                    TRTools.Log($"Loaded: tileTypes -- {this.tileTypes}.");
                }
            }

            this.assetBundle.Unload(false);
        }
    }

    [Serializable]
    internal class ItemSaveData {
        public enum EquipLocation { Hat, Face, Shirt, Pants, Shoes }

        public enum ItemLocations { Inventory, Equipped, Chest, Stash, World, HomeFloor, HomeWall }
        public enum WorldObject { OnTile, OnTop, Bridge, Path }

        public string uniqueID;
        public ItemLocations location;
        public EquipLocation equipment;
        public WorldObject type;
        public int onTopPos;
        public int status;
        public int stashPostition;
        public int ObjectXPos;
        public int ObjectYPos;
        public int HouseXPos;
        public int HouseYPos;
        public int rotation;
        public int slotNo;
        public int stackSize;
        public int tileType;
        public int bridgeLength;

        // For saving items that were in the player's inventory
        public ItemSaveData(TRCustomItem customItem, int slotNo, int stackSize) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Inventory;
            this.slotNo = slotNo;
            this.stackSize = stackSize;
        }

        // For saving items that were in a chest
        public ItemSaveData(TRCustomItem customItem, int slotNo, int stackSize, int ObjectXPos, int ObjectYPos, int HouseXPos, int HouseYPos) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Chest;

            this.stackSize = stackSize;
            this.slotNo = slotNo;

            // Position of tile outside (the house in some cases)
            this.ObjectXPos = ObjectXPos;
            this.ObjectYPos = ObjectYPos;

            // Position of inside the house
            this.HouseXPos = HouseXPos;
            this.HouseYPos = HouseYPos;
        }

        // For saving items that out in the world
        public ItemSaveData(TRCustomItem customItem, int ObjectXPos, int ObjectYPos, int rotation, WorldObject type, int HouseXPos, int HouseYPos, int status = -1, int onTopPos = -1, int bridgeLength = -1) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.World;

            // Type of Object: OnTile, OnTop, Bridge
            this.type = type;
            this.rotation = rotation;

            // Position of tile outside (the house in some cases)
            this.ObjectXPos = ObjectXPos;
            this.ObjectYPos = ObjectYPos;

            // Position of inside the house
            this.HouseXPos = HouseXPos;
            this.HouseYPos = HouseYPos;

            // Status of current object
            this.status = status;

            // What y level (height) it is at
            this.onTopPos = onTopPos;

            this.bridgeLength = bridgeLength;
        }

        // Saving items in a stash
        public ItemSaveData(TRCustomItem customItem, int stackSize, int stashPostition, int slotNo) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Stash;
            this.stackSize = stackSize;
            this.stashPostition = stashPostition;
            this.slotNo = slotNo;
        }

        // Saving equipment slots
        public ItemSaveData(TRCustomItem customItem, EquipLocation equipment, int stackSize) {
            this.uniqueID = customItem.uniqueID;
            this.location = ItemLocations.Equipped;
            this.equipment = equipment;
            this.stackSize = stackSize;
        }

        public ItemSaveData(TRCustomItem customItem, WorldObject type, int objectXPos, int objectYPos, int tileType) {
            this.uniqueID = customItem.uniqueID;
            this.tileType = tileType;
            this.location = ItemLocations.World;
            this.ObjectXPos = objectXPos;
            this.ObjectYPos = objectYPos;
            this.type = type;
        }
    }

}
