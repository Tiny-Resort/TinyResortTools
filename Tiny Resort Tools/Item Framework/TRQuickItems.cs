using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
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
            var quickItems = Path.Combine(Paths.PluginPath, "custom_assets", "quick_items");
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
                newItem.invItem = Object.Instantiate(obj).GetComponent<InventoryItem>();
                GameObject.DontDestroyOnLoad(newItem.invItem);
                newItem.invItem.itemName = fileName;
                
                // Sets the texture for the item (TODO: Would need to be set up to work with non-clothing quick items)
                newItem.invItem.equipable.material = new Material(newItem.invItem.equipable.material);
                newItem.invItem.equipable.material.mainTexture = texture;

                newItem.isQuickItem = true;
                newItem.customItemID = "QI." + folderName + "_" + fileName.Replace(" ", "") + ext.Replace(".", "_");
                TRItems.customItems[newItem.customItemID] = newItem;
                newItem.invItem.value = 1000;
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

    }

}
