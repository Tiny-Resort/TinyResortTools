using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TinyResort;

// TODO: Add method for tracking development items turning into production items. 

/// <summary>Tools for quickly creating clothing items.</summary>
public class TRQuickItems {

    internal static List<string> filePaths = new();
    internal static List<string> arrayPaths = new();
    internal static string qversionPath;
    internal static List<QuickItemInfo> arrayItems = new();
    internal static AssetBundle quickItemsBundle = TRAssets.LoadAssetBundleFromDLL("quickitems_bundle");
    internal static List<string> currentCustomIDs = new();

    internal static void FindAllPaths(string initialDir) {
        foreach (var file in Directory.GetFiles(initialDir)) {
            if (Path.GetExtension(file) == ".qitem") filePaths.Add(file);
            if (Path.GetExtension(file) == ".qitems") arrayPaths.Add(file);
            if (Path.GetExtension(file) == ".qversion") qversionPath = file;
        }
        foreach (var dir in Directory.GetDirectories(initialDir)) FindAllPaths(dir);
    }

    internal static void InitializeCustomItem(string path, GameObject obj, QuickItemInfo itemInfo) {

        var fileName = Path.GetFileName(path);
        var folderName = Path.GetDirectoryName(path);
        var ext = Path.GetExtension(path);

        if (itemInfo.uniqueID < 0 || string.IsNullOrEmpty(itemInfo.fileName)) {
            TRTools.LogError("The unique ID or filename is missing from the .qitem file.");
            return;
        }

        if (TRItems.customItems.ContainsKey(itemInfo.nexusID + "." + itemInfo.uniqueID)
         || TRItems.customItems.ContainsKey(
                "QI." + folderName + "_" + itemInfo.itemName.Replace(" ", "") + ext.Replace(".", "_")
            )) {
            TRTools.LogError("A custom item with the same nexus ID and unique ID has already been loaded.");
            return;
        }

        // If no texture could be loaded, skip this one
        // Path Combine doesn't seem to work here? Acts like PLuginPath is empty...
        var texture = TRAssets.LoadTexture(Paths.PluginPath + path.Trim());
        var normalMap = string.IsNullOrWhiteSpace(itemInfo.normalMapFileName)
                            ? null
                            : TRAssets.LoadTexture(
                                Paths.PluginPath + path.Replace(fileName, itemInfo.normalMapFileName)
                            );
        if (!texture) {
            TRTools.LogError($"No texture was found at {Paths.PluginPath + path.Trim()}.");
            return;
        }

        // Creates a new instance of the item

        if (string.IsNullOrEmpty(folderName))
            folderName = "unknown";
        else
            folderName = folderName.Split('\\').Last().Replace(" ", "");

        var newItem = new TRCustomItem();
        newItem.inventoryItem = Object.Instantiate(obj).GetComponent<InventoryItem>();
        Object.DontDestroyOnLoad(newItem.inventoryItem);
        newItem.inventoryItem.itemName = itemInfo.itemName;
        newItem.inventoryItem.itemDescription = itemInfo.description;

        // Sets the texture for the item
        newItem.inventoryItem.equipable.material = new Material(newItem.inventoryItem.equipable.material);
        newItem.inventoryItem.equipable.material.mainTexture = texture;

        if (normalMap != null)
            newItem.inventoryItem.equipable.material.EnableKeyword("_NORMALMAP");
        else
            newItem.inventoryItem.equipable.material.DisableKeyword("_NORMALMAP");
        newItem.inventoryItem.equipable.material.SetTexture("_BumpMap", normalMap);
        newItem.inventoryItem.equipable.material.SetColor("_Color", Color.white);

        newItem.inventoryItem.equipable.material.name = itemInfo.fileName;

        newItem.isQuickItem = true;

        if (currentCustomIDs.Contains(itemInfo.nexusID + "." + itemInfo.uniqueID))
            TRTools.LogError($"The file {itemInfo.fileName} has the same unique ID as another item.");
        else
            currentCustomIDs.Add(itemInfo.nexusID + "." + itemInfo.uniqueID);

        if (itemInfo.nexusID <= 0 && LeadPlugin.developerMode.Value) {
            TRTools.LogError(
                "Loading a Quick Item in with a -1 Nexus ID. This is allowed since you are in the developer mode, but please update the files before release.(or notify mod author)."
            );
            newItem.customItemID =
                "QI." + folderName + "_" + itemInfo.itemName.Replace(" ", "") + ext.Replace(".", "_");
            if (!string.IsNullOrWhiteSpace(itemInfo.iconFileName))
                UpdateIcon(
                    newItem.inventoryItem, Paths.PluginPath + path.Replace(itemInfo.fileName, itemInfo.iconFileName)
                );
            TRItems.customItems[newItem.customItemID] = newItem;
        }
        else if (itemInfo.nexusID > 0) {
            newItem.customItemID = itemInfo.nexusID + "." + itemInfo.uniqueID;
            if (!string.IsNullOrWhiteSpace(itemInfo.iconFileName))
                UpdateIcon(
                    newItem.inventoryItem, Paths.PluginPath + path.Replace(itemInfo.fileName, itemInfo.iconFileName)
                );
            TRItems.customItems[newItem.customItemID] = newItem;
        }
        else {
            TRTools.LogError(
                "Loading a Quick Item in with a -1 Nexus ID. This is not allowed and will be blocked. If you are a developer, please turn on developer mode."
            );
        }

        if (TRItems.customItems.ContainsKey(newItem.customItemID)
         && itemInfo.type.ToLower().Replace(" ", "").Trim() == "path") {
            newItem.tileTypes = Object.Instantiate(
                quickItemsBundle.LoadAsset<GameObject>("pathTileType").GetComponent<TileTypes>()
            );
            Object.DontDestroyOnLoad(newItem.tileTypes);
            newItem.tileTypes.myTileMaterial = new Material(newItem.inventoryItem.equipable.material);
            newItem.tileTypes.dropOnChange = newItem.inventoryItem;
        }

        //TRTools.Log($"Custom ID: {newItem.customItemID}");
        newItem.inventoryItem.value = itemInfo.value;

    }

