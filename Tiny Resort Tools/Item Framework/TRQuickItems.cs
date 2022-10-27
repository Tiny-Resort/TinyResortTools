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
        internal static void Initialize() {

            // Load Asset Bundle and Sort through all folders
            TRTools.Log($"Before Clothing Bundle");
            var customClothingBundle = TRAssets.LoadAssetBundleFromDLL("clothing_bundle");
            var quickItems = Path.Combine(Paths.PluginPath, "TR Tools", "quick_items");

            //var customPathBundle = TRAssets.LoadBundle(Path.Combine(customPaths, "path_bundle"));

            // TODO: Create wallpaper and flooring bundles to support those
            SortThroughFolder(Path.Combine(quickItems, "shirts"), customClothingBundle.LoadAsset<GameObject>("shirt"));
            SortThroughFolder(Path.Combine(quickItems, "boots"), customClothingBundle.LoadAsset<GameObject>("boots"));
            SortThroughFolder(Path.Combine(quickItems, "dresses_and_coats"), customClothingBundle.LoadAsset<GameObject>("doublesideddress"));
            SortThroughFolder(Path.Combine(quickItems, "hats_baseball"), customClothingBundle.LoadAsset<GameObject>("baseballcap"));
            SortThroughFolder(Path.Combine(quickItems, "hats_beanie"), customClothingBundle.LoadAsset<GameObject>("beanie"));
            SortThroughFolder(Path.Combine(quickItems, "hats_bow"), customClothingBundle.LoadAsset<GameObject>("bowhat"));
            SortThroughFolder(Path.Combine(quickItems, "hats_bucket"), customClothingBundle.LoadAsset<GameObject>("buckethat"));
            SortThroughFolder(Path.Combine(quickItems, "hats_flatcap"), customClothingBundle.LoadAsset<GameObject>("flatcap"));
            SortThroughFolder(Path.Combine(quickItems, "pants"), customClothingBundle.LoadAsset<GameObject>("pants"));
            SortThroughFolder(Path.Combine(quickItems, "shoes_flippers"), customClothingBundle.LoadAsset<GameObject>("flippers"));
            SortThroughFolder(Path.Combine(quickItems, "shoes_sneakers"), customClothingBundle.LoadAsset<GameObject>("sneakers"));
            SortThroughFolder(Path.Combine(quickItems, "shoes_standard"), customClothingBundle.LoadAsset<GameObject>("shoes"));
            SortThroughFolder(Path.Combine(quickItems, "shorts"), customClothingBundle.LoadAsset<GameObject>("shorts"));

            //SortThroughFolder(Path.Combine(customPaths, "path"), customClothingBundle.LoadAsset<GameObject>("path"));

        }

        // Goes through all textures in a specific folder and tries to load them as custom items
        internal static void SortThroughFolder(string path, GameObject obj) {

            var files = TRAssets.ListAllTextures(path);

            foreach (var file in files) {

                // If no texture could be loaded, skip this one
                var texture = TRAssets.LoadTexture(file);
                if (!texture) continue;

                // Creates a new instance of the item
                var fileName = Path.GetFileNameWithoutExtension(file);
                var folderName = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(folderName)) { folderName = "unknown"; }
                else { folderName = folderName.Split('\\').Last().Replace(" ", ""); }
                var ext = Path.GetExtension(file);
                var newItem = new TRCustomItem();
                newItem.inventoryItem = Object.Instantiate(obj).GetComponent<InventoryItem>();
                GameObject.DontDestroyOnLoad(newItem.inventoryItem);
                newItem.inventoryItem.itemName = fileName;

                // Sets the texture for the item (TODO: Would need to be set up to work with non-clothing quick items)
                newItem.inventoryItem.equipable.material = new Material(newItem.inventoryItem.equipable.material);
                newItem.inventoryItem.equipable.material.mainTexture = texture;

                newItem.isQuickItem = true;
                newItem.customItemID = "QI." + folderName + "_" + fileName.Replace(" ", "") + ext.Replace(".", "_");
                TRItems.customItems[newItem.customItemID] = newItem;
                newItem.inventoryItem.value = 1000;
            }

        }

        internal static void MainMenuInitialization() {
            foreach (var item in TRItems.customItems) {
                if (item.Key.Contains("QI.")) {
                    //item.Value.invItem;
                    //TRItems.customItemsByItemID[]
                }
            }
        }

        internal static void TestJson() {

            string filePath = Path.Combine(Paths.PluginPath, "TR Tools", "quick_items");
            
            string json1 = File.ReadAllText(Path.Combine(filePath, "quickitems.json"));
            TRTools.Log(json1);
            string jsonEdit = "{\"Items\":" + json1 + "}";
            // QuickItems[] QIArray = JsonHelper.FromJson<QuickItems>(json);
            var QIArray = JsonUtility.FromJson<RootQuickItems>(json1);
            //var QIArray = JSONArray.LoadFromFile(Path.Combine(filePath, "quickitems.json"));
            
            TRTools.Log($"Starting Check...");
            TRTools.Log($"Multi...: QI Array: {QIArray}");
            TRTools.Log($"Multi...: QI Array: {QIArray.items}");
            TRTools.Log($"Multi...: QI Array: {QIArray.items.Count}"); 

            
            // This works for single items...
            string json = File.ReadAllText(Path.Combine(filePath, "quickitemsSingle.json")); 
            var oneItem = QuickItems.CreateFromJson(json);

            TRTools.Log($"Single Item: {oneItem.fileName}");
        }

    }

    [Serializable]
    public class QuickItems {

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
        private class Wrapper<T> {
            public T[] Items;
        }
    }

}
