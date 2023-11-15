using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Bootstrap;
using HarmonyLib;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TinyResort
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    internal class LeadPlugin : BaseUnityPlugin {

        public const string pluginName = "TRTools";
        public const string pluginGuid = "dev.TinyResort." + pluginName;
        public const string pluginVersion = "0.8.4";

        public static TRPlugin plugin;
        internal static LeadPlugin instance;
        internal static int currentSlot;

        internal static ConfigEntry<bool> developerMode;
        public static ConfigEntry<bool> useSlashToOpenChat;
        private static bool initialSceneSetupDone;

        private void Awake() {

            instance = this;
            plugin = this.Initialize(83, "tr");
            plugin.harmony.PatchAll();

            useSlashToOpenChat = Config.Bind(
                "Chat", "UseSlashToOpenChat", true,
                "If true, then pressing forward slash on the keyboard will open the chat box with a slash already in place."
            );
            developerMode = Config.Bind(
                "Developer", "DeveloperMode", false,
                "If true, allows the use of nexusID being set to '-1'. Default is false, so you will need to update nexusID before releasing for the enduser."
            );

            // If in developer mode, give a big warning about nexus ID usage
            if (developerMode.Value) TRTools.LogError(TRTools.TRDeveloperMode());

            TRLicences.Initialize();
            TRItems.Initialize();
            TRQuickItems.LoadAllQuickItems();

            SceneManager.sceneLoaded += SceneLoaded;

        }

        internal void SceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name != "scene1") return;
            TRTools.sceneSetupEvent?.Invoke();
        }

        private void Start() {

            TRNetwork.Initialize();
            TRInterface.Initialize();
            TRModUpdater.Initialize();
            TRConflictingPlugins.Initialize();
            TRItems.ManageAllItemArray();
            TRIcons.Initialize();
            TRBackup.Initialize();

            // GriefProtection.IntializeGriefProtection();
            /*var TestLicense = plugin.AddLicence(1, "Test License 1",  10);
            TestLicense.SetColor(Color.cyan);
            TestLicense.SetLevelInfo(1, "Level 1: This is a license made for testing the framework.", 500);
            TestLicense.SetLevelInfo(2, "Level 2: This is a license made for testing the framework.", 1500);
            TestLicense.SetLevelInfo(3, "Level 3: This is a license made for testing the framework.", 2500);
            TestLicense.AddSkillRequirement(1, CharLevelManager.SkillTypes.Mining, 10);
            TestLicense.AddSkillRequirement(2, CharLevelManager.SkillTypes.Mining, 20);
            TestLicense.AddSkillRequirement(3, CharLevelManager.SkillTypes.Mining, 30);*/
        }

        private void Update() {

            // Ensures scene setup happens on first load as well
            if (!initialSceneSetupDone) {
                initialSceneSetupDone = true;
                TRTools.sceneSetupEvent?.Invoke();
            }

            TRModUpdater.Update();
            TRConflictingPlugins.Update();
            TRNetwork.Update();

            if (NetworkMapSharer.Instance.localChar) {
                TRBackup.LoadSavedBackups();
                TRIcons.InitializeIcons();
            }
            if (NetworkMapSharer.Instance.localChar && !TRItems.fixedRecipes) TRItems.FixRecipes();

            #region For Testing Only

            /*
            if (Input.GetKeyDown(KeyCode.Home)) {
                TRNetwork.share.RpcCustomRPC("Sent to Both Host/Client from Host.");

                /*foreach (var character in NetworkPlayersManager.manage.connectedChars) {
                    TRTools.LogError($"connectionToClient {character.connectionToClient}");
                    TRTools.LogError($"connectionToServer: {character.connectionToServer}");
                }
                TRNetwork.share.TargetSendMessageToClient(NetworkPlayersManager.manage.connectedChars[0].connectionToClient, "test");#1#
                TRNetwork.share.CmdSendMessageToHost("Sent to Host from Client");
            }
            */

            /*
            if (Input.GetKeyDown(KeyCode.F11)) GriefProtection.ResetBanList();
            if (Input.GetKeyDown(KeyCode.F10)) ClientManagement.listBannedPlayers();
            if (Input.GetKeyDown(KeyCode.End) && ChestWindow.chests.chestWindowOpen) GriefProtection.LockChest();
            */

            //if (Input.GetKeyDown(KeyCode.F11)) { TRItems.UnloadCustomItems(); }
            //if (Input.GetKeyDown(KeyCode.F12)) { TRItems.CurrentSaveInfo(); }
            //if (Input.GetKeyDown(KeyCode.F10)) { TRItems.LoadCustomItems(); }
            //if (Input.GetKeyDown(KeyCode.F9)) { 
            //     TRTools.Log($"Size: {SaveLoad.saveOrLoad.carryablePrefabs.Length}");
            //     NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[10], NetworkMapSharer.Instance.localChar.transform.position, true);
            // }

            #endregion

        }

    }
}
