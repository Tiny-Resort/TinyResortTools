using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyResort;

public class TRStorage
{
    /// <summary>Information and functions related to your custom storage.</summary>
    [Serializable]
    public class TRCustomStorage {
        // Inventory Item is not Serializable, so we need to store only the ID
        public int itemID { get; set; }
        public int stackSize { get; set; }
    
        // Add an equality comparer based on the item ID
        public override bool Equals(object obj)
        {
            if (obj is TRCustomStorage other)
            {
                return itemID == other.itemID;
            }
            return false;
        }
    
        public override int GetHashCode()
        {
            return itemID.GetHashCode();
        }
    }
  
    
    private static TRModData Data;

    private static List<TRCustomStorage> customInventory;
    
    // Method to safely add items
    public static bool TryAddItem(InventoryItem itemDetails, int stackSize)
    {
        // Safety check - initialize if null
        if (customInventory == null)
            customInventory = new List<TRCustomStorage>();
            
        var newStorage = new TRCustomStorage { itemID = itemDetails.itemId, stackSize = stackSize };
    
        // Check if item already exists
        var existingItem = customInventory.FirstOrDefault(x => x.Equals(newStorage));
        if (existingItem != null)
        {
            // Increase the stack size of the existing item
            existingItem.stackSize += stackSize;
            return true;
        }
    
        // Item doesn't exist, add it
        customInventory.Add(newStorage);
        return true;
    }
    
    // Method to safely remove items
    public static bool TryRemoveItem(InventoryItem itemDetails, int stackSize)
    {
        // Safety check - initialize if null
        if (customInventory == null)
            return false;
            
        // Create a temporary storage object to use for comparison
        var tempStorage = new TRCustomStorage { itemID = itemDetails.itemId, stackSize = 0 };
        
        // Find the existing item
        var existingItem = customInventory.FirstOrDefault(x => x.Equals(tempStorage));
        if (existingItem == null)
        {
            // Item doesn't exist in inventory
            return false;
        }
        
        // Decrease the stack size
        existingItem.stackSize -= stackSize;
        
        // If stack size is 0 or less, remove the item entirely
        if (existingItem.stackSize <= 0)
        {
            customInventory.Remove(existingItem);
        }
        
        return true;
    }
    
    internal static void Initialize() {
        // Initialize storage list
        customInventory = new List<TRCustomStorage>();

        /*TRTools.QuickPatch(typeof(LicenceManager), "Start", typeof(TRLicences), "StartPatch");
        TRTools.QuickPatch(typeof(LicenceManager), "getLicenceName", typeof(TRLicences), "getLicenceNamePatch");
        TRTools.QuickPatch(
            typeof(LicenceManager), "getLicenceLevelDescription", typeof(TRLicences), "getLicenceLevelDescriptionPatch"
        );
        TRTools.QuickPatch(typeof(Licence), "getNextLevelPrice", typeof(TRLicences), "getNextLevelPricePrefix");
        TRTools.QuickPatch(typeof(Licence), "canAffordNextLevel", typeof(TRLicences), "canAffordNextlevelPrefix");
        TRTools.QuickPatch(typeof(Licence), "getCurrentMaxLevel", typeof(TRLicences), "getCurrentMaxLevelPrefix");
        TRTools.QuickPatch(typeof(LicenceButton), "fillButton", typeof(TRLicences), null, "fillButtonPostfix");
        */

        LeadPlugin.plugin.AddCommand(
            "add_item",
            "Add item to TRStorage.",
            AddItem, "LicenceName", "Level"
        );
        LeadPlugin.plugin.AddCommand(
            "remove_item",
            "Remove item from TRStorage.",
            RemoveItem, "LicenceName", "Level"
        );
        LeadPlugin.plugin.AddCommand("list_items", "Lists all items.", ListInventory);
        

        Data = TRData.Subscribe("TR.TinyStorage");
        //TRData.cleanDataEvent += UnloadLicences;
        TRData.preSaveEvent += SaveStorageData;
        TRData.postLoadEvent += LoadStorageData;
        //TRData.injectDataEvent += LoadStorageData;

        /*defaultLicenceSprite = TRAssets.LoadTextureFromAssetBundle(
            TRAssets.LoadAssetBundleFromDLL("licenceimages"), "default_licence", Vector2.one * 0.5f
        );*/
    }

    
    #region Saving & Loading Data
    

    internal static void SaveStorageData() {
        try {
            // Make sure we're not null before saving
            if (customInventory == null)
                customInventory = new List<TRCustomStorage>();
            
            Data.SetValue("TRStorage", customInventory);
        }
        catch (Exception ex) {
            TRTools.LogError("Error saving storage data: " + ex.Message);
        }
    }

    internal static void LoadStorageData()
    {
        try
        {
            var val = Data.GetValue("TRStorage");
            if (val == null)
                customInventory = new List<TRCustomStorage>();
            else
            {
                customInventory = (List<TRCustomStorage>)val;
            }
        }
        catch (Exception ex)
        {
            TRTools.LogError("Error loading storage data: " + ex.Message);
            TRTools.Log("Creating new storage as previous storage couldn't be loaded");
            customInventory = new List<TRCustomStorage>();
            
            // Remove the problematic data
            Data.Remove("TRStorage");
        }
    }
    
    #endregion

    
    /// <returns>The details for an item with the given item ID.</returns>
    public static InventoryItem GetItemDetails(int itemID) {
        if (itemID >= 0 && itemID < Inventory.Instance.allItems.Length) return Inventory.Instance.allItems[itemID];
        if (itemID == -1) return null;
        TRTools.LogError("Attempting to get item details for item with ID of " + itemID + " which does not exist.");
        return null;
    }
    internal static string AddItem(string[] args) //, InventoryItem item)
    {
        var itemDetails = GetItemDetails(int.TryParse(args[0], out var itemID) ? itemID : -1);
        if (itemDetails == null) return "Item with ID " + args[0] + " does not exist.";
        var stackSize = int.TryParse(args[1], out var stackSizeInt) ? stackSizeInt : 0;
        if (stackSizeInt < 1) return "Stack size must be greater than 0.";
        
        TryAddItem(itemDetails, stackSize);
        return $"Added {stackSize} {itemDetails.itemName} to the storage.";
    }
    
    internal static string RemoveItem(string[] args) //, InventoryItem item)
    {
        var itemDetails = GetItemDetails(int.TryParse(args[0], out var itemID) ? itemID : -1);
        if (itemDetails == null) return "Item with ID " + args[0] + " does not exist.";
        var stackSize = int.TryParse(args[1], out var stackSizeInt) ? stackSizeInt : 0;
        if (stackSizeInt < 1) return "Stack size must be greater than 0.";
        
        if (!TryRemoveItem(itemDetails, stackSize))
        {
            return $"Could not find {itemDetails.itemName} in storage.";
        }
        
        return $"Removed {stackSize} {itemDetails.itemName} from storage.";
    }

    // For Testing Purposes
    internal static string ListInventory(string[] args)
    {
        // Safety check - initialize if null
        if (customInventory == null)
        {
            customInventory = new List<TRCustomStorage>();
            return "Storage is empty.";
        }
        
        if (customInventory.Count == 0)
            return "Storage is empty.";
            
        foreach (var storage in customInventory)
        { 
            var item = GetItemDetails(storage.itemID);
            if (item != null)
            {
                TRChat.SendMessage($"{item.itemId} - {item.itemName} (x{storage.stackSize})");
            }
            else
            {
                TRChat.SendMessage($"Unknown item ID {storage.itemID} (x{storage.stackSize})");
            }
        }

        return "Listed all items in storage.";
    }
}