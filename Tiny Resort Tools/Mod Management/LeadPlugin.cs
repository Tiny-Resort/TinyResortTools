using System.Collections.Generic;
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

            useSlashToOpenChat = Config.Bind("Chat", "UseSlashToOpenChat", true, "If true, then pressing forward slash on the keyboard will open the chat box with a slash already in place.");
            
            TRLicences.Initialize();
            TRIcons.Initialize();
            TRItems.Initialize();
            TRQuickItems.Initialize();
        }

        private void Start() {
            
            TRInterface.Initialize();
            TRModUpdater.Initialize();
            TRConflictingPlugins.Initialize();
            TRItems.ManageAllItemArray();

            ItemChangeType ict = ItemChangerRecipe.CreateICT(1005);
            ItemChange ic = ItemChangerRecipe.CreateIC(1005);
            List<ItemChangeType> ictl = ItemChangerRecipe.CreateICTL(ic);
            ictl.Add(ict);
            ic.changesAndTheirChanger = ictl.ToArray();
            Inventory.inv.allItems[1005].itemChange = ic;
            /*var TestLicense = plugin.AddLicence(1, "Test License 1",  10);
            TestLicense.SetColor(Color.cyan);
            TestLicense.SetLevelInfo(1, "Level 1: This is a license made for testing the framework.", 500);
            TestLicense.SetLevelInfo(2, "Level 2: This is a license made for testing the framework.", 1500);
            TestLicense.SetLevelInfo(3, "Level 3: This is a license made for testing the framework.", 2500);
            TestLicense.AddSkillRequirement(1, CharLevelManager.SkillTypes.Mining, 10);
            TestLicense.AddSkillRequirement(2, CharLevelManager.SkillTypes.Mining, 20);
            TestLicense.AddSkillRequirement(3, CharLevelManager.SkillTypes.Mining, 30);
            
            var TestLicense2 = plugin.AddLicence(2, "Test License 2", 2);
            TestLicense2.SetLevelInfo(1, "Level 1: This is a license made for testing the framework.", 500);
            TestLicense2.SetLevelInfo(2, "Level 2: This is a license made for testing the framework.", 1500);
            TestLicense2.SetLevelInfo(3, "Level 3: This is a license made for testing the framework.", 3000);
            TestLicense2.AddSkillRequirement(1, CharLevelManager.SkillTypes.Farming, 10);
            TestLicense2.AddSkillRequirement(2, CharLevelManager.SkillTypes.Farming, 20);
            TestLicense2.AddSkillRequirement(3, CharLevelManager.SkillTypes.Farming, 30);

            var TestLicense3 = plugin.AddLicence(3, "Test License 3", 3);
            TestLicense3.SetLevelInfo(1, "Level 1: This is a license made for testing the framework.", 1000);
            TestLicense3.SetLevelInfo(2, "Level 2: This is a license made for testing the framework.", 2000);
            TestLicense3.SetLevelInfo(3, "Level 3: This is a license made for testing the framework.", 5000);
            TestLicense3.AddSkillRequirement(1, CharLevelManager.SkillTypes.Hunting, 10);
            TestLicense3.AddSkillRequirement(2, CharLevelManager.SkillTypes.Farming, 20);
            TestLicense3.AddSkillRequirement(3, CharLevelManager.SkillTypes.Farming, 30);
            TestLicense3.AddPrerequisite(TestLicense); */
        }

        private void Update() {
            
            TRModUpdater.Update();
            TRConflictingPlugins.Update();

            if (NetworkMapSharer.share.localChar) TRIcons.InitializeIcons();
            
            if (Input.GetKeyDown(KeyCode.F11)) { TRItems.UnloadCustomItems();
            }
            if (Input.GetKeyDown(KeyCode.F12)) { TRItems.CurrentSaveInfo(); }
            if (Input.GetKeyDown(KeyCode.F10)) { TRItems.LoadCustomItems(); }
            if (Input.GetKeyDown(KeyCode.F9)) { 
                TRTools.Log($"Size: {SaveLoad.saveOrLoad.carryablePrefabs.Length}");
                NetworkMapSharer.share.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[10], NetworkMapSharer.share.localChar.transform.position, true);
            }
        }
        
    }

}
