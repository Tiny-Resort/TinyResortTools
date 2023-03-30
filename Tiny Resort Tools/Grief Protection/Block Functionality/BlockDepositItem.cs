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

public class BlockDepositItem {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockMachines;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdDepositItem", typeof(BlockDepositItem), "InvokeUserCode_CmdDepositItemPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdDepositItem", typeof(BlockDepositItem), "UserCode_CmdDepositItemPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdDepositItemInside", typeof(BlockDepositItem), "InvokeUserCode_CmdDepositItemInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdDepositItemInside", typeof(BlockDepositItem), "UserCode_CmdDepositItemInsidePrefix");
    }

    public static bool checkConditions() {
        if (!LockMachines.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdDepositItemPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdDepositItemPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdDepositItemInsidePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdDepositItemInsidePrefix() => checkConditions();
}