    internal static void UpdateIcon(InventoryItem invItem, string path) {
        TRIcons.itemList.Add(invItem.itemName);
        var addNewSprite = new TRIcons.CustomSprites();
        var sprite = TRAssets.LoadSprite(path, Vector2.one * 0.5f);
        if (sprite != null) {
            invItem.itemSprite = sprite;
            addNewSprite.itemName = invItem.itemName.ToLower().Replace(" ", "_");
            addNewSprite.customSprite = sprite;
            addNewSprite.spritePath = path;
            TRIcons.proccessedQIItemList.Add(addNewSprite);
        }
    }

    internal static void LoadItem(string path) {
        var oneItem = QuickItemInfo.CreateFromJson(File.ReadAllText(path));

        var relativePath = path.Remove(0, Paths.PluginPath.Length).Replace(Path.GetFileName(path), oneItem.fileName);
        try {
            var asset = quickItemsBundle.LoadAsset<GameObject>(oneItem.type.ToLower().Replace(" ", ""));
            InitializeCustomItem(relativePath, asset, oneItem);
        }
        catch {
            TRTools.LogError(
                "Missing or incorrect Item Type. Please refer to the documentation for a list of available options."
            );
        }
    }

    internal static void LoadArrayItems(string path) {
        var jsonList = JsonConvert.DeserializeObject<List<QuickItemInfo>>(File.ReadAllText(path));
        if (jsonList != null)
            foreach (var item in jsonList) {
                var relativePath = path.Remove(0, Paths.PluginPath.Length)
                                       .Replace(Path.GetFileName(path), item.fileName);
                try {
                    var asset = quickItemsBundle.LoadAsset<GameObject>(item.type.ToLower().Replace(" ", ""));
                    InitializeCustomItem(relativePath, asset, item);
                }
                catch {
                    TRTools.LogError(
                        "Missing or incorrect Item Type. Please refer to the documentation for a list of available options."
                    );
                }
            }
    }

    internal static void LoadAllQuickItems() {
        FindAllPaths(Paths.PluginPath);
        foreach (var item in arrayPaths) LoadArrayItems(item);
        foreach (var item in filePaths) LoadItem(item);

        if (!string.IsNullOrWhiteSpace(qversionPath))
            TRModUpdater.QuickItemInfo.Add(QIModInfo.CreateFromJson(File.ReadAllText(qversionPath)));
    }
}

#pragma warning disable CS1591
[Serializable]
public class QuickItemInfo {

    // Mod Info
    public int nexusID;
    public int uniqueID = -1;

    // The file name for the default texture
    public string fileName;

    // The type of item being used
    public string type;

    // Default Item information
    public string itemName = "Item That Must Not Be Named";
    public string description = "What a wonderful use of a quick item's description!";
    public int value = 1000;

    // Setting the normal map for items
    public string normalMapFileName;

    // For setting custom icon name instead of using item_icons folder
    public string iconFileName;

    public static QuickItemInfo CreateFromJson(string jsonString) => JsonUtility.FromJson<QuickItemInfo>(jsonString);
}

/// <summary> Please ignore! This is only for internal use but must be public in order for it to be loadable from JSON. </summary>
[Serializable]
public class QIModInfo {

    // Mod Info
    public int nexusID;
    public string version;
    public string modName;

    public static QIModInfo CreateFromJson(string jsonString) => JsonUtility.FromJson<QIModInfo>(jsonString);
}
#pragma warning restore CS1591
