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
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace TinyResort; 

public class BlockUsingItems {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockSpawnPlaceables;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(EquipItemToChar), "InvokeUserCode_CmdUsingItem", typeof(BlockUsingItems), "InvokeUserCode_CmdUsingItemPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(EquipItemToChar), "UserCode_CmdUsingItem", typeof(BlockUsingItems), "UserCode_CmdUsingItemPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdSpawnPlaceable", typeof(BlockUsingItems), "InvokeUserCode_CmdSpawnPlaceablePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdSpawnPlaceable", typeof(BlockUsingItems), "UserCode_CmdSpawnPlaceablePrefix");
    }

    public static bool checkConditions() {
        if (!LockSpawnPlaceables.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    // Blocks damage done to creatures
    [HarmonyPrefix] public static void InvokeUserCode_CmdUsingItemPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdUsingItemPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdSpawnPlaceablePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdSpawnPlaceablePrefix() => checkConditions();

}
