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

public class BlockCatch {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockBugsAndFish;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdCatchBug", typeof(BlockCatch), "InvokeUserCode_CmdCatchBugPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdCatchBug", typeof(BlockCatch), "UserCode_CmdCatchBugPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(NetworkFishingRod), "InvokeUserCode_CmdCastLine", typeof(BlockCatch), "InvokeUserCode_CmdCastLinePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(NetworkFishingRod), "UserCode_CmdCastLine", typeof(BlockCatch), "UserCode_CmdCastLinePrefix");

    }

    public static bool checkConditions() {
        if (!LockBugsAndFish.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdCatchBugPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdCatchBugPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdCastLinePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdCastLinePrefix() => checkConditions();
}
