using System;
using UnityEngine;

namespace TinyResort {

    // My idea of this was to remove all/most of these variables. The variables would all be in their own class dependent on their type. 
    // We would want to keep the Enum and also the Unique ID, but otherwise all the rest would get removed and used in each of their own classes. 
    [Serializable]
    internal class ItemSaveData {
        
        public enum EquipLocation { Hat, Face, Shirt, Pants, Shoes }

        public enum ItemLocations { Inventory, Equipped, Chest, Stash, World, HomeFloor, HomeWall, Letter, Buried, Dropped, Vehicle, Carry }
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

        // We would create a default for each class type of item
        public SavedLetter letter = new SavedLetter();
        public SavedBuriedItems buriedItem = new SavedBuriedItems();
        public SavedDroppedItem droppedItem = new SavedDroppedItem();
        public SavedVehicle vehicle = new SavedVehicle();
        public SavedWallpaper wallpaper = new SavedWallpaper();
        public SavedFlooring flooring = new SavedFlooring();
        public SavedCarryable carry = new SavedCarryable();

        public ItemSaveData() { }

        // If we create a initialize in each of the classes, this would just end up 2-3 lines for each Store method. 

        // Create a new ItemSaveData, Add all of the appropriate information needed, and return the ItemSaveData
        // This will be called to gather al of the data and add it to the approproiate list for when it needs to be restored. 
        public ItemSaveData StoreDroppedItem(DroppedItem toRemove, string ID) {
            ItemSaveData tmpDroppedItem = new ItemSaveData();

            tmpDroppedItem.droppedItem.myItemId = toRemove.myItemId;
            tmpDroppedItem.droppedItem.stackAmount = toRemove.stackAmount;
            tmpDroppedItem.droppedItem.desiredPositionX = toRemove.desiredPos.x;
            tmpDroppedItem.droppedItem.desiredPositionY = toRemove.desiredPos.y;
            tmpDroppedItem.droppedItem.desiredPositionZ = toRemove.desiredPos.z;
            if (toRemove.inside != null) {
                tmpDroppedItem.droppedItem.houseX = toRemove.inside.xPos;
                tmpDroppedItem.droppedItem.houseY = toRemove.inside.yPos;
            }
            tmpDroppedItem.droppedItem.saveDrop = toRemove.saveDrop;
            tmpDroppedItem.droppedItem.underground = toRemove.underground;

            tmpDroppedItem.location = ItemLocations.Dropped;
            tmpDroppedItem.uniqueID = ID;

            return tmpDroppedItem;
        }

        public ItemSaveData StoreLetter(Letter toRemove, bool tomorrow, string uniqueID) {
            ItemSaveData storeLetter = new ItemSaveData();

            storeLetter.letter.itemAttached = toRemove.itemAttached;
            storeLetter.letter.itemOriginallAttached = toRemove.itemOriginallAttached;
            storeLetter.letter.stackOfItemAttached = toRemove.stackOfItemAttached;
            storeLetter.letter.myType = toRemove.myType;
            storeLetter.letter.seasonSent = toRemove.seasonSent;
            storeLetter.letter.letterTemplateNo = toRemove.letterTemplateNo;
            storeLetter.letter.sentById = toRemove.sentById;
            storeLetter.letter.hasBeenRead = toRemove.hasBeenRead;
            storeLetter.location = ItemLocations.Letter;
            storeLetter.letter.tomorrow = tomorrow;

            storeLetter.uniqueID = uniqueID;
            return storeLetter;
        }

        public ItemSaveData StoreBuriedItem(BuriedItem toRemove, string uniqueID) {
            ItemSaveData tmpBuriedItem = new ItemSaveData();

            tmpBuriedItem.buriedItem.itemId = toRemove.itemId;
            tmpBuriedItem.buriedItem.stack = toRemove.stackedAmount;
            tmpBuriedItem.buriedItem.xP = toRemove.xPos;
            tmpBuriedItem.buriedItem.yP = toRemove.yPos;

            tmpBuriedItem.location = ItemLocations.Buried;
            tmpBuriedItem.uniqueID = uniqueID;
            return tmpBuriedItem;
        }

