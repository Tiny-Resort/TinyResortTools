using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    /// <summary>A framework for creating custom licenses and using them to gate your mod's features behind player progression.</summary>
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

            LeadPlugin.plugin.AddCommand("unlock_license", "Unlocks the specified license at no cost. Use the list_licenses command to get the license names.", UnlockLicense, "LicenseName", "Level");
            LeadPlugin.plugin.AddCommand("list_licenses", "Lists all custom licenses added by any mods.", ListLicenses);

            Data = TRData.Subscribe("TR.CustomLicenses");
            TRData.cleanDataEvent += UnloadLicenses;
            TRData.preSaveEvent += SaveLicenseData;
            TRData.postLoadEvent += LoadLicenseData;
            TRData.injectDataEvent += LoadLicenses;

            defaultLicenseSprite = TRAssets.LoadSprite(Path.Combine("custom_assets", "license_icons", "default_license.png"), Vector2.one * 0.5f);

        }

        internal static string ListLicenses(string[] args) {
            if (CustomLicenses.Count <= 0) { return "None of the installed mods have a custom license."; }
            var str = "\nThe following custom licenses exist:\n";
            foreach (var li in CustomLicenses) { str += li.title + " (ID: " + li.nexusID + "." + li.licenseID + ")" + "\n"; }
            return str;
        }

        internal static string UnlockLicense(string[] args) {
            if (args.Length <= 0) { return "<color=red>No license specified.</color> " + ListLicenses(null); }
            foreach (var li in CustomLicenses) {
                if ((li.nexusID + "." + li.licenseID).ToLower() == args[0]) {
                    li.info.currentLevel = li.info.maxLevel;
                    return $"Unlocking all levels of license \"{li.title}\" (ID: {li.nexusID}.{li.licenseID})";
                }
            }
            return "<color=red>Could not find a license with the specified ID.</color>";
        }

        internal static TRCustomLicense AddLicense(int nexusID, int licenseID, string licenseName, int maxLevel = 1) {
            
            var NewLicense = new TRCustomLicense {
                nexusID = nexusID,
                info = new Licence(),
                licenseID = licenseID,
                licenseIcon = defaultLicenseSprite,
                title = licenseName,
                maxLevel = maxLevel,
                licenseIndex = LicenceTypesCount + CustomLicenses.Count
            };

            for (var i = 1; i <= maxLevel; i++) { NewLicense.SetLevelInfo(i, "This is a placeholder description for level " + i + ".", 500 * i); }

            NewLicense.SetInfo();
            CustomLicenses.Add(NewLicense);
            return NewLicense;
            
        }

        #region Patches

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
            if ((int)type >= LicenceTypesCount && (int)type - LicenceTypesCount < CustomLicenses.Count) {
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
                Data.SetValue(CustomLicenses[i].nexusID + "." + CustomLicenses[i].licenseID, CustomLicenses[i].info.currentLevel);
            }
        }

        internal static void LoadLicenseData() {
            for (var i = 0; i < CustomLicenses.Count; i++) {
                var val = Data.GetValue(CustomLicenses[i].nexusID + "." + CustomLicenses[i].licenseID);
                if (val == null) continue;
                CustomLicenses[i].info.currentLevel = (int) val;
                CustomLicenses[i].SetInfo();
            }
        }
        
        #endregion

    }

    /// <summary>Information and functions related to your custom license.</summary>
    [Serializable]
    public class TRCustomLicense {

        /// <summary>The player's currently unlocked level for this license. If 0, the player has not unlocked this license.</summary>
        public int level => info.currentLevel;
        internal Licence info;

        internal int nexusID;
        internal int licenseID;
        internal int licenseIndex;
        
        // Appearance
        internal string title;
        internal Color color = new Color(195f / 255f, 135f / 255f, 112f / 255f, 1);
        internal Sprite licenseIcon;
        
        // Descriptions must be set per-level
        internal Dictionary<int, string> descriptions = new Dictionary<int, string>();
        internal Dictionary<int, int> costs = new Dictionary<int, int>();
        
        internal int maxLevel = 3;

        internal bool unlockedWithLevel;
        internal CharLevelManager.SkillTypes unlockedBySkill;
        internal int unlockedEveryLevel;

        /// <summary>Sets the color of the banner for the license. Keep in mind this will not change the color of the license icon.</summary>
        public void SetColor(Color col) { color = col; }

        /// <summary>Sets the icon used to represent the license.</summary>
        /// <param name="iconFileName">The name (including extension) of the image file you want to use as an icon for the license. This file must be placed in the custom_assets/license_icons/ folder.</param>
        public void SetIcon(string iconFileName) { licenseIcon = TRAssets.LoadSprite(Path.Combine("custom_assets", "license_icons", iconFileName), Vector2.one * 0.5f); }

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
