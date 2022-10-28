using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using I2.Loc.SimpleJSON;
using Mirror;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace TinyResort {

    /// <summary>Tools for quickly creating clothing items.</summary>
    public class TRQuickItems {
        internal static List<string> pathsArray = new List<string>();
        internal static List<string> paths = new List<string>();
        internal static AssetBundle customClothingBundle = TRAssets.LoadAssetBundleFromDLL("clothing_bundle");

        internal static void FindAllPaths(string initialDir) {
            foreach (string file in Directory.GetFiles(initialDir)) {
                if (Path.GetExtension(file) == ".qitem") {
                    if (Path.GetFileNameWithoutExtension(file) == "items") { pathsArray.Add(file); }
                    else { paths.Add(file); }
                }
            }
            foreach (string dir in Directory.GetDirectories(initialDir)) { FindAllPaths(dir); }
        }

        internal static void InitializeCustomItem(string path, GameObject obj, QuickItems item) {

            // If no texture could be loaded, skip this one
            // Path Combine doesn't seem to work here? Acts like PLuginPath is empty...
            var texture = TRAssets.LoadTexture(Paths.PluginPath + path.Trim());
            if (!texture) return;

            // Creates a new instance of the item
            var fileName = Path.GetFileNameWithoutExtension(path);
            var folderName = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(folderName)) { folderName = "unknown"; }
            else { folderName = folderName.Split('\\').Last().Replace(" ", ""); }

            var ext = Path.GetExtension(path);

            var newItem = new TRCustomItem();
            newItem.inventoryItem = Object.Instantiate(obj).GetComponent<InventoryItem>();
            GameObject.DontDestroyOnLoad(newItem.inventoryItem);
            newItem.inventoryItem.itemName = item.itemName;

            // Sets the texture for the item (TODO: Would need to be set up to work with non-clothing quick items)
            newItem.inventoryItem.equipable.material = new Material(newItem.inventoryItem.equipable.material);
            newItem.inventoryItem.equipable.material.mainTexture = texture;

            newItem.isQuickItem = true;
            if (item.nexusID <= 0 && LeadPlugin.developerMode.Value) {
                TRTools.LogError($"Loading a Quick Item in with a -1 Nexus ID. This is allowed since you are in the developer mode, but please update the files before release.(or notify mod author).");
                newItem.customItemID = "QI." + folderName + "_" + item.itemName.Replace(" ", "") + ext.Replace(".", "_");
                TRItems.customItems[newItem.customItemID] = newItem;
            }
            else if (!LeadPlugin.developerMode.Value && item.nexusID > 0) { 
                newItem.customItemID = item.nexusID + "." + item.uniqueID;
                TRItems.customItems[newItem.customItemID] = newItem;
            }
            else {
                TRTools.LogError($"Loading a Quick Item in with a -1 Nexus ID. THis is not allowed and will be blocked. If you are a developer, please turn on developer mode.");
            }
            
            TRTools.Log($"Custom ID: {newItem.customItemID}");
            newItem.inventoryItem.value = item.itemValue;

        }

        internal static void LoadItem(string path) {
            string json = File.ReadAllText(path);
            var oneItem = QuickItems.CreateFromJson(json);

            string relativePath = path.Remove(0, Paths.PluginPath.Length).Replace(Path.GetFileName(path), oneItem.fileName);
            InitializeCustomItem(relativePath, customClothingBundle.LoadAsset<GameObject>(oneItem.itemType), oneItem);

            TRTools.Log($"Single Item: {oneItem.fileName} | {oneItem.itemName} | {oneItem.itemType} | {oneItem.itemValue}");
        }

        // This doesn't work yet...
        internal static void LoadArrayItem(string path) {
            string json = File.ReadAllText(path);
            var allItems = JsonUtility.FromJson<RootQuickItems>(json);

            try {
                TRTools.LogError($"Paths Array Size: {allItems.items.Count}");
                foreach (var item in allItems.items) {
                    string relativePath = path.Remove(0, Paths.PluginPath.Length).Replace(Path.GetFileName(path), item.fileName);
                    TRTools.LogError($"Multilisted Item: {item.fileName} | {item.itemName} | {item.itemType} | {item.itemValue}");
                    //InitializeCustomItem(relativePath, customClothingBundle.LoadAsset<GameObject>(item.itemType), item);
                }
            }
            catch { TRTools.LogError($"MultiItems Failed"); }
        }

        internal static void LoadAllQuickItems() {
            FindAllPaths(Paths.PluginPath);

            foreach (var item in paths) { LoadItem(item); }
            foreach (var items in pathsArray) { LoadArrayItem(items); }
        }
    }

    [Serializable]
    public class QuickItems {

        public int nexusID;
        public int uniqueID;
        public string fileName;
        public int itemValue;
        public string itemName;
        public string itemType;

        public static QuickItems CreateFromJson(string jsonString) { return JsonUtility.FromJson<QuickItems>(jsonString); }

    }

    [Serializable]
    public class RootQuickItems {
        public List<QuickItems> items = new List<QuickItems>();
    }

    public static class JsonHelper {
        public static T[] FromJson<T>(string json) {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array) {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint) {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        public class Wrapper<T> {
            public T[] Items;
        }
    }

}

/*internal static void MainMenuInitialization() {
    foreach (var item in TRItems.customItems) {
        if (item.Key.Contains("QI.")) {
            //item.Value.invItem;
            //TRItems.customItemsByItemID[]
        }
    }
}
         
            
            /*
            foreach (var path in paths) { TRTools.Log(path); }
            
            string filePath = Path.Combine(Paths.PluginPath, "TR Tools", "quick_items");
            
            string json1 = File.ReadAllText(Path.Combine(filePath, "quickitems.qitem"));
            TRTools.Log(json1);

            var QIArray = JsonUtility.FromJson<RootQuickItems>(json1);
            
            TRTools.Log($"Starting Check...");
            TRTools.Log($"Multi...: QI Array: {QIArray}");
            TRTools.Log($"Multi...: QI Array: {QIArray.items}");
            TRTools.Log($"Multi...: QI Array: {QIArray.items.Count}");  
            */
