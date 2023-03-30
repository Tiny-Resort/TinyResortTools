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

public class BlockBuyFromStall {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockShopping;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdBuyItemFromStall", typeof(BlockBuyFromStall), "InvokeUserCode_CmdBuyItemFromStallPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdBuyItemFromStall", typeof(BlockBuyFromStall), "UserCode_CmdBuyItemFromStallPrefix");
    }

    public static bool checkConditions() {
        if (!LockShopping.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdBuyItemFromStallPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdBuyItemFromStallPrefix() => checkConditions();

}
