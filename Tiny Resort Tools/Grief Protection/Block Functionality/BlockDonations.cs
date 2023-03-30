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

public class BlockDonations {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockMuseumDonations;
    public static ConfigEntry<bool> LockTownDebtDonations;
    public static ConfigEntry<bool> LockDeedIngredients;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdDonateDeedIngredients", typeof(BlockDonations), "InvokeUserCode_CmdDonateDeedIngredientsPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdDonateDeedIngredients", typeof(BlockDonations), "UserCode_CmdDonateDeedIngredientsPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdDonateItemToMuseum", typeof(BlockDonations), "InvokeUserCode_CmdDonateItemToMuseumPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdDonateItemToMuseum", typeof(BlockDonations), "UserCode_CmdDonateItemToMuseumPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdPayTownDebt", typeof(BlockDonations), "InvokeUserCode_CmdPayTownDebtPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdPayTownDebt", typeof(BlockDonations), "UserCode_CmdPayTownDebtPrefix");
    }

    public static bool checkConditions() {
        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdDonateDeedIngredientsPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdDonateDeedIngredientsPrefix() {
        if (!LockMuseumDonations.Value) return true;
        return checkConditions();
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdDonateItemToMuseumPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdDonateItemToMuseumPrefix() {
        if (!LockTownDebtDonations.Value) return true;
        return checkConditions();
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdPayTownDebtPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPayTownDebtPrefix() {
        if (!LockDeedIngredients.Value) return true;
        return checkConditions();
    }

}
