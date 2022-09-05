using System;
using System.Collections.Generic;
using System.ComponentModel;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    public static class TRLicenses {

        private static ModData Data;
        private static int LicenceTypesCount = Enum.GetNames(typeof(LicenceManager.LicenceTypes)).Length;
        private static List<ModLicense> CustomLicenses = new List<ModLicense>();
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Initialize() {

            TRTools.QuickPatch(typeof(LicenceManager), "Start", typeof(TRLicenses), "StartPatch");
            TRTools.QuickPatch(typeof(LicenceButton), "fillButton", typeof(TRLicenses), "fillButtonPatch");
            TRTools.QuickPatch(typeof(LicenceManager), "getLicenceName", typeof(TRLicenses), "getLicenceNamePatch");
            TRTools.QuickPatch(typeof(LicenceManager), "getLicenceLevelDescription", typeof(TRLicenses), "getLicenceLevelDescriptionPatch");
            TRTools.QuickPatch(typeof(LicenceManager), "setLicenceLevelsAndPrice", typeof(TRLicenses), "setLicenceLevelsAndPricePatch");
            TRTools.QuickPatch(typeof(LicenceManager), "checkForUnlocksOnLevelUp", typeof(TRLicenses), "checkForUnlocksOnLevelUpPatch");

            Data = TRData.Subscribe("TR.CustomLicenses");
            TRData.cleanDataEvent += UnloadLicensesBeforeSave;
            TRData.preSaveEvent += SaveLicenseData;
            TRData.postLoadEvent += LoadLicenseData;
            TRData.injectDataEvent += ReloadLicensesAfterSave;

        }
        
        /// <summary>Adds a custom license to the system. Must be done for each custom license.</summary>
        /// <param name="pluginGUID">The pluginGuid of your plugin. Changing this after save data has been made WILL result in save data mixups.</param>
        /// <param name="licenseID">A unique number you are assigning this license only. Changing this after save data has been made WILL result in save data mixups.</param>
        /// <param name="name">The name that will appear on the license in-game. (Can be changed at any time without issue)</param>
        /// <param name="color">The color of the banner for the license.</param>
        /// <param name="initialCost">How many permit points it costs to unlock the first level of this license.</param>
        /// <param name="levelCostMultiplier">What factor the permit point cost is multiplied by for each level above 1.</param>
        /// <param name="maxLevel">The highest unlockable level for this license.</param>
        /// <param name="iconType">The kind of icon that should be used for this license.</param>
        /// <returns>The custom license that is created. Save a reference to this in order to access its state at any time.</returns>
        public static ModLicense AddLicense(string pluginGUID, string name, Color color, 
                                            int initialCost = 250, int levelCostMultiplier = 2, int maxLevel = 1, 
                                            LicenceManager.LicenceTypes iconType = LicenceManager.LicenceTypes.Mining) {
            
            var NewLicense = new ModLicense {
                DinkumLicense = new Licence(),
                iconType = iconType,
                name = name,
                color = color,
                initialLevelCost = initialCost,
                levelCostMultiplier = levelCostMultiplier,
                maxLevel = maxLevel,
                pluginGuid = pluginGUID,
                licenseIndex = LicenceTypesCount + CustomLicenses.Count
            };
            
            
            SetLicenseInfo(NewLicense);
            CustomLicenses.Add(NewLicense);
            return NewLicense;
            
        }

        // Adds custom licenses to the game licences
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool StartPatch(LicenceManager __instance) {

            // Gets original arrays of licence data
            __instance.allLicences = new Licence[LicenceTypesCount + CustomLicenses.Count];
            var licenceIcons = new List<Sprite>(__instance.licenceIcons);
            var licenseColors = new List<Color>(__instance.licenceColours);

            // Adds custom licenses
            for (var i = 0; i < CustomLicenses.Count; i++) {
                licenceIcons.Add(__instance.licenceIcons[(int) CustomLicenses[i].iconType]);
                licenseColors.Add(CustomLicenses[i].color);
            }
            
            // Combines list of custom and standard licenses
            __instance.licenceIcons = licenceIcons.ToArray();
            __instance.licenceColours = licenseColors.ToArray();

            // Gets references to the buttn lists
            var licenceButtons = (List<LicenceButton>)AccessTools.Field(typeof(LicenceManager), "licenceButtons").GetValue(__instance);
            var journalButtons = (List<LicenceButton>)AccessTools.Field(typeof(LicenceManager), "journalButtons").GetValue(__instance);
            
            // Fills in information for each license
            for (int i = 1; i < __instance.allLicences.Length; i++) {
                if (i >= LicenceTypesCount) { __instance.allLicences[i] = CustomLicenses[i - LicenceTypesCount].DinkumLicense; } 
                else { __instance.allLicences[i] = new Licence((LicenceManager.LicenceTypes) i); }
                LicenceButton component = GameObject.Instantiate(__instance.licenceButtonPrefab, __instance.licenceButtonParent).GetComponent<LicenceButton>();
                component.fillButton(i);
                licenceButtons.Add(component);
                LicenceButton component2 = GameObject.Instantiate(__instance.journalButtonPrefab, __instance.journalWindow).GetComponent<LicenceButton>();
                component2.fillDetailsForJournal(i);
                journalButtons.Add(component2);
            }
            
            // Initializes certain values for licenses
            __instance.setLicenceLevelsAndPrice();
            
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

        // Patches the function that gets a license name so that it can understand custom licenses
        [HarmonyPrefix]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool getLicenceNamePatch(LicenceManager __instance, ref string __result, LicenceManager.LicenceTypes type) {
            if ((int)type >= LicenceTypesCount) {
                //TRTools.Log("Giving name " + CustomLicenses[(int) type - LicenceTypesCount].name);
                __result = CustomLicenses[(int) type - LicenceTypesCount].name;
                return false;
            }
            return true;
        }

        // 
        [HarmonyPostfix]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void fillButtonPatch(LicenceButton __instance, int newLicenceId) {
            //Debug.Log("Fill Name " + newLicenceId + ", " + LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)newLicenceId));
            //Debug.Log("Fill Description " + newLicenceId + ", " + LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)newLicenceId));
        }

        // If this is a custom license, return our own descriptions
        [HarmonyPrefix]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool getLicenceLevelDescriptionPatch(LicenceManager __instance, ref string __result, ref LicenceManager.LicenceTypes type, ref int level) {
            if ((int)type >= LicenceTypesCount) {
                var license = CustomLicenses[(int)type - LicenceTypesCount];
                if (!license.descriptions.TryGetValue(level, out var desc)) {
                    if (level > 0) { TRTools.Log("Custom License " + license.name + " has no description for level " + level); }
                    __result = "";
                } else { __result = desc; }
                return false;
            }
            return true;

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void setLicenceLevelsAndPricePatch(LicenceManager __instance) {

            foreach (var license in CustomLicenses) {
                
                // TODO: Connect to skill levels where necessary and set other things here 
                //license.DinkumLicence.connectToSkillLevel();
                
            }

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool checkForUnlocksOnLevelUpPatch(LicenceManager __instance, Licence check, bool loadCheck) {

            foreach (var license in CustomLicenses) {

                if (license.DinkumLicense != check) continue;

                // TODO: Unlock licenses that have met requirements
                return false;

            }

            return true;

        }
        
        #region Saving & Loading Data

        // Makes sure the only licenses being saved to the slot are base game ones
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UnloadLicensesBeforeSave() {
            var currentLicenses = LicenceManager.manage.allLicences;
            LicenceManager.manage.allLicences = new Licence[LicenceTypesCount];
            for (var i = 0; i < LicenceManager.manage.allLicences.Length; i++) {
                LicenceManager.manage.allLicences[i] = currentLicenses[i];
            }
        }

        // Loads in the custom licenses and their save data
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ReloadLicensesAfterSave() {
            var currentLicenses = LicenceManager.manage.allLicences;
            LicenceManager.manage.allLicences = new Licence[LicenceTypesCount + CustomLicenses.Count];
            for (var i = 0; i < LicenceManager.manage.allLicences.Length; i++) {
                if (i < LicenceTypesCount) { LicenceManager.manage.allLicences[i] = currentLicenses[i]; }
                else { LicenceManager.manage.allLicences[i] = CustomLicenses[i - LicenceTypesCount].DinkumLicense; } 
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveLicenseData() {
            for (var i = 0; i < CustomLicenses.Count; i++) {
                Data.SetValue(CustomLicenses[i].pluginGuid + " - " + CustomLicenses[i].name, CustomLicenses[i].DinkumLicense.currentLevel);
            }
            TRTools.Log("SAVING CUSTOM LICENSES: " + Data.Package.data.Count);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void LoadLicenseData() {
            for (var i = 0; i < CustomLicenses.Count; i++) {
                var val = Data.GetValue(CustomLicenses[i].pluginGuid + " - " + CustomLicenses[i].name);
                if (val == null) continue;
                CustomLicenses[i].DinkumLicense.currentLevel = (int) val;
                SetLicenseInfo(CustomLicenses[i]);
            }
        }
        
        #endregion

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetLicenseInfo(ModLicense license) {
            license.DinkumLicense.type = (LicenceManager.LicenceTypes) license.licenseIndex;
            license.DinkumLicense.maxLevel = license.maxLevel;
            license.DinkumLicense.levelCost = license.initialLevelCost;
            license.DinkumLicense.levelCostMuliplier = license.levelCostMultiplier;
            license.DinkumLicense.isUnlocked = true;
        }

    }

    [Serializable]
    public class ModLicense {

        public string pluginGuid;
        public int licenseIndex;
        
        public Licence DinkumLicense;
        
        // Appearance
        public string name;
        public Color color;
        public LicenceManager.LicenceTypes iconType;
        
        // Descriptions must be set per-level
        public Dictionary<int, string> descriptions = new Dictionary<int, string>();

        // Leveling and cost
        public int initialLevelCost;
        public int levelCostMultiplier = 2;
        public int maxLevel = 3;

        public bool unlockedWithLevel;
        public CharLevelManager.SkillTypes unlockedBySkill;
        public int unlockedEveryLevel = 3;

        public LicenceManager.LicenceTypes type;

        /* NOTES
        public int currentLevel;
        public bool isUnlocked;
        public bool unlockedWithLevel;
        public CharLevelManager.SkillTypes unlockedBySkill;
        public int unlockedEveryLevel = 3;
        public bool hasBeenSeenBefore;
        public int sortingNumber;
        */

        /// <summary>Sets the description shown on the license purchase page and in the player details of the journal.</summary>
        /// <param name="level">What level of the license this is a description for.</param>
        /// <param name="description">What description should be shown for that level.</param>
        public void SetDescription(int level, string description) { descriptions[level] = description; }

        /// <summary>Sets a requirement that your license can only be purchased when the player has a minimum level in a particular skill.</summary>
        /// <param name="skill">What skill the license will be linked with. You can only link to one skill per license.</param>
        /// <param name="skillLevelsPerLicenseLevel">How many levels in the given skill the player must gain before unlocking the next level of this license.</param>
        public void ConnectToSkill(CharLevelManager.SkillTypes skill, int skillLevelsPerLicenseLevel) {
            DinkumLicense.unlockedWithLevel = true;
            DinkumLicense.unlockedBySkill = skill;
            DinkumLicense.unlockedEveryLevel = skillLevelsPerLicenseLevel;
            DinkumLicense.isUnlocked = true;
        }
        
    }

}
