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

public class BlockHarvest {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockHarvest;

    public static void init() {
        // Patched for Blocking Dropping of Items
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "InvokeUserCode_CmdHarvestAnimal", typeof(BlockHarvest), "InvokeUserCode_CmdHarvestAnimalPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "UserCode_CmdHarvestAnimal", typeof(BlockHarvest), "UserCode_CmdHarvestAnimalPrefix");

        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "InvokeUserCode_CmdHarvestAnimalToInv", typeof(BlockHarvest), "InvokeUserCode_CmdHarvestAnimalToInvPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "UserCode_CmdHarvestAnimalToInv", typeof(BlockHarvest), "UserCode_CmdHarvestAnimalToInvPrefix");

        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdHarvestCrabPot", typeof(BlockHarvest), "InvokeUserCode_CmdHarvestCrabPotPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdHarvestCrabPot", typeof(BlockHarvest), "UserCode_CmdHarvestCrabPotPrefix");

        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdHarvestOnTile", typeof(BlockHarvest), "InvokeUserCode_CmdHarvestOnTilePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdHarvestOnTile", typeof(BlockHarvest), "UserCode_CmdHarvestOnTilePrefix");

        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdHarvestOnTileDeath", typeof(BlockHarvest), "InvokeUserCode_CmdHarvestOnTileDeathPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdHarvestOnTileDeath", typeof(BlockHarvest), "UserCode_CmdHarvestOnTileDeathPrefix");
    }

    public static bool checkConditions() {
        if (!LockHarvest.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdHarvestAnimalPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdHarvestAnimalPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdHarvestAnimalToInvPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdHarvestAnimalToInvPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdHarvestCrabPotPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdHarvestCrabPotPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdHarvestOnTilePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdHarvestOnTilePrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdHarvestOnTileDeathPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdHarvestOnTileDeathPrefix() => checkConditions();
}