        public ItemSaveData StoreVehicle(Vehicle toRemove, string ID) {
            ItemSaveData tmpVehicle = new ItemSaveData();
            tmpVehicle.uniqueID = ID;
            tmpVehicle.location = ItemLocations.Vehicle;
            tmpVehicle.vehicle.Initialize(toRemove);
            return tmpVehicle;
        }

        public ItemSaveData StoreWallpaper(HouseDetails house, string ID) {
            ItemSaveData tmpWallpaper = new ItemSaveData();
            tmpWallpaper.wallpaper.wallpaperID = house.wall;
            tmpWallpaper.wallpaper.houseX = house.xPos;
            tmpWallpaper.wallpaper.houseY = house.yPos;
            tmpWallpaper.uniqueID = ID;

            tmpWallpaper.location = ItemLocations.HomeWall;

            return tmpWallpaper;
        }

        public ItemSaveData StoreFlooring(HouseDetails house, string ID) {
            ItemSaveData tmpFlooring = new ItemSaveData();
            tmpFlooring.flooring.flooringID = house.floor;
            tmpFlooring.flooring.houseX = house.xPos;
            tmpFlooring.flooring.houseY = house.yPos;
            tmpFlooring.uniqueID = ID;

            tmpFlooring.location = ItemLocations.HomeFloor;

            return tmpFlooring;
        }

        // Example of using initialize to make it cleaner here
        public ItemSaveData StoreCarry(PickUpAndCarry myCarry, string ID) {
            ItemSaveData tmpCarry = new ItemSaveData();
            tmpCarry.carry.Initialize(myCarry);
            tmpCarry.uniqueID = ID;

            tmpCarry.location = ItemLocations.Carry;

            return tmpCarry;
        }

        // The original way we were doing it and is much much messier...
        // We sent in specific information and "hoped" it would be unique enough to find the correct method. 

        // For saving items that were in the player's inventory
        public ItemSaveData(TRCustomItem customItem, int slotNo, int stackSize) {
            uniqueID = customItem.uniqueID;
            location = ItemLocations.Inventory;
            this.slotNo = slotNo;
            this.stackSize = stackSize;
        }

        // For saving items that were in a chest
        public ItemSaveData(TRCustomItem customItem, int slotNo, int stackSize, int ObjectXPos, int ObjectYPos, int HouseXPos, int HouseYPos) {
            uniqueID = customItem.uniqueID;
            location = ItemLocations.Chest;

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
            uniqueID = customItem.uniqueID;
            location = ItemLocations.World;

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
            uniqueID = customItem.uniqueID;
            location = ItemLocations.Stash;
            this.stackSize = stackSize;
            this.stashPostition = stashPostition;
            this.slotNo = slotNo;
        }

        // Saving equipment slots
        public ItemSaveData(TRCustomItem customItem, EquipLocation equipment, int stackSize) {
            uniqueID = customItem.uniqueID;
            location = ItemLocations.Equipped;
            this.equipment = equipment;
            this.stackSize = stackSize;
        }

        // Pretty sure this was for paths. 
        public ItemSaveData(TRCustomItem customItem, WorldObject type, int objectXPos, int objectYPos, int tileType) {
            uniqueID = customItem.uniqueID;
            this.tileType = tileType;
            location = ItemLocations.World;
            ObjectXPos = objectXPos;
            ObjectYPos = objectYPos;
            this.type = type;
        }
    }

    [Serializable]
    internal class SavedLetter {
        public int itemAttached;
        public int itemOriginallAttached;
        public int stackOfItemAttached;
        public Letter.LetterType myType;
        public int seasonSent;
        public int letterTemplateNo;
        public int sentById;
        public bool hasBeenRead;
        public bool tomorrow;

        public Letter Restore() {
            Letter tmp = new Letter(this.sentById, this.myType, this.itemAttached, this.stackOfItemAttached);
            tmp.itemOriginallAttached = this.itemOriginallAttached;
            tmp.seasonSent = this.seasonSent;
            tmp.hasBeenRead = this.hasBeenRead;
            return tmp;
        }
    }

    [Serializable]
    internal class SavedDroppedItem {
        // Cant serialize HouseDetails
        // House data isnt saved right now...
        public bool inside;
        public int myItemId;
        public int stackAmount;
        public float desiredPositionX;
        public float desiredPositionY;
        public float desiredPositionZ;
        public int houseX = -1;
        public int houseY = -1;
        public bool saveDrop;
        public bool underground;

