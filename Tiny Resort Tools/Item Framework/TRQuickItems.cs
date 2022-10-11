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
            var customClothes = Path.Combine(Paths.PluginPath, "custom_assets", "custom_clothing");
            var customPaths = Path.Combine(Paths.PluginPath, "custom_assets", "custom_paths");

            var customClothingBundle = TRAssets.LoadBundle(Path.Combine(customClothes, "clothing_bundle"));
            var customPathBundle = TRAssets.LoadBundle(Path.Combine(customPaths, "path_bundle"));
            SortThroughFolder(Path.Combine(customClothes, "shirts"), customClothingBundle.LoadAsset<GameObject>("shirt"));
            SortThroughFolder(Path.Combine(customClothes, "boots"), customClothingBundle.LoadAsset<GameObject>("boots"));
            SortThroughFolder(Path.Combine(customClothes, "dresses_and_coats"), customClothingBundle.LoadAsset<GameObject>("doublesideddress"));
            SortThroughFolder(Path.Combine(customClothes, "hats_baseball"), customClothingBundle.LoadAsset<GameObject>("baseballcap"));
            SortThroughFolder(Path.Combine(customClothes, "hats_beanie"), customClothingBundle.LoadAsset<GameObject>("beanie"));
            SortThroughFolder(Path.Combine(customClothes, "hats_bow"), customClothingBundle.LoadAsset<GameObject>("bowhat"));
            SortThroughFolder(Path.Combine(customClothes, "hats_bucket"), customClothingBundle.LoadAsset<GameObject>("buckethat"));
            SortThroughFolder(Path.Combine(customClothes, "hats_flatcap"), customClothingBundle.LoadAsset<GameObject>("flatcap"));
            SortThroughFolder(Path.Combine(customClothes, "pants"), customClothingBundle.LoadAsset<GameObject>("pants"));
            SortThroughFolder(Path.Combine(customClothes, "shoes_flippers"), customClothingBundle.LoadAsset<GameObject>("flippers"));
            SortThroughFolder(Path.Combine(customClothes, "shoes_sneakers"), customClothingBundle.LoadAsset<GameObject>("sneakers"));
            SortThroughFolder(Path.Combine(customClothes, "shoes_standard"), customClothingBundle.LoadAsset<GameObject>("shoes"));
            SortThroughFolder(Path.Combine(customClothes, "shorts"), customClothingBundle.LoadAsset<GameObject>("shorts"));
            SortThroughFolder(Path.Combine(customPaths, "path"), customClothingBundle.LoadAsset<GameObject>("path"));

        }
        
        // Goes through all textures in a specific folder and tries to load them as custom items
        internal static void SortThroughFolder(string path, GameObject obj) {
            
            var files = TRAssets.ListAllTextures(path);
            
            foreach (var file in files) {
                
                // If no texture could be loaded, skip this one
                var texture = TRAssets.LoadTexture(file);
                if (!texture) continue;
                
                // Creates a new instance of the item
                var fileName = Path.GetFileName(file).Split('.')[0].Split('_');
                var newItem = new TRCustomItem();
                newItem.invItem = Object.Instantiate(obj).GetComponent<InventoryItem>();
                newItem.invItem.itemName = fileName[0];
                
                // Sets the texture for the item (TODO: Would need to be set up to work with non-clothing quick items)
                // This should automatically work for wallpaper and such, but not furniture. 
                // *******THIS SHOULD WORK FOR WALLPAPER/FLOORING AUTOMATICALLY*************
                newItem.invItem.equipable.material = new Material(newItem.invItem.equipable.material);
                newItem.invItem.equipable.material.mainTexture = texture;
                
                // Sets the value of the custom item
                var value = fileName.Length >= 2 && int.TryParse(fileName[1], out var val) ? val : 2500; 
                newItem.invItem.value = value;
                
                TRItems.AddCustomItem(newItem, "QuickItem." + fileName[0]);

            }
            
        }

    }

}
