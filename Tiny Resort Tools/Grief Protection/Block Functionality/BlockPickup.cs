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

public class BlockPickup {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockPickup;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "InvokeUserCode_CmdPickUp", typeof(BlockPickup), "InvokeUserCode_CmdPickUpPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "UserCode_CmdPickUp", typeof(BlockPickup), "UserCode_CmdPickUpPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPickUpOnTileInside", typeof(BlockPickup), "InvokeUserCode_CmdPickUpOnTileInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPickUpOnTileInside", typeof(BlockPickup), "UserCode_CmdPickUpOnTileInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPickUpOnTile", typeof(BlockPickup), "InvokeUserCode_CmdPickUpOnTilePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPickUpOnTile", typeof(BlockPickup), "UserCode_CmdPickUpOnTilePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPickUpObjectOnTopOfInside", typeof(BlockPickup), "InvokeUserCode_CmdPickUpObjectOnTopOfInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPickUpObjectOnTopOfInside", typeof(BlockPickup), "UserCode_CmdPickUpObjectOnTopOfInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPickUpObjectOnTopOf", typeof(BlockPickup), "InvokeUserCode_CmdPickUpObjectOnTopOfPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPickUpObjectOnTopOf", typeof(BlockPickup), "UserCode_CmdPickUpObjectOnTopOfPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPickUpObjectOnTop", typeof(BlockPickup), "InvokeUserCode_CmdPickUpObjectOnTopPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPickUpObjectOnTop", typeof(BlockPickup), "UserCode_CmdPickUpObjectOnTopPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "InvokeUserCode_CmdPickUpObject", typeof(BlockPickup), "InvokeUserCode_CmdPickUpObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "UserCode_CmdPickUpObject", typeof(BlockPickup), "UserCode_CmdPickUpObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPickUpItemOnTopOfInside", typeof(BlockPickup), "InvokeUserCode_CmdPickUpItemOnTopOfInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPickUpItemOnTopOfInside", typeof(BlockPickup), "UserCode_CmdPickUpItemOnTopOfInsidePrefix");
    }

    public static bool checkConditions() {
        if (!LockPickup.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpItemOnTopOfInsidePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpItemOnTopOfInsidePrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpObjectPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpObjectPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpObjectOnTopPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpObjectOnTopPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpObjectOnTopOfPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpObjectOnTopOfPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpObjectOnTopOfInsidePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpObjectOnTopOfInsidePrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpOnTilePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpOnTilePrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPickUpOnTileInsidePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPickUpOnTileInsidePrefix() => checkConditions();
}
