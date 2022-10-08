using System.ComponentModel;
using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TinyResort {

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    internal class LeadPlugin : BaseUnityPlugin {

        public static TRPlugin plugin;
        internal static LeadPlugin instance;

        public static ConfigEntry<bool> useSlashToOpenChat;
        
        public const string pluginName = "TRTools";
        public const string pluginGuid = "dev.TinyResort." + pluginName;
        public const string pluginVersion = "0.5.0";

        private void Awake() {

            instance = this;
            plugin = TRTools.Initialize(this, 83, "tr");
            plugin.harmony.PatchAll();
            // Initializing early so we can use it for all other stuff. 

            useSlashToOpenChat = Config.Bind("Chat", "UseSlashToOpenChat", true, "If true, then pressing forward slash on the keyboard will open the chat box with a slash already in place.");
            
            TRLicenses.Initialize();
            TRIcons.Initialize();
            TRItems.Initialize();
            TRCustomClothes.Initialize();
        }

        private void Start() {
            TRInterface.Initialize();
            TRModUpdater.Initialize();
            TRItems.ManageAllItemArray();
            

        }

        private void Update() {
            TRModUpdater.Update();

            if (NetworkMapSharer.share.localChar) TRIcons.InitializeIcons();
            
            if (Input.GetKeyDown(KeyCode.F11)) {
                TRItems.UnloadCustomItems();
            }
            if (Input.GetKeyDown(KeyCode.F10)) { TRItems.LoadCustomItems(); }
            if (Input.GetKeyDown(KeyCode.F9)) { 
                TRTools.Log($"Size: {SaveLoad.saveOrLoad.carryablePrefabs.Length}");
                NetworkMapSharer.share.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[10], NetworkMapSharer.share.localChar.transform.position, true);
            }
        }
        
        
        /*private void Start() {

            /*var TestLicense = TRLicenses.AddLicense(pluginGuid, "001", "Test License 1", Color.cyan, 500, 3, 1, LicenceManager.LicenceTypes.Mining);
            TestLicense.SetDescription(1, "Level 1: This is a license made for testing the framework.");
            TestLicense.ConnectToSkill(CharLevelManager.SkillTypes.Mining, 10);

            var TestLicense2 = TRLicenses.AddLicense(pluginGuid, "002", "Test License 2", Color.red, 250, 2, 3, LicenceManager.LicenceTypes.Bridge);
            TestLicense2.SetDescription(1, "Level 1: This is a license made for testing the framework.");
            TestLicense2.SetDescription(2, "Level 2: This is a license made for testing the framework.");
            TestLicense2.SetDescription(3, "Level 3: This is a license made for testing the framework.");
            TestLicense2.ConnectToSkill(CharLevelManager.SkillTypes.Farming, 20);

            var TestLicense3 = TRLicenses.AddLicense(pluginGuid, "003", "Test License 3", Color.red, 680, 2, 3, LicenceManager.LicenceTypes.Fishing);
            TestLicense3.SetDescription(1, "Level 1: This is a license made for testing the framework.");
            TestLicense3.SetDescription(2, "Level 2: This is a license made for testing the framework.");
            TestLicense3.SetDescription(3, "Level 3: This is a license made for testing the framework.");#1#
            
        }*/
        
    }

}