        // Create a new DropToSave and then use that to run spawnDrop to place it back onto the floor. This doesn't currently run since drops aren't saved. 
        public void Restore() {
            DropToSave tmpToDrop = new DropToSave(this.myItemId, this.stackAmount, new Vector3(this.desiredPositionX, this.desiredPositionY, this.desiredPositionZ), this.houseX, this.houseY);
            tmpToDrop.spawnDrop();
        }

    }

    [Serializable]
    internal class SavedBuriedItems {
        public int itemId;
        public int stack;
        public int xP;
        public int yP;

        // Creates a new BuriedItem and returns it. This is used to a list and is eventually added to the allBurriedItems list
        public BuriedItem Restore() { return new BuriedItem(this.itemId, this.stack, this.xP, this.yP); }
    }

    [Serializable]
    internal class SavedVehicle {
        public int vehicleId;
        public int colourVariation;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationX;
        public float rotationY;
        public float rotationZ;

        public string uniqueID;

        // Initiailizing vehicle felt clunky so I made a Initialize method. This is the date James uses when laoded a vehicle. 
        public void Initialize(Vehicle toSave) {
            this.vehicleId = toSave.saveId;
            this.colourVariation = toSave.getVariation();
            this.positionX = toSave.transform.position.x;
            this.positionY = toSave.transform.position.y;
            this.positionZ = toSave.transform.position.z;
            this.rotationX = toSave.transform.eulerAngles.x;
            this.rotationY = toSave.transform.eulerAngles.y;
            this.rotationZ = toSave.transform.eulerAngles.z;
        }

        // This is his method for spawning a vehicle back into the game. 
        public void Restore() {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SaveLoad.saveOrLoad.vehiclePrefabs[vehicleId], new Vector3(positionX, positionY, positionZ), Quaternion.Euler(rotationX, rotationY, rotationZ));
            gameObject.GetComponent<Vehicle>().setVariation(colourVariation);
            NetworkMapSharer.share.spawnGameObject(gameObject);
        }
    }

    [Serializable]
    internal class SavedWallpaper {
        public int houseX;
        public int houseY;
        public int wallpaperID;

        // Get the current house with the x,y position and then set the house.wall to the customID
        public void Restore() {
            var house = HouseManager.manage.getHouseInfo(houseX, houseY);
            house.wall = wallpaperID;
        }
    }

    [Serializable]
    internal class SavedFlooring {
        public int houseX;
        public int houseY;
        public int flooringID;

        // The same as wallpaper
        public void Restore() {
            var house = HouseManager.manage.getHouseInfo(houseX, houseY);
            house.floor = flooringID;
        }
    }

    [Serializable]
    internal class SavedCarryable {

        public bool farmAnimalBox;
        public bool trappedAnimal;
        public int animalId;
        public int animalVariation;
        public string animalName;

        public int carryablePrefabId;
        public float positionX;
        public float positionY;
        public float positionZ;

        // Iniatialize a new carryable item. These are how he does it in his code. 
        public void Initialize(PickUpAndCarry myCarry) {
            AnimalCarryBox component = myCarry.GetComponent<AnimalCarryBox>();
            if (component) {
                this.farmAnimalBox = true;
                this.trappedAnimal = false;
                this.animalId = component.animalId;
                this.animalVariation = component.variation;
                this.animalName = component.animalName;
            }
            TrappedAnimal component2 = myCarry.GetComponent<TrappedAnimal>();
            if (component2) {
                this.farmAnimalBox = false;
                this.trappedAnimal = true;
                this.animalId = component2.trappedAnimalId;
                this.animalVariation = component2.trappedAnimalVariation;
            }
            this.carryablePrefabId = myCarry.prefabId;
            this.positionX = myCarry.transform.position.x;
            this.positionY = myCarry.transform.position.y;
            this.positionZ = myCarry.transform.position.z;
        }

        // Spawns a carryable with the initialized information
        public void Restore() { NetworkMapSharer.share.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[carryablePrefabId], new Vector3(positionX, positionY, positionZ), false); }

    }

}
