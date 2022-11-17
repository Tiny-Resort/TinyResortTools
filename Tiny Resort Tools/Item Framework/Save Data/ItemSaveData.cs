using System;

namespace TinyResort; 

[Serializable]
internal class ItemSaveData {

    // The ID of the saved custom item (nexusID + uniqueItemID
    public string customItemID;

    // Position of the tile relative to world (or house if it is in one)
    public int objectXPos;
    public int objectYPos;

    //public int ObjectZPos;

    // Position of House within World
    public int houseXPos;
    public int houseYPos;

    // Inventory Info
    public int slotNo;
    public int stackSize;

    // Rotation of world objects such as furniture
    public int rotation;
}
