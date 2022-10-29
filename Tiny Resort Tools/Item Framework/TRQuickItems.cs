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
        
        internal static List<string> paths = new List<string>();
        internal static AssetBundle customClothingBundle = TRAssets.LoadAssetBundleFromDLL("clothing_bundle");

        internal static void FindAllPaths(string initialDir) {
            foreach (string file in Directory.GetFiles(initialDir)) { if (Path.GetExtension(file) == ".qitem") { paths.Add(file); } }
            foreach (string dir in Directory.GetDirectories(initialDir)) { FindAllPaths(dir); }
        }

        internal static void InitializeCustomItem(string path, GameObject obj, QuickItems item) {

            // If no texture could be loaded, skip this one
            // Path Combine doesn't seem to work here? Acts like PLuginPath is empty...
            var texture = TRAssets.LoadTexture(Paths.PluginPath + path.Trim());
            if (!texture) return;

            // Creates a new instance of the item
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
            
            else { TRTools.LogError($"Loading a Quick Item in with a -1 Nexus ID. THis is not allowed and will be blocked. If you are a developer, please turn on developer mode."); }
            
            TRTools.Log($"Custom ID: {newItem.customItemID}");
            newItem.inventoryItem.value = item.value;

        }

        internal static void LoadItem(string path) {
            string json = File.ReadAllText(path);
            var oneItem = QuickItems.CreateFromJson(json);

            string relativePath = path.Remove(0, Paths.PluginPath.Length).Replace(Path.GetFileName(path), oneItem.fileName);
            InitializeCustomItem(relativePath, customClothingBundle.LoadAsset<GameObject>(oneItem.type), oneItem);

            TRTools.Log($"Single Item: {oneItem.fileName} | {oneItem.itemName} | {oneItem.type} | {oneItem.value}");
        }

        internal static void LoadAllQuickItems() {
            FindAllPaths(Paths.PluginPath);
            foreach (var item in paths) { LoadItem(item); }
        }
    }

    [Serializable]
    public class QuickItems {

        public int nexusID;
        public int uniqueID;
        public string itemName;
        public string fileName;
        public int value;
        public string type;

        public static QuickItems CreateFromJson(string jsonString) { return JsonUtility.FromJson<QuickItems>(jsonString); }
    }
}
