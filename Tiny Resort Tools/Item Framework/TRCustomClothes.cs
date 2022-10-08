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

namespace TinyResort {

    /// <summary>Tools for quickly creating clothing items.</summary>
    public class TRCustomClothes {
        
        internal static string customAssetsPath = Path.Combine(BepInEx.Paths.PluginPath, "custom_assets\\custom_clothing");
        internal static string boots = Path.Combine(customAssetsPath, "boots");
        internal static string dresses_and_coats = Path.Combine(customAssetsPath, "dresses_and_coats");
        internal static string hats_baseball = Path.Combine(customAssetsPath, "hats_baseball");
        internal static string hats_beanie = Path.Combine(customAssetsPath, "hats_beanie");
        internal static string hats_bow = Path.Combine(customAssetsPath, "hats_bow");
        internal static string hats_bucket = Path.Combine(customAssetsPath, "hats_bucket");
        internal static string hats_flatcap = Path.Combine(customAssetsPath, "hats_flatcap");
        internal static string pants = Path.Combine(customAssetsPath, "pants");
        internal static string shirts = Path.Combine(customAssetsPath, "shirts");
        internal static string shoes_flippers = Path.Combine(customAssetsPath, "shoes_flippers");
        internal static string shoes_sneakers = Path.Combine(customAssetsPath, "shoes_sneakers");
        internal static string shoes_standard = Path.Combine(customAssetsPath, "shoes_standard");
        internal static string shorts = Path.Combine(customAssetsPath, "shorts");

        internal static void Initialize() {
        
            var customClothingBundle = TRAssets.LoadBundle(Path.Combine(customAssetsPath, "clothing_bundle"));
            
            SortThroughFolder(shirts, customClothingBundle.LoadAsset<GameObject>("shirt"));
            SortThroughFolder(boots, customClothingBundle.LoadAsset<GameObject>("boots"));
            SortThroughFolder(dresses_and_coats, customClothingBundle.LoadAsset<GameObject>("doublesideddress"));
            SortThroughFolder(hats_baseball, customClothingBundle.LoadAsset<GameObject>("baseballcap"));
            SortThroughFolder(hats_beanie, customClothingBundle.LoadAsset<GameObject>("beanie"));
            SortThroughFolder(hats_bow, customClothingBundle.LoadAsset<GameObject>("bowhat"));
            SortThroughFolder(hats_bucket, customClothingBundle.LoadAsset<GameObject>("buckethat"));
            SortThroughFolder(hats_flatcap, customClothingBundle.LoadAsset<GameObject>("flatcap"));
            SortThroughFolder(pants, customClothingBundle.LoadAsset<GameObject>("pants"));
            SortThroughFolder(shoes_flippers, customClothingBundle.LoadAsset<GameObject>("flippers"));
            SortThroughFolder(shoes_sneakers, customClothingBundle.LoadAsset<GameObject>("sneakers"));
            SortThroughFolder(shoes_standard, customClothingBundle.LoadAsset<GameObject>("shoes"));
            SortThroughFolder(shorts, customClothingBundle.LoadAsset<GameObject>("shorts"));
            
        }
        
        internal static void SortThroughFolder(string path, GameObject obj) {
            var files = Directory.GetFiles(path).ToList();
            foreach (var file in files) {
                var texture = TRAssets.LoadTexture(file);
                if (!texture) continue;
                var fileName = Path.GetFileName(file).Split('.')[0].Split('_');
                var value = fileName.Length >= 2 && int.TryParse(fileName[1], out var val) ? val : 2500; 
                TRItems.AddCustomClothing(obj, texture, fileName[0], value);
            }
        }

    }

}
