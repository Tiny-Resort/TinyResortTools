using HarmonyLib;
using UnityEngine;

namespace TinyResort
{
    [HarmonyPatch(typeof(Inventory), "fillHoverDescription")]
    internal class FillHoverDescription {

        internal static string DebugMessage = "";
        internal static int itemID;

        [HarmonyPostfix]
        [HarmonyPriority(999)]
        [HarmonyAfter("spicy.museumtooltip", "Octr_ValueTooltip")]
        internal static void postfix(ref Inventory __instance, InventorySlot rollOverSlot) {

            if (rollOverSlot == null) return;
            if (rollOverSlot.itemNo < 0) return;
            try {
                // This broke when hovering over an item in crafting menu and/or mail window
                if (CraftingManager.manage.craftMenuOpen
                    || MailManager.manage.mailWindow.activeSelf
                    || !__instance.allItems[rollOverSlot.itemNo]
                    || DeedManager.manage.deedWindowOpen)
                    return;

                if (!Inventory.Instance.invOpen) return;

                itemID = rollOverSlot.itemNo;
                var stackable = __instance.allItems[rollOverSlot.itemNo].isStackable;
                var maxStack = __instance.allItems[rollOverSlot.itemNo].maxStack;
                var hasFuel = __instance.allItems[rollOverSlot.itemNo].hasFuel;
                var value = __instance.allItems[rollOverSlot.itemNo].value;
                var isTool = __instance.allItems[rollOverSlot.itemNo].isATool;
                var isPowerTool = __instance.allItems[rollOverSlot.itemNo].isPowerTool;
                var staminaCost = __instance.allItems[rollOverSlot.itemNo].getStaminaCost();
                var spriteName = __instance.allItems[rollOverSlot.itemNo].getSprite().name;

                var fuelMessage = "";
                if (hasFuel) {
                    var fuel = rollOverSlot.stack;
                    var maxFuel = __instance.allItems[rollOverSlot.itemNo].fuelMax;
                    fuelMessage =
                        $"Tool: {isTool} | Power Tool: {isPowerTool}\nFuel: {fuel} | Max Fuel: {maxFuel} | Cost per Swing: {staminaCost}\n";
                }

                var foodType = "";
                var buffMessage = "";

                if (__instance.allItems[rollOverSlot.itemNo].consumeable) {
                    if (__instance.allItems[rollOverSlot.itemNo].consumeable.isFruit)
                        foodType = "Fruit";
                    else if (__instance.allItems[rollOverSlot.itemNo].consumeable.isMeat)
                        foodType = "Meat";
                    else if (__instance.allItems[rollOverSlot.itemNo].consumeable.isVegitable)
                        foodType = "Vegetable";
                    else if (__instance.allItems[rollOverSlot.itemNo].consumeable.isAnimalProduct)
                        foodType = "Animal Product";
                    else
                        foodType = "No Type";

                    if (__instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs != null)
                        for (var i = 0; i < __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs.Length; i++) {
                            var buffType = __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs[i].myType;
                            var buffTimeLimit = __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs[i].seconds;
                            var buffLevel = __instance.allItems[rollOverSlot.itemNo].consumeable.myBuffs[i].myLevel;
                            buffMessage =
                                $"Buff Type: {buffType} | Duration: {buffTimeLimit / 60} mins | Level: {buffLevel}\n";
                        }
                }

                var clothesType = "";
                var ShirtMesh = "";
                var ClothingMaterial = "";
                if (__instance.allItems[rollOverSlot.itemNo].equipable)
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.cloths) {
                        if (__instance.allItems[rollOverSlot.itemNo].equipable.dress) clothesType = "Dress";
                        if (__instance.allItems[rollOverSlot.itemNo].equipable.face) clothesType = "Face";
                        if (__instance.allItems[rollOverSlot.itemNo].equipable.hat) clothesType = "Hat";
                        if (__instance.allItems[rollOverSlot.itemNo].equipable.pants) clothesType = "Pants";
                        if (__instance.allItems[rollOverSlot.itemNo].equipable.shirt) clothesType = "Shirt";
                        if (__instance.allItems[rollOverSlot.itemNo].equipable.shoes) clothesType = "Shoes";

                        if (__instance.allItems[rollOverSlot.itemNo].equipable.shirtMesh)
                            ShirtMesh = __instance.allItems[rollOverSlot.itemNo].equipable.shirtMesh.name;
                        if (__instance.allItems[rollOverSlot.itemNo].equipable.material)
                            ClothingMaterial = __instance.allItems[rollOverSlot.itemNo].equipable.material.name;
                    }

                var FurnitureType = "";
                if (__instance.allItems[rollOverSlot.itemNo].isFurniture) FurnitureType = "Furniture";
                if (__instance.allItems[rollOverSlot.itemNo].equipable) {
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.wallpaper) FurnitureType = "Wallpaper";
                    if (__instance.allItems[rollOverSlot.itemNo].equipable.flooring) FurnitureType = "Flooring";
                }

                var DefaultItemMessages = $"\n\n[ITEM] - ID: {itemID} - Sprite: {spriteName}\n";
                DefaultItemMessages += $"Base Value: {value} | Stackable: {stackable} | Max Stack: {maxStack}\n";

                var ToolMessages = "";
                if (!string.IsNullOrEmpty(fuelMessage)) {
                    ToolMessages = "[TOOLS]\n";
                    ToolMessages += fuelMessage;
                }

                var ClothingMessages = "";
                if (!string.IsNullOrEmpty(clothesType)) {
                    ClothingMessages = $"[CLOTHES] - Type: {clothesType}\n";
                    if (!string.IsNullOrEmpty(clothesType) && !string.IsNullOrEmpty(ShirtMesh))
                        ClothingMessages += $"Mesh: {ShirtMesh} | Material: {ClothingMaterial}\n";
                }

                var FoodMessages = "";
                if (!string.IsNullOrEmpty(foodType)) {
                    FoodMessages = $"[FOOD] - Type: {foodType}\n";
                    FoodMessages += buffMessage;
                }

                var FurnitureMessages = "";
                if (!string.IsNullOrEmpty(FurnitureType)) FurnitureMessages = $"[FURNITURE] - Type: {FurnitureType}\n";

                //FurnitureMessages += $"";
                // TODO: Calculate Value after license
                // TODO: Calculate Stamina use based on level

                DebugMessage = DefaultItemMessages;
                if (!string.IsNullOrEmpty(ToolMessages)) DebugMessage += ToolMessages;
                if (!string.IsNullOrEmpty(FoodMessages)) DebugMessage += FoodMessages;
                if (!string.IsNullOrEmpty(ClothingMessages)) DebugMessage += ClothingMessages;
                if (!string.IsNullOrEmpty(FurnitureMessages)) DebugMessage += FurnitureMessages;

                //TODO: Set an option to have this always turned on(?)
                if (Input.GetKey(KeyCode.LeftAlt) && __instance
                                                  && !__instance.InvDescriptionText.text.Contains(DebugMessage))
                    __instance.InvDescriptionText.text += DebugMessage;

            }
            catch { TRTools.LogError("Failed To Parse Data for Hover Description. WIP."); }

        }
    }
}
