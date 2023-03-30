using System;
using System.Collections;
using System.Collections.Generic;
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
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace TinyResort; 

public class LockChests {

    [Serializable]
    public class LockedChests {
        public int chestX;
        public int chestY;
    }

    public static List<LockedChests> blacklistedChests = new();
    public static List<LockedChests> whitelistedChests = new();

    public static int currentConnectionID;

    public static ConfigEntry<bool> LockAllChests;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "InvokeUserCode_CmdOpenChest", typeof(LockChests), "InvokeUserCode_CmdOpenChestPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "UserCode_CmdOpenChest", typeof(LockChests), "UserCode_CmdOpenChestPrefix");
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdOpenChestPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix]
    public static bool UserCode_CmdOpenChestPrefix(int xPos, int yPos) {
        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        var currentBlacklist = blacklistedChests.Find(i => i.chestX == xPos && i.chestY == yPos);
        var currentWhitelist = whitelistedChests.Find(i => i.chestX == xPos && i.chestY == yPos);

        if (currentConnectionID > 0 && LockAllChests.Value && currentWhitelist == null) return false;

        if (currentConnectionID > 0 && !LockAllChests.Value && currentBlacklist != null) return false;

        return true;
    }

    public static void LockAChest(int cChestX, int cChestY) {
        var tempChest = new LockedChests();
        tempChest.chestX = cChestX;
        tempChest.chestY = cChestY;
        blacklistedChests.Add(tempChest);
    }

    public static void removeLockedChest(int cChestX, int cChestY) {
        var current = blacklistedChests.Find(i => i.chestX == cChestX && i.chestY == cChestY);
        blacklistedChests.Remove(current);
    }

    public static void SaveCustomizedChests() {
        if (blacklistedChests != null) GriefProtection.modData.SetValue("blacklistedChests", blacklistedChests);
        if (whitelistedChests != null) GriefProtection.modData.SetValue("whitelistedChests", whitelistedChests);
        GriefProtection.modData.Save();
    }

    public static void LoadCustomizedChests() {
        blacklistedChests = (List<LockedChests>)GriefProtection.modData.GetValue("blacklistedChests", new List<LockedChests>());
        whitelistedChests = (List<LockedChests>)GriefProtection.modData.GetValue("whitelistedChests", new List<LockedChests>());
    }
}
