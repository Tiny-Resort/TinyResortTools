using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    public static class TRLicenses {

        private static TRModData Data;
        private static int LicenceTypesCount = Enum.GetNames(typeof(LicenceManager.LicenceTypes)).Length;
        private static List<TRCustomLicense> CustomLicenses = new List<TRCustomLicense>();
        private static Sprite defaultLicenseSprite;
        
        internal static void Initialize() {

            TRTools.QuickPatch(typeof(LicenceManager), "Start", typeof(TRLicenses), "StartPatch");
            TRTools.QuickPatch(typeof(LicenceManager), "getLicenceName", typeof(TRLicenses), "getLicenceNamePatch");
            TRTools.QuickPatch(typeof(LicenceManager), "getLicenceLevelDescription", typeof(TRLicenses), "getLicenceLevelDescriptionPatch");
            TRTools.QuickPatch(typeof(LicenceManager), "checkForUnlocksOnLevelUp", typeof(TRLicenses), "checkForUnlocksOnLevelUpPatch");
            TRTools.QuickPatch(typeof(Licence), "getNextLevelPrice", typeof(TRLicenses), "getNextLevelPricePrefix");
            //TRTools.QuickPatch(typeof(Licence), "canAffordNextlevel", typeof(TRLicenses), "canAffordNextlevelPrefix");
            //TRTools.QuickPatch(typeof(Licence), "getCurrentMaxLevel", typeof(TRLicenses), "getCurrentMaxLevelPrefix");

            Data = TRData.Subscribe("TR.CustomLicenses");
            TRData.cleanDataEvent += UnloadLicenses;
            TRData.preSaveEvent += SaveLicenseData;
            TRData.postLoadEvent += LoadLicenseData;
            TRData.injectDataEvent += LoadLicenses;

            defaultLicenseSprite = TRAssets.ImportSprite(Path.Combine("TR Tools", "textures", "default_license.png"), Vector2.one * 0.5f);

        }

        /// <summary>Adds a custom license to the system. Must be done for each custom license.</summary>
        /// <param name="pluginGUID">The pluginGuid of your plugin. Changing this after save data has been made WILL result in lost save data.</param>
        /// <param name="uniqueID">A unique string you are assigning this license only. Changing this after save data has been made WILL result in save data mixups.</param>
        /// <param name="name">The name that will appear on the license in-game. (Can be changed at any time without issue)</param>
        /// <param name="color">The color of the banner for the license.</param>
        /// <param name="maxLevel">The highest unlockable level for this license.</param>
        /// <param name="licenseIcon">The sprite used for the badge for the license. If left null, a default sprite will be used.</param>
        /// <returns>The custom license that is created. Save a reference to this in order to access its state at any time.</returns>
        public static TRCustomLicense AddLicense(string pluginGUID, string uniqueID, string name, Color color, int maxLevel = 1, Sprite licenseIcon = null) {
            
            var NewLicense = new TRCustomLicense {
                pluginGuid = pluginGUID,
                info = new Licence(),
                uniqueID = uniqueID,
                licenseIcon = licenseIcon == null ? defaultLicenseSprite : licenseIcon,
                title = name,
                color = color,
                maxLevel = maxLevel,
                licenseIndex = LicenceTypesCount + CustomLicenses.Count
            };

            NewLicense.SetInfo();
            CustomLicenses.Add(NewLicense);
            return NewLicense;
            
        }

        // Adds custom licenses to the game licences
        internal static bool StartPatch(LicenceManager __instance) {

            // Gets original arrays of licence data
            __instance.allLicences = new Licence[LicenceTypesCount + CustomLicenses.Count];
            var licenceIcons = new List<Sprite>(__instance.licenceIcons);
            var licenseColors = new List<Color>(__instance.licenceColours);

            // Adds custom licenses
            for (var i = 0; i < CustomLicenses.Count; i++) {
                licenceIcons.Add(CustomLicenses[i].licenseIcon);
                licenseColors.Insert(LicenceTypesCount + i - 1, CustomLicenses[i].color); 
            }
            
            // Combines list of custom and standard licenses
            __instance.licenceIcons = licenceIcons.ToArray();
            __instance.licenceColours = licenseColors.ToArray();

            // Gets references to the buttn lists
            var licenceButtons = (List<LicenceButton>)AccessTools.Field(typeof(LicenceManager), "licenceButtons").GetValue(__instance);
            var journalButtons = (List<LicenceButton>)AccessTools.Field(typeof(LicenceManager), "journalButtons").GetValue(__instance);
            
            // Fills in information for each license
            for (int i = 1; i < __instance.allLicences.Length; i++) {
                
                if (i >= LicenceTypesCount) { __instance.allLicences[i] = CustomLicenses[i - LicenceTypesCount].info; } 
                else { __instance.allLicences[i] = new Licence((LicenceManager.LicenceTypes) i); }
                
                LicenceButton component = GameObject.Instantiate(__instance.licenceButtonPrefab, __instance.licenceButtonParent).GetComponent<LicenceButton>();
                component.licenceIcon.preserveAspect = true;
                component.fillButton(i);
                licenceButtons.Add(component);
                
                LicenceButton component2 = GameObject.Instantiate(__instance.journalButtonPrefab, __instance.journalWindow).GetComponent<LicenceButton>();
                component2.licenceIcon.preserveAspect = true;
                component2.fillDetailsForJournal(i);
                journalButtons.Add(component2);
                
            }
            
            // Initializes certain values for licenses
            __instance.setLicenceLevelsAndPrice();
            __instance.licenceIcon.preserveAspect = true;
            
            // Updates the button information
            for (int j = 0; j < licenceButtons.Count; j++) {
                licenceButtons[j].updateButton();
                journalButtons[j].updateJournalButton();
            }
            
            // Sorts the buttons
            __instance.sortLicenceList(licenceButtons);
            __instance.sortLicenceList(journalButtons);

            return false;

        }
        
        #region Patches

        /*[HarmonyPrefix]
        internal static bool getCurrentMaxLevelPrefix(Licence __instance, ref int __result) {
            if ((int) __instance.type >= LicenceTypesCount) {
                for (int i = 0; i < __instance.maxLevel; i++) {
                    if (i * __instance.unlockedEveryLevel > CharLevelManager.manage.currentLevels[(int)__instance.unlockedBySkill]) { return i; }
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        internal static bool canAffordNextlevelPrefix(Licence __instance) {
            if ((int) __instance.type >= LicenceTypesCount) {
                
                return false;
            }
            return true;
        }*/

        // Patches the function that gets license level cost
        [HarmonyPrefix]
        internal static bool getNextLevelPricePrefix(Licence __instance, ref int __result) {
            if ((int) __instance.type >= LicenceTypesCount) {
                var license = CustomLicenses[(int) __instance.type - LicenceTypesCount];
                if (!license.costs.TryGetValue(license.level + 1, out var val)) return true;
                __result = val;
                return false;
            }
            return true;
        }

        // Patches the function that gets a license name so that it can understand custom licenses
        [HarmonyPrefix]
        internal static bool getLicenceNamePatch(LicenceManager __instance, ref string __result, LicenceManager.LicenceTypes type) {
            if ((int)type >= LicenceTypesCount) {
                __result = CustomLicenses[(int) type - LicenceTypesCount].title;
                return false;
            }
            return true;
        }

        // If this is a custom license, return our own descriptions
        [HarmonyPrefix]
        internal static bool getLicenceLevelDescriptionPatch(LicenceManager __instance, ref string __result, ref LicenceManager.LicenceTypes type, ref int level) {
            if ((int)type >= LicenceTypesCount) {
                var license = CustomLicenses[(int)type - LicenceTypesCount];
                if (!license.descriptions.TryGetValue(level, out var desc)) {
                    if (level > 0 && level < license.maxLevel) { TRTools.Log("Custom License " + license.title + " has no description for level " + level); }
                    __result = "";
                } else { __result = desc; }
                return false;
            }
            return true;

        }

        internal static bool checkForUnlocksOnLevelUpPatch(LicenceManager __instance, Licence check, bool loadCheck) {

            foreach (var license in CustomLicenses) {

                if (license.info != check) continue;

                // TODO: Unlock licenses that have met requirements
                return false;

            }

            return true;

        }
        
        #endregion
        
        #region Saving & Loading Data

        // Makes sure the only licenses being saved to the slot are base game onesinternal
        internal static void UnloadLicenses() {
            var currentLicenses = LicenceManager.manage.allLicences;
            LicenceManager.manage.allLicences = new Licence[LicenceTypesCount];
            for (var i = 0; i < LicenceManager.manage.allLicences.Length; i++) {
                LicenceManager.manage.allLicences[i] = currentLicenses[i];
            }
        }

        // Loads in the custom licenses and their save data
        internal static void LoadLicenses() {
            var currentLicenses = LicenceManager.manage.allLicences;
            LicenceManager.manage.allLicences = new Licence[LicenceTypesCount + CustomLicenses.Count];
            for (var i = 0; i < LicenceManager.manage.allLicences.Length; i++) {
                if (i < LicenceTypesCount) { LicenceManager.manage.allLicences[i] = currentLicenses[i]; }
                else { LicenceManager.manage.allLicences[i] = CustomLicenses[i - LicenceTypesCount].info; } 
            }
        }

        internal static void SaveLicenseData() {
            for (var i = 0; i < CustomLicenses.Count; i++) {
                Data.SetValue(CustomLicenses[i].pluginGuid + " - " + CustomLicenses[i].uniqueID, CustomLicenses[i].info.currentLevel);
            }
        }

        internal static void LoadLicenseData() {
            for (var i = 0; i < CustomLicenses.Count; i++) {
                var val = Data.GetValue(CustomLicenses[i].pluginGuid + " - " + CustomLicenses[i].uniqueID);
                if (val == null) continue;
                CustomLicenses[i].info.currentLevel = (int) val;
                CustomLicenses[i].SetInfo();
            }
        }
        
        #endregion

    }

    [Serializable]
    public class TRCustomLicense {

        public int level => info.currentLevel;
        internal Licence info;

        internal string pluginGuid;
        internal string uniqueID;
        internal int licenseIndex;
        
        // Appearance
        internal string title;
        internal Color color;
        internal Sprite licenseIcon;
        
        // Descriptions must be set per-level
        internal Dictionary<int, string> descriptions = new Dictionary<int, string>();
        internal Dictionary<int, int> costs = new Dictionary<int, int>();
        
        internal int maxLevel = 3;

        internal bool unlockedWithLevel;
        internal CharLevelManager.SkillTypes unlockedBySkill;
        internal int unlockedEveryLevel;

        /// <summary>Sets the description and cost for a specific level of the license.</summary>
        /// <param name="setLevel">What level of the license is being changed.</param>
        /// <param name="description">What description should be shown for that level.</param>
        /// <param name="cost">How many permit points it will cost to buy this level.</param>
        public void SetLevelInfo(int setLevel, string description, int cost) {
            descriptions[setLevel] = description;
            costs[setLevel] = cost;
        }

        /// <summary>Sets a requirement that your license can only be purchased when the player has a minimum level in a particular skill.</summary>
        /// <param name="skill">What skill the license will be linked with. You can only link to one skill per license.</param>
        /// <param name="skillLevelsPerLicenseLevel">How many levels in the given skill the player must gain before unlocking the next level of this license.</param>
        public void ConnectToSkill(CharLevelManager.SkillTypes skill, int skillLevelsPerLicenseLevel) {
            unlockedWithLevel = true;
            unlockedBySkill = skill;
            unlockedEveryLevel = skillLevelsPerLicenseLevel;
            SetInfo();
        }

        internal void SetInfo() {
            info.isUnlocked = true;
            info.type = (LicenceManager.LicenceTypes) licenseIndex;
            info.maxLevel = maxLevel;
            info.unlockedWithLevel = unlockedWithLevel;
            info.unlockedBySkill = unlockedBySkill;
            info.unlockedEveryLevel = unlockedEveryLevel;
        }
        
    }

}
