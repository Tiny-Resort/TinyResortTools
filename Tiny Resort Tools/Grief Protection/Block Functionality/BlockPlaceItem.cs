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

public class BlockPlaceItem {

    public static int currentConnectionID;
    public static ConfigEntry<bool> LockPlaceItems;

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdPlaceAnimalInCollectionPoint", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceAnimalInCollectionPointPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdPlaceAnimalInCollectionPoint", typeof(BlockPlaceItem), "UserCode_CmdPlaceAnimalInCollectionPointPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPlaceBridgeTileObject", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceBridgeTileObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPlaceBridgeTileObject", typeof(BlockPlaceItem), "UserCode_CmdPlaceBridgeTileObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(Tractor), "InvokeUserCode_CmdPlaceFertilizer", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceFertilizerPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(Tractor), "UserCode_CmdPlaceFertilizer", typeof(BlockPlaceItem), "UserCode_CmdPlaceFertilizerPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPlaceItemInToTileObject", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceItemInToTileObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPlaceItemInToTileObject", typeof(BlockPlaceItem), "UserCode_CmdPlaceItemInToTileObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPlaceItemOnTopOf", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceItemOnTopOfPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPlaceItemOnTopOf", typeof(BlockPlaceItem), "UserCode_CmdPlaceItemOnTopOfPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPlaceItemOnTopOfInside", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceItemOnTopOfInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPlaceItemOnTopOfInside", typeof(BlockPlaceItem), "UserCode_CmdPlaceItemOnTopOfInsidePrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "InvokeUserCode_CmdPlaceMarkerOnMap", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceMarkerOnMapPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharMovement), "UserCode_CmdPlaceMarkerOnMap", typeof(BlockPlaceItem), "UserCode_CmdPlaceMarkerOnMapPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "InvokeUserCode_CmdPlaceMultiTiledObject", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceMultiTiledObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharInteract), "UserCode_CmdPlaceMultiTiledObject", typeof(BlockPlaceItem), "UserCode_CmdPlaceMultiTiledObjectPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "InvokeUserCode_CmdPlaceOntoAnimal", typeof(BlockPlaceItem), "InvokeUserCode_CmdPlaceOntoAnimalPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(CharPickUp), "UserCode_CmdPlaceOntoAnimal", typeof(BlockPlaceItem), "UserCode_CmdPlaceOntoAnimalPrefix");
    }

    public static bool checkConditions() {
        if (!LockPlaceItems.Value) return true;

        var whitelist = ClientManagement.currentWhitelist.Any(i => i.connectionID == currentConnectionID);
        var blacklist = ClientManagement.currentBlacklist.Any(i => i.connectionID == currentConnectionID);

        if (whitelist) return true;
        if (blacklist) return false;

        if (currentConnectionID > 0) return false;

        return true;
    }

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceAnimalInCollectionPointPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceAnimalInCollectionPointPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceBridgeTileObjectPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceBridgeTileObjectPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceFertilizerPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceFertilizerPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceItemInToTileObjectPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceItemInToTileObjectPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceItemOnTopOfPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceItemOnTopOfPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceItemOnTopOfInsidePrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceItemOnTopOfInsidePrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceMarkerOnMapPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceMarkerOnMapPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceMultiTiledObjectPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceMultiTiledObjectPrefix() => checkConditions();

    [HarmonyPrefix] public static void InvokeUserCode_CmdPlaceOntoAnimalPrefix(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) =>
        currentConnectionID = senderConnection.connectionId;

    [HarmonyPrefix] public static bool UserCode_CmdPlaceOntoAnimalPrefix() => checkConditions();
}
