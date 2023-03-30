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

public class BlockVehicles {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockVehicles;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdSpawnVehicle", typeof(BlockVehicles), "InvokeUserCode_CmdSpawnVehiclePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdSpawnVehicle", typeof(BlockVehicles), "UserCode_CmdSpawnVehiclePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "InvokeUserCode_CmdStartDriving", typeof(BlockVehicles), "InvokeUserCode_CmdStartDrivingPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "UserCode_CmdStartDriving", typeof(BlockVehicles), "UserCode_CmdStartDrivingPrefix");

    }

    public static bool checkConditions() {
        if (!LockVehicles.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdSpawnVehiclePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdSpawnVehiclePrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdStartDrivingPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdStartDrivingPrefix() => checkConditions();
}
