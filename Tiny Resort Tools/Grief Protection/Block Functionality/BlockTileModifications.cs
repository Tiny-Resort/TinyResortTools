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

public class BlockTileModifications {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockTileModifications;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdChangeTileHeight", typeof(BlockTileModifications), "InvokeUserCode_CmdChangeTileHeightPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdChangeTileHeight", typeof(BlockTileModifications), "UserCode_CmdChangeTileHeightPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdChangeTileType", typeof(BlockTileModifications), "InvokeUserCode_CmdChangeTileTypePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdChangeTileType", typeof(BlockTileModifications), "UserCode_CmdChangeTileTypePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdChangeOnTile", typeof(BlockTileModifications), "InvokeUserCode_CmdChangeOnTilePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdChangeOnTile", typeof(BlockTileModifications), "UserCode_CmdChangeOnTilePrefix");
    }

    public static bool checkConditions() {
        if (!LockTileModifications.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdChangeTileHeightPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdChangeTileHeightPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdChangeTileTypePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdChangeTileTypePrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdChangeOnTilePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdChangeOnTilePrefix() => checkConditions();
}
