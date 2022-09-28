/*
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "LoadInv")]
    internal class LoadInv {

        [HarmonyPrefix]
        internal static void Prefix(TMP_InputField __instance) {
	        FileStream fileStream = null;
	        PlayerInv playerInv;
	        BinaryFormatter binaryFormatter = new BinaryFormatter();
	        fileStream = File.Open(Path.Combine(new string[] { Application.persistentDataPath + "\\Slot1"}) + "/playerInfo.dat", FileMode.Open);
	        playerInv = (PlayerInv)binaryFormatter.Deserialize(fileStream);
	        fileStream.Close();
	        
	        Inventory.inv.changeWalletToLoad(playerInv.money);
			BankMenu.menu.accountBalance = playerInv.bankBalance;
			if (playerInv.hair < 0)
			{
				playerInv.hair = Mathf.Abs(playerInv.hair + 1);
			}
			Inventory.inv.playerHair = playerInv.hair;
			Inventory.inv.playerHairColour = playerInv.hairColour;
			Inventory.inv.playerEyes = playerInv.eyeStyle;
			Inventory.inv.nose = playerInv.nose;
			Inventory.inv.mouth = playerInv.mouth;
			Inventory.inv.playerEyeColor = playerInv.eyeColour;
			Inventory.inv.skinTone = playerInv.skinTone;
			Inventory.inv.playerName = playerInv.playerName;
			Inventory.inv.islandName = playerInv.islandName;
			EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(playerInv.head, 1);
			EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(playerInv.face, 1);
			EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(playerInv.body, 1);
			EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(playerInv.pants, 1);
			EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(playerInv.shoes, 1);
			StatusManager.manage.loadStatus(playerInv.health, playerInv.healthMax, playerInv.stamina, playerInv.staminaMax);
			TRTools.Log($"WROEKD UNTIL Catalogue SLOTS");

			if (playerInv.catalogue != null) {
				for (int i = 0; i < playerInv.catalogue.Length; i++) { CatalogueManager.manage.collectedItem[i] = playerInv.catalogue[i]; }
			}
			TRTools.Log($"WROEKD UNTIL INV SLOTS");
			
			for (int j = 0; j < playerInv.itemsInInvSlots.Length; j++)
			{
				Inventory.inv.invSlots[j].itemNo = playerInv.itemsInInvSlots[j];
				Inventory.inv.invSlots[j].stack = playerInv.stacksInSlots[j];
				Inventory.inv.invSlots[j].updateSlotContentsAndRefresh(playerInv.itemsInInvSlots[j], playerInv.stacksInSlots[j]);
			}
			TRTools.Log($"WROEKD after INV SLOTS");

			
		}
        
        
    }

}
*/
