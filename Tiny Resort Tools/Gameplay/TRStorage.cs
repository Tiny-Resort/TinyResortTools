using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TinyResort;
public class TRStorage
{
    private static TRModData Data;
    private static float scrollPosition;
    private static float scrollMaxPosition;
    
    private static List<TRCustomStorage> customInventory;
    private static GameObject modsWindow;
    private static GameObject creditsWindow;
    private static GameObject canvas;

    private static RectTransform updateButtonGrid;
    
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
    
    public static void Update() {

        // Creates the mods update checker window and button to open window if they aren't created yet
        if (WorldManager.Instance && !creditsWindow) CreateInventoryUI();

        if (modsWindow && modsWindow.activeInHierarchy) {
            scrollPosition = Mathf.Clamp(scrollPosition - Input.mouseScrollDelta.y, 0, scrollMaxPosition);
            updateButtonGrid.anchoredPosition = new Vector2(0, scrollPosition * 58);
        }

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
    
    internal static void CreateInventoryUI()
    {
        TRTools.Log("Starting CreateInventoryUI method");

        if (!creditsWindow)
        {

            creditsWindow = GameObject.Find("MapCanvas/MenuScreen/Credits");

            if (creditsWindow)
            {
                TRTools.Log("Creating Inventory window from credits window template");

                // Create and setup a window for displaying update buttons for mods
                modsWindow = Object.Instantiate(creditsWindow, creditsWindow.transform.parent.parent);
                modsWindow.name = "Inventory Window";
                TRTools.Log("Inventory window instantiated");

                modsWindow.transform.GetChild(0).name = "Inventory Window Internals";
                TRTools.Log("Renamed first child to Mod Window Internals");

                // Add the Dinkum Mods logo at the top of the updater window

                var modLogo = modsWindow.transform.GetChild(0).GetChild(7).GetComponent<Image>();
                modLogo.rectTransform.anchoredPosition += new Vector2(0, -30);
                modLogo.rectTransform.sizeDelta = new Vector2(modLogo.rectTransform.sizeDelta.x, 250);
                modLogo.sprite = TRInterface.ModLogo;
                modLogo.name = "Dinkum Mods Logo";


                // Destroy all unused children
                Object.Destroy(modsWindow.transform.GetChild(0).GetChild(2).gameObject); // Title  
                Object.Destroy(modsWindow.transform.GetChild(0).GetChild(3).gameObject); // Music
                Object.Destroy(modsWindow.transform.GetChild(0).GetChild(4).gameObject); // VoicesBy
                Object.Destroy(modsWindow.transform.GetChild(0).GetChild(5).gameObject); // Special Thanks
                Object.Destroy(modsWindow.transform.GetChild(0).GetChild(6).gameObject); // Acknowledgements
                Object.Destroy(modsWindow.transform.GetChild(0).GetChild(8).gameObject); // Additional Dialogue
                Object.Destroy(modsWindow.transform.GetChild(1).gameObject); // License Button (top right)


                var scrollArea = new GameObject("Inventory Items Scroll Area");
                scrollArea.transform.SetParent(modsWindow.transform.GetChild(0));
                scrollArea.transform.SetAsLastSibling();
                var scrollAreaImage = scrollArea.AddComponent<Image>();
                scrollAreaImage.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                scrollAreaImage.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                scrollAreaImage.rectTransform.pivot = new Vector2(0.5f, 1f);
                scrollAreaImage.rectTransform.sizeDelta = new Vector2(550, 340);
                scrollAreaImage.rectTransform.anchoredPosition = new Vector2(0, -155f);
                scrollAreaImage.color = Color.red;
                scrollAreaImage.rectTransform.localScale = Vector3.one;
                scrollArea.AddComponent<Mask>().showMaskGraphic = false;

                // Create a UI grid for the update buttons to go in
                var updateButtonObj = new GameObject("Inventory Item Grid");
                updateButtonObj.transform.SetParent(scrollArea.transform);
                updateButtonObj.transform.SetAsLastSibling();
                var gridLayoutGroup = updateButtonObj.AddComponent<GridLayoutGroup>();
                gridLayoutGroup.cellSize = new Vector2(500, 50);
                gridLayoutGroup.spacing = new Vector2(8, 8);
                gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = 1;
                updateButtonGrid = updateButtonObj.GetComponent<RectTransform>();
                updateButtonGrid.pivot = new Vector2(0.5f, 1);
                updateButtonGrid.anchorMax = new Vector2(0.5f, 1);
                updateButtonGrid.anchorMin = new Vector2(0.5f, 1);
                updateButtonGrid.localScale = Vector3.one;
                updateButtonGrid.anchoredPosition = new Vector2(0, 0);

            }
        }
    }

    
    /*private static void PopulateModList() {

            scrollPosition = 0;
            scrollMaxPosition = Mathf.Max(customInventory.Count - 6, 0);

            foreach (var item in customInventory) {

                // If a button already exists for this mod, move on
                if (item.itemSlot != null) continue;

                // Create a button for each mod, indicating if it has an update available with link to mod page on nexus
                if (mod.updateState == PluginUpdateState.NotSetUp) {
                    if (!showUnknownNexusID.Value) continue;

                    mod.updateButton = updateButton.Copy(
                        updateButtonGrid.transform,
                        $"<size=15>{mod.name}</size>\n<color=#787877FF>Status Unknown: Missing NexusID</color>",
                        delegate {
                            openWebpage(
                                "https://modding.wiki/en/dinkum/TRTools/ModManager#why-isnt-x-mod-showing-up-in-the-update-checker"
                            );
                        }
                    );
                }

                // Create a button for each mod, indicating if it has an update available with link to mod page on nexus
                else {
                    if (mod.updateState == PluginUpdateState.UpToDate && !showUpToDateMods.Value) continue;

                    mod.updateButton = updateButton.Copy(
                        updateButtonGrid.transform,
                        mod.updateState == PluginUpdateState.UpdateAvailable
                            ? $"<size=15>{mod.name}</size>\n<color=#00ff00ff><b>UPDATE AVAILABLE</b></color> (<color=#ff7226ff>{mod.modVersion}</color> -> <color=#00ff00ff>{mod.nexusVersion}</color>)"
                            : $"<size=15>{mod.name}</size>\n<color=#787877FF>UP TO DATE ({mod.modVersion})</color>",
                        delegate {
                            openWebpage(string.Format("https://www.nexusmods.com/dinkum/mods/{0}/?tab=files", mod.id));
                        }
                    );
                }

            }

            // Organize the mod update buttons with mods that have an update available at the top
            loadedPlugins = loadedPlugins.OrderBy(i => i.updateState).ThenBy(i => i.name).ToList();
            for (var i = 0; i < loadedPlugins.Count; i++) loadedPlugins[i].updateButton.transform.SetSiblingIndex(i);

        }*/
    
    internal static string AddItem(string[] args) //, InventoryItem item)
    {
        var itemDetails = TRItems.GetItemDetails(int.TryParse(args[0], out var itemID) ? itemID : -1);
        if (itemDetails == null) return "Item with ID " + args[0] + " does not exist.";
        var stackSize = int.TryParse(args[1], out var stackSizeInt) ? stackSizeInt : 0;
        if (stackSizeInt < 1) return "Stack size must be greater than 0.";
        
        TryAddItem(itemDetails, stackSize);
        return $"Added {stackSize} {itemDetails.itemName} to the storage.";
    }
    
    internal static string RemoveItem(string[] args) //, InventoryItem item)
    {
        var itemDetails = TRItems.GetItemDetails(int.TryParse(args[0], out var itemID) ? itemID : -1);
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
            var item = TRItems.GetItemDetails(storage.itemID);
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
    
    internal static void OpenInventoryWindow()
    {
        modsWindow.gameObject.SetActive(!modsWindow.gameObject.activeSelf);

    }

    /// <summary>Information and functions related to your custom storage.</summary>
    [Serializable]
    public class TRCustomStorage
    {
        // Inventory Item is not Serializable, so we need to store only the ID
        public int itemID { get; set; }
        public int stackSize { get; set; }
        private static TRButton itemSlot;

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
}