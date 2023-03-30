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

public class BlockDropItem {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockDroppingItems;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdDropItem", typeof(BlockDropItem), "InvokeUserCode_CmdDropItemPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdDropItem", typeof(BlockDropItem), "UserCode_CmdDropItemPrefix");
    }

    public static bool checkConditions() {
        if (!LockDroppingItems.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdDropItemPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdDropItemPrefix() => checkConditions();

}
