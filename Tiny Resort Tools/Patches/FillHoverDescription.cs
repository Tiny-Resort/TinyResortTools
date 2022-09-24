using System;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(Inventory), "fillHoverDescription")]
    internal class FillHoverDescription {

        internal static string DebugMessage = "";
        internal static int itemID;

        [HarmonyPostfix]
        [HarmonyPriority(999)]
        [HarmonyAfter(new string[] { "spicy.museumtooltip", "Octr_ValueTooltip" })]
        internal static void postfix(ref Inventory __instance, InventorySlot rollOverSlot) {

            if (CraftingManager.manage.craftMenuOpen) return;
            
            itemID = rollOverSlot.itemNo;
            bool stackable = __instance.allItems[rollOverSlot.itemNo].isStackable;
            int maxStack = __instance.allItems[rollOverSlot.itemNo].maxStack;
            bool hasFuel = __instance.allItems[rollOverSlot.itemNo].hasFuel;
            int value = __instance.allItems[rollOverSlot.itemNo].value;
            bool isTool = __instance.allItems[rollOverSlot.itemNo].isATool;
            bool isPowerTool = __instance.allItems[rollOverSlot.itemNo].isPowerTool;
            float staminaCost = __instance.allItems[rollOverSlot.itemNo].getStaminaCost();
            string spriteName = __instance.allItems[rollOverSlot.itemNo].getSprite().name;
            
            string fuelMessage = "";
            if (hasFuel) {
                int fuel = rollOverSlot.stack;
                int maxFuel = __instance.allItems[rollOverSlot.itemNo].fuelMax;
                fuelMessage = $"Tool: {isTool} | Power Tool: {isPowerTool}\nFuel: {fuel} | Max Fuel: {maxFuel} | Cost per Swing: {staminaCost}\n";
            }

            string foodType = "";
            string buffMessage = "";

            if (__instance.allItems[rollOverSlot.itemNo].consumeable) {
                if (__instance.allItems[rollOverSlot.itemNo].consumeable.isFruit) { foodType = $"Fruit"; }
                else if (__instance.allItems[rollOverSlot.itemNo].consumeable.isMeat) { foodType = $"Meat"; }
                else if (__instance.allItems[rollOverSlot.itemNo].consumeable.isVegitable) { foodType = $"Vegetable"; }
                else if (__instance.allItems[rollOverSlot.itemNo].consumeable.isAnimalProduct) { foodType = $"Animal Product"; }
                else { foodType = $"No Type"; }
                
                for (int i = 0; i < __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs.Length; i++) {
                    var buffType = __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs[i].myType;
                    var buffTimeLimit = __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs[i].seconds;
                    var buffLevel = __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs[i].myLevel;
                    buffMessage = $"Buff Type: {buffType} | Duration: {buffTimeLimit / 60} mins | Level: {buffLevel}\n";
                }
            }

           

            string clothesType = "";
            string ShirtMesh = "";
            string ClothingMaterial = "";
            if (__instance.allItems[rollOverSlot.itemNo].equipable) {
                if (__instance.allItems[rollOverSlot.itemNo].equipable.cloths) {
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.dress) clothesType = "Dress";
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.face) clothesType = "Face";
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.hat) clothesType = "Hat";
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.pants) clothesType = "Pants";
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.shirt) clothesType = "Shirt";
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.shoes) clothesType = "Shoes";

                    if (__instance.allItems[rollOverSlot.itemNo].equipable.shirtMesh) { ShirtMesh = __instance.allItems[rollOverSlot.itemNo].equipable.shirtMesh.name; }
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.material) { ClothingMaterial = __instance.allItems[rollOverSlot.itemNo].equipable.material.name; }
                }
            }

            string FurnitureType = "";
            if (__instance.allItems[rollOverSlot.itemNo].isFurniture) { FurnitureType = "Furniture"; }
            if (__instance.allItems[rollOverSlot.itemNo].equipable) {
                if (__instance.allItems[rollOverSlot.itemNo].equipable.wallpaper) FurnitureType = "Wallpaper";
                if (__instance.allItems[rollOverSlot.itemNo].equipable.flooring) FurnitureType = "Flooring";
            }
            
            string DefaultItemMessages = $"\n\n[ITEM] - ID: {itemID} - Sprite: {spriteName}\n";
            DefaultItemMessages += $"Base Value: {value} | Stackable: {stackable} | Max Stack: {maxStack}\n";
            
            
            string ToolMessages = "";
            if (!string.IsNullOrEmpty(fuelMessage)) {
                ToolMessages = $"[TOOLS]\n";
                ToolMessages += fuelMessage;
            }

       
            string ClothingMessages = "";
            if (!string.IsNullOrEmpty(clothesType)) {
                ClothingMessages = $"[CLOTHES] - Type: {clothesType}\n";
                if (!string.IsNullOrEmpty(clothesType) && !string.IsNullOrEmpty(ShirtMesh)) ClothingMessages += $"Mesh: {ShirtMesh} | Material: {ClothingMaterial}\n";
            }

            string FoodMessages = "";
            if (!string.IsNullOrEmpty(foodType)) {
                FoodMessages = $"[FOOD] - Type: {foodType}\n";
                FoodMessages += buffMessage;
            }

            string FurnitureMessages = "";
            if (!string.IsNullOrEmpty(FurnitureType)) {
                FurnitureMessages = $"[FURNITURE] - Type: {FurnitureType}\n";
                //FurnitureMessages += $"";
            }
            
            // TODO: Calculate Value after license
            // TODO: Calculate Stamina use based on level
            
            DebugMessage = DefaultItemMessages;
            if (!string.IsNullOrEmpty(ToolMessages)) DebugMessage += ToolMessages;
            if (!string.IsNullOrEmpty(FoodMessages)) DebugMessage += FoodMessages;
            if (!string.IsNullOrEmpty(ClothingMessages)) DebugMessage += ClothingMessages;
            if (!string.IsNullOrEmpty(FurnitureMessages)) DebugMessage += FurnitureMessages;

            if (Input.GetKey(KeyCode.LeftAlt)) __instance.InvDescriptionText.text += DebugMessage;
        }

    }

}
