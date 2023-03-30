using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TinyResort;

public class GriefProtection {

    public static int playerToClickID;
    public static bool createdBanButtons;
    public static GameObject ChunkIndicator;
    public static Vector3 currentPosition;
    public static TRModData modData;

    // public static TRConflictingPlugins conflicting;

    public static string configPath = Path.Combine(Application.dataPath.Replace("Dinkum_Data", "BepInEx/config/"), LeadPlugin.pluginGuid);
    public static string configFilePath = Path.Combine(configPath, "banlist.txt");

    public static bool isBanListLoaded = false;
    public static bool isCustomizedChestLoaded = false;

    internal static void IntializeGriefProtection() {
        modData = TRData.Subscribe("TR.GriefProtection");

        #region Configuration

        LockChests.LockAllChests = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "AutoLockAllChests", false, "Set to true if you would like all chests to automatically be locked.");
        BlockVehicles.LockVehicles = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockVehicles", false, "Set to false to unlock Vehicles.");
        BlockUsingItems.LockSpawnPlaceables = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockPlaceables", false, "Set to false to unlock putting down placeable items.");
        BlockTileModifications.LockTileModifications = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockTileModifications", false, "Set to false to unlock tile modifications.");
        BlockPlaceItem.LockPlaceItems = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockPlaceItems", false, "Set to false to unlock placing items.");
        BlockPickup.LockPickup = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockPickups", false, "Set to false to unlock picking up items.");
        BlockHarvest.LockHarvest = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockHarvest", false, "Set to false to unlock harvesting plants, bushes, etc.");
        BlockDropItem.LockDroppingItems = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockDroppingItems", false, "Set to false to unlock dropping items.");
        BlockDonations.LockMuseumDonations = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockMuseumDonations", false, "Set to false to unlock museum donations.");
        BlockDonations.LockTownDebtDonations = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockTownDebtDonations", false, "Set to false to unlock town debt donations.");
        BlockDonations.LockDeedIngredients = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockDeedIngredientsDonations", false, "Set to false to unlock donating deed ingredients.");
        BlockDepositItem.LockMachines = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockMachines", false, "Set to false to unlock depositing items into machines.");
        BlockDamage.LockDamage = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockDamage", false, "Set to false to unlock damaging creatures.");
        BlockBuyFromStall.LockShopping = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockShoppingFromStalls", false, "Set to false to unlock buying items from stalls.");
        BlockCatch.LockBugsAndFish = LeadPlugin.instance.Config.Bind<bool>("Grief Protection", "LockCatchingBugsAndFish", false, "Set to false to unlock catching bugs and fish.");

        #endregion

        #region Patching

        LeadPlugin.plugin.QuickPatch(typeof(RealWorldTimeLight), "Update", typeof(GriefProtection), "updateRWTLPrefix");

        ClientManagement.init();
        BlockDropItem.init();
        BlockHarvest.init();
        BlockPickup.init();
        LockChests.init();
        BlockDamage.init();
        BlockTileModifications.init();
        BlockUsingItems.init();
        BlockVehicles.init();
        BlockPlaceItem.init();
        BlockDepositItem.init();
        BlockDonations.init();
        BlockCatch.init();
        BlockBuyFromStall.init();

        #endregion

        var host = new ClientManagement.CurrentPlayers();
        host.connectionID = 0;
        ClientManagement.currentWhitelist.Add(host);

        LeadPlugin.plugin.AddCommand("whitelist", "Whitelist a player by running /gp whitelist playerName", ClientManagement.addWhitelistPlayerChatCommand);

    }

    public void Awake() { }

    public static void SaveBanList() {
        modData.SetValue("banList", ClientManagement.bannedPlayers);
        modData.Save();
    }

    public static void LoadBanList() => ClientManagement.bannedPlayers = (List<ClientManagement.CurrentPlayers>)modData.GetValue("banList", new List<ClientManagement.CurrentPlayers>());

    internal static void GPLoadPresets() {
        if (!isBanListLoaded) {
            LoadBanList();
            isBanListLoaded = true;
        }
        if (!isCustomizedChestLoaded) {
            LockChests.LoadCustomizedChests();
            isCustomizedChestLoaded = true;
        }
    }

    internal static void updateRWTLPrefix(RealWorldTimeLight __instance) {

        if (Input.GetKeyDown(KeyCode.F11)) {
            ClientManagement.resetBanList();
            SaveBanList();
        }
        if (Input.GetKeyDown(KeyCode.F10)) ClientManagement.listBannedPlayers();

        if (Input.GetKeyDown(KeyCode.End) && ChestWindow.chests.chestWindowOpen) {
            var currentlyOpenedChest = Traverse.Create(ChestWindow.chests).Field("currentlyOpenedChest").GetValue() as Chest;

            var BlacklistChestExists = LockChests.blacklistedChests.Find(i => i.chestX == currentlyOpenedChest.xPos && i.chestY == currentlyOpenedChest.yPos);
            var WhitelistChestExists = LockChests.whitelistedChests.Find(i => i.chestX == currentlyOpenedChest.xPos && i.chestY == currentlyOpenedChest.yPos);

            var checkChest = LockChests.LockAllChests.Value ? WhitelistChestExists : BlacklistChestExists;
            var message = "";
            if (checkChest != null) {
                if (LockChests.LockAllChests.Value) {
                    message = $"Removed Whitelisted Chest at ({checkChest.chestX}, {checkChest.chestY}).";
                    LockChests.whitelistedChests.Remove(checkChest);
                }
                else {
                    message = $"Removed Blacklisted Chest at ({checkChest.chestX}, {checkChest.chestY}).";
                    LockChests.blacklistedChests.Remove(checkChest);
                }
            }
            else {
                var tempChest = new LockChests.LockedChests();
                tempChest.chestX = currentlyOpenedChest.xPos;
                tempChest.chestY = currentlyOpenedChest.yPos;
                if (LockChests.LockAllChests.Value) {
                    message = $"Added Whitelisted Chest at ({tempChest.chestX}, {tempChest.chestY}).";
                    LockChests.whitelistedChests.Add(tempChest);
                }
                else {
                    message = $"Added Blacklisted Chest at ({tempChest.chestX}, {tempChest.chestY}).";
                    LockChests.blacklistedChests.Add(tempChest);
                }

            }
            TRTools.TopNotification("Grief Protection", message);

            LockChests.SaveCustomizedChests();
        }
    }
}
