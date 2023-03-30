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
using Object = UnityEngine.Object;

namespace TinyResort; 

public class ClientManagement {

    public static CurrentPlayers[] currentlyConnectedSteamIDs = new CurrentPlayers[3];
    public static CurrentPlayers currentPlayer;
    public static CurrentPlayers chatPlayer;

    public static List<CurrentPlayers> bannedPlayers = new();
    public static List<CurrentPlayers> currentPlayersInGame = new();

    public static List<CurrentPlayers> fullWhitelist = new();
    public static List<CurrentPlayers> currentWhitelist = new();

    public static List<CurrentPlayers> fullBlacklist = new();
    public static List<CurrentPlayers> currentBlacklist = new();

    public static NetworkPlayersManager networkInstance;

    public static bool playerIsConnecting;
    public static bool bannedButtonPressed = false;

    public static string[] whitelistName;

    public static int kickID = -1;

    public static ulong SteamIDDuringConnection = 0;

    public static GameObject GO;

    [Serializable]
    public class CurrentPlayers {
        public string playerName;
        public ulong SteamID;
        public int connectionID;
    }

    public static void init() {
        LeadPlugin.plugin.QuickPatch(typeof(NextServer), "OnConnectionStatusChanged", typeof(ClientManagement), "OnConnectionStatusChangedPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(NetworkPlayersManager), "addPlayer", typeof(ClientManagement), null, "addPlayerPostfix");
        LeadPlugin.plugin.QuickPatch(typeof(NetworkPlayersManager), "kickPlayer", typeof(ClientManagement), "kickPlayerPrefix");
        LeadPlugin.plugin.QuickPatch(typeof(NetworkPlayersManager), "refreshButtons", typeof(ClientManagement), null, "refreshButtonsPostfix");
    }

    public static void OnConnectionStatusChangedPrefix(NextServer __instance, SteamNetConnectionStatusChangedCallback_t param) {
        var steamID = param.m_info.m_identityRemote.GetSteamID64();
        var playerBanned = bannedPlayers.Find(i => i.SteamID == steamID);

        if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting) {
            if (playerBanned != null) { SteamNetworkingSockets.CloseConnection(param.m_hConn, 0, "Banned User", false); }
            else {
                playerIsConnecting = true;
                SteamIDDuringConnection = steamID;
            }
        }
    }

    [HarmonyPostfix]
    public static void addPlayerPostfix(NetworkPlayersManager __instance, CharMovement newChar) {
        networkInstance = __instance;
        if (newChar.connectionToClient.connectionId != 0 && playerIsConnecting && SteamIDDuringConnection != 0) {
            var tmpCurrent = new CurrentPlayers();
            tmpCurrent.connectionID = newChar.connectionToClient.connectionId;
            tmpCurrent.SteamID = SteamIDDuringConnection;
            currentPlayersInGame.Add(tmpCurrent);
            createBanButton(__instance.connectedChars.Count - 1);
        }
    }

    public static void refreshButtonsPostfix(NetworkPlayersManager __instance) {
        for (var i = 0; i < __instance.connectedChars.Count; i++) {
            currentPlayer = currentPlayersInGame.Find(j => j.connectionID == __instance.connectedChars[i].connectionToClient.connectionId);
            if (currentPlayer != null) currentPlayer.playerName = __instance.connectedChars[i].myEquip.playerName;
        }
    }

    [HarmonyPrefix]
    public static void kickPlayerPrefix(NetworkPlayersManager __instance, int id) {
        networkInstance = __instance;
        currentPlayer = currentPlayersInGame.Find(i => i.connectionID == __instance.connectedChars[id].connectionToClient.connectionId);
        if (currentPlayer != null) kickID = id;
    }

    public static void listBannedPlayers() {
        for (var i = 0; i < bannedPlayers.Count; i++) {
            LeadPlugin.plugin.Log($"Player {bannedPlayers[i].playerName} is banned.");
            NotificationManager.manage.createChatNotification($"Player {bannedPlayers[i].playerName} is banned.");
        }
    }

    public static void BanPlayer() {
        if (kickID != -1) {
            LeadPlugin.plugin.Log($"Name: {currentPlayer.playerName} | ID: {currentPlayer.connectionID} | Steam: {currentPlayer.SteamID}");
            bannedPlayers.Add(currentPlayer);
            kickID = -1;
            currentPlayer = null;
            GriefProtection.SaveBanList();
        }
    }

    // TODO: Chat commands arent done and prob dont work

    public static void BanPlayerChatCommand() {
        if (kickID != -1) {
            LeadPlugin.plugin.Log($"Name: {currentPlayer.playerName} | ID: {currentPlayer.connectionID} | Steam: {currentPlayer.SteamID}");
            bannedPlayers.Add(currentPlayer);
            kickID = -1;
            currentPlayer = null;
            GriefProtection.SaveBanList();
        }
    }
    /*public static void unbanPlayerChatCommand(string[] playerName) {
        currentPlayer = currentPlayersInGame.Find(i => i.playerName == playerName);
        bannedPlayers.Remove(currentPlayer);
        GriefProtection.SaveBanList();
    }*/

    public static void addBlacklistPlayerChatCommand(string[] playerName) {
        chatPlayer = null;
        for (var i = 0; i < networkInstance.connectedChars.Count; i++) {
            chatPlayer = currentPlayersInGame.Find(player => player.connectionID == networkInstance.connectedChars[i].connectionToClient.connectionId);
            if (chatPlayer != null) break;
        }
        if (chatPlayer != null) {
            LeadPlugin.plugin.Log($"Added {playerName[0]} to the Blacklist.");
            currentBlacklist.Add(chatPlayer);
        }
    }

    public static string addWhitelistPlayerChatCommand(string[] playerName) {
        LeadPlugin.plugin.Log($"Running method");
        chatPlayer = null;
        for (var i = 0; i < networkInstance.connectedChars.Count; i++) {
            chatPlayer = currentPlayersInGame.Find(player => player.connectionID == networkInstance.connectedChars[i].connectionToClient.connectionId);
            if (chatPlayer != null) break;
        }
        if (chatPlayer != null) {
            LeadPlugin.plugin.Log($"Added {playerName[0]} to the Whitelist.");
            currentWhitelist.Add(chatPlayer);
        }
        return $"Added {playerName[0]} to the whitelist.";
    }

    public static void removeWhitelistPlayerChatCommand(string[] playerName) {
        chatPlayer = null;
        for (var i = 0; i < networkInstance.connectedChars.Count; i++) {
            chatPlayer = currentPlayersInGame.Find(player => player.connectionID == networkInstance.connectedChars[i].connectionToClient.connectionId);
            if (chatPlayer != null) break;
        }
        if (chatPlayer != null) {
            LeadPlugin.plugin.Log($"Added {playerName[0]} to the Whitelist.");
            currentWhitelist.Add(chatPlayer);
        }
    }

    public static void resetBanList() => bannedPlayers.Clear();

    public static void createBanButton(int id) {
        GO = Object.Instantiate(NetworkPlayersManager.manage.playerButttons[id].transform.GetChild(1).gameObject, NetworkPlayersManager.manage.playerButttons[id].transform.GetChild(1).parent, true);
        var banButton = GO.GetComponent<InvButton>();
        banButton.onButtonPress.AddListener(delegate { BanPlayer(); });
        var banText = GO.GetComponentInChildren<TextMeshProUGUI>();
        banText.text = "BAN";
        banText.enableWordWrapping = false;
        var Im = GO.GetComponent<Image>();
        Im.rectTransform.anchoredPosition += new Vector2(-80, 0);
    }
}
