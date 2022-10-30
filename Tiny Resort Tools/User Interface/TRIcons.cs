using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BepInEx;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TinyResort {

    internal class TRIcons : MonoBehaviour {

        internal static bool notParsed = true;
        internal static List<string> itemList = new List<string>();
        internal static List<CustomSprites> proccessedItemList = new List<CustomSprites>();
        internal static List<CustomSprites> proccessedQIItemList = new List<CustomSprites>();

        internal static List<string> FolderList = new List<string>();
        internal static List<string> defaultSprites = new List<string>();

        internal static string relativePath = Path.Combine("TR Tools", "item_icons");

        internal class CustomSprites {
            internal string itemName;
            internal Sprite customSprite;
            internal string spritePath;
            internal InventoryItem item;
        }

        #region Required Initializations

        internal static void Initialize() {
            TRTools.QuickPatch(typeof(InventoryItem), "getSprite", typeof(TRIcons), "getSpritePrefix");
            TRTools.QuickPatch(typeof(ItemSign), "updateStatus", typeof(TRIcons), null, "updateStatusPostfix");
            LeadPlugin.plugin.AddCommand("reload_icons", "This will reload all icons in the custom icon's folder.", UpdateAllSprites);
        }

        internal static void InitializeIcons() {
            if (notParsed) {
                string[] defaultArg = new[] { "all" };
                InitializeDefaults();
                UpdateAllSprites(defaultArg);
                DebugIcons();
                notParsed = false;
            }
        }

        internal static void InitializeDefaults() {
            // Default Sprites
            defaultSprites.Add("ui2_10"); // Shirt
            defaultSprites.Add("ui2_12"); // Pants
            defaultSprites.Add("ui2_13"); // Shoes
            defaultSprites.Add("allUI_0"); // Box
            defaultSprites.Add("allUI_3"); // Dress
            defaultSprites.Add("allUI_150"); // Rugs
            defaultSprites.Add("allUI_149"); // Wallpaper
            defaultSprites.Add("fruitAndPlantsUI_4"); // Hat

            // Fix names of items that didn't match
            Inventory.inv.allItems[56].itemName = "Wooden Seat"; // It was named Wooden Chair for some reason
            Inventory.inv.allItems[144].itemName = "Long White Coffee Table"; // Coffee was spelled wrong
            Inventory.inv.allItems[977].itemName = "Dark P.I Hat"; // P.I Hat already exists
        }

        #endregion

        internal static bool IsInFolder(string itemName) {
            if (File.Exists(Path.Combine(Paths.PluginPath, relativePath, itemName.Replace(" ", "_") + ".png"))) { return true; }
            return false;
        }

        internal static string UpdateAllSprites(string[] args) {
            itemList.Clear();
            proccessedItemList.Clear();

            var items = Inventory.inv.allItems;
            for (int i = 0; i < Inventory.inv.allItems.Length; i++) {
                var itemName = items[i].itemName.ToLower().Replace(" ", "_");
                itemList.Add(itemName);
                if (IsInFolder(itemName)) {
                    CustomSprites addNewSprite = new CustomSprites();
                    Sprite sprite = TRAssets.LoadSprite(Path.Combine(relativePath, itemName + ".png"), Vector2.one * 0.5f);
                    if (sprite != null) {
                        items[i].itemSprite = sprite;
                        addNewSprite.itemName = items[i].itemName.ToLower().Replace(" ", "_");
                        addNewSprite.customSprite = sprite;
                        proccessedItemList.Add(addNewSprite);
                    }
                }
            }
            return "Refreshed All Sprites";
        }

        #region Patches

        internal static bool getSpritePrefix(InventoryItem __instance, ref Sprite __result) {

            var itemName = __instance.itemName.ToLower().Replace(" ", "_");
            var FindSprite = proccessedItemList.Find(i => i.itemName == itemName);
            var FindQISprite = proccessedQIItemList.Find(i => i.itemName == itemName);

            if (FindSprite != null) {
                //TRTools.LogError($"\nItem Name: {itemName}");
                __result = FindSprite.customSprite;
                return false;
            }
            if (FindQISprite != null) {
                //TRTools.LogError($"\nItem Name: {itemName}");
                __result = FindQISprite.customSprite;
                return false;
            }

            //Plugin.Log($"getSprite Prefab Name = {__instance.itemName}");
            if (IsInFolder(itemName)) {
                //Plugin.Log($"inside getSprite Prefab Name = {__instance.itemName}");
                Sprite sprite = TRAssets.LoadSprite(Path.Combine(relativePath, itemName + ".png"), Vector2.one * 0.5f);
                if (sprite != null) { __instance.itemSprite = sprite; }
                __result = sprite;
                return false;
            }
            return true;
        }

        internal static void updateStatusPostfix(ItemSign __instance, int newStatus) {
            if (newStatus != -1) {
                var FindSprite = proccessedItemList.Find(i => i.itemName == Inventory.inv.allItems[newStatus].itemName.ToLower());

                if (FindSprite != null) { __instance.itemRenderer.transform.localScale = new Vector3(.4858f, .4858f, .4858f); }
            }
        }

        #endregion

        #region Debugging Methods

        // Shows which files are incorrectly named and which files are missing a sprite
        internal static void DebugIcons() {

            var files = TRAssets.ListAllTextures(relativePath);
            for (int i = 0; i < files.Count; i++) { FolderList.Add(Path.GetFileName(files[i]).Replace(".png", "").Replace(" ", "_")); }

            var oddItems = new List<string>();
            for (int j = 0; j < FolderList.Count; j++) {
                if (!itemList.Contains(FolderList[j])) { oddItems.Add($"{FolderList[j]}"); }
            }

            var invalidItems = new List<string>();
            for (int k = 0; k < Inventory.inv.allItems.Length; k++) {
                var spriteName = Inventory.inv.allItems[k].itemSprite.name;
                if ((string.IsNullOrWhiteSpace(spriteName) || defaultSprites.Contains(spriteName)) && !FolderList.Contains(Inventory.inv.allItems[k].itemName.ToLower().Replace(" ", "_"))) { invalidItems.Add($"{k} {Inventory.inv.allItems[k].itemName}"); }
            }

            if (oddItems.Count > 0) { TRTools.LogError($"Item Icon Issue - The following icons found in the item_icons folder have a name that does not match any items:\n" + string.Join("\n", oddItems)); }
            if (invalidItems.Count > 0) { TRTools.Log($"Item Icon Issue - Item has no unique sprite:\n" + string.Join("\n", invalidItems), false); }

        }

        #endregion

    }

}
