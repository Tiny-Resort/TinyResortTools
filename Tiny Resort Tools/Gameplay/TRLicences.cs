using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    /// <summary>A framework for creating custom licences and using them to gate your mod's features behind player progression.</summary>
    public static class TRLicences {

        private static TRModData Data;
        private static int LicenceTypesCount = Enum.GetNames(typeof(LicenceManager.LicenceTypes)).Length;
        private static List<TRCustomLicence> CustomLicences = new List<TRCustomLicence>();
        private static Dictionary<string, TRCustomLicence> CustomLicencesDict = new Dictionary<string, TRCustomLicence>();
        private static Sprite defaultLicenceSprite;
        
        internal static void Initialize() {

            TRTools.QuickPatch(typeof(LicenceManager), "Start", typeof(TRLicences), "StartPatch");
            TRTools.QuickPatch(typeof(LicenceManager), "getLicenceName", typeof(TRLicences), "getLicenceNamePatch");
            TRTools.QuickPatch(typeof(LicenceManager), "getLicenceLevelDescription", typeof(TRLicences), "getLicenceLevelDescriptionPatch");
            TRTools.QuickPatch(typeof(Licence), "getNextLevelPrice", typeof(TRLicences), "getNextLevelPricePrefix");
            TRTools.QuickPatch(typeof(Licence), "canAffordNextLevel", typeof(TRLicences), "canAffordNextlevelPrefix");
            TRTools.QuickPatch(typeof(Licence), "getCurrentMaxLevel", typeof(TRLicences), "getCurrentMaxLevelPrefix");
            TRTools.QuickPatch(typeof(LicenceButton), "fillButton", typeof(TRLicences), null, "fillButtonPostfix");

            LeadPlugin.plugin.AddCommand("unlock_licence", "Unlocks the specified licence at no cost. Use the list_licences command to get the licence names.", UnlockLicence, "LicenceName", "Level");
            LeadPlugin.plugin.AddCommand("list_licences", "Lists all custom licences added by any mods.", ListLicences);

            Data = TRData.Subscribe("TR.CustomLicences");
            TRData.cleanDataEvent += UnloadLicences;
            TRData.preSaveEvent += SaveLicenceData;
            TRData.postLoadEvent += LoadLicenceData;
            TRData.injectDataEvent += LoadLicences;

            defaultLicenceSprite = TRAssets.LoadTextureFromAssetBundle(TRAssets.LoadAssetBundleFromDLL("licenceimages"), "default_licence", Vector2.one*0.5f);
        }

        internal static string ListLicences(string[] args) {
            if (CustomLicences.Count <= 0) { return "None of the installed mods have a custom licence."; }
            var str = "\nThe following custom licences exist:\n";
            foreach (var li in CustomLicences) { str += li.uniqueID + " (" + li.title + ")" + "\n"; }
            return str;
        }
 
        internal static string UnlockLicence(string[] args) {
            if (args.Length <= 0) { return "<color=red>No licence specified.</color> " + ListLicences(null); }
            foreach (var li in CustomLicences) {
                if ((li.uniqueID).ToLower() == args[0]) {
                    li.info.currentLevel = li.info.maxLevel;
                    return $"Unlocking all levels of licence \"{li.title}\" (ID: {li.uniqueID})";
                }
            }
            return "<color=red>Could not find a licence with the specified ID.</color>";
        }

        internal static TRCustomLicence AddLicence(TRPlugin plugin, int licenceID, string licenceName, int maxLevel = 1) {
            
            var nexusID = plugin.nexusID.Value == -1 ? plugin.plugin.Info.Metadata.Name.Replace(" ", "_").Replace(".", "_") : plugin.nexusID.Value.ToString(); 

            var NewLicence = new TRCustomLicence {
                uniqueID = nexusID + "." + licenceID,
                info = new Licence(),
                licenceIcon = defaultLicenceSprite,
                title = licenceName,
                maxLevel = maxLevel,
                licenceIndex = LicenceTypesCount + CustomLicences.Count
            };

            for (var i = 1; i <= maxLevel; i++) { NewLicence.SetLevelInfo(i, "This is a placeholder description for level " + i + ".", 500 * i); }

            NewLicence.SetInfo();
            CustomLicences.Add(NewLicence);
            CustomLicencesDict[NewLicence.uniqueID] = NewLicence;
            return NewLicence;
            
        }

        #region Patches

        // Adds custom licences to the game licences
        internal static bool StartPatch(LicenceManager __instance) {

            // Gets original arrays of licence data
            __instance.allLicences = new Licence[LicenceTypesCount + CustomLicences.Count];
            var licenceIcons = new List<Sprite>(__instance.licenceIcons);
            var licenceColors = new List<Color>(__instance.licenceColours);

            // Adds custom licences
            for (var i = 0; i < CustomLicences.Count; i++) {
                licenceIcons.Add(CustomLicences[i].licenceIcon);
                licenceColors.Insert(LicenceTypesCount + i - 1, CustomLicences[i].color); 
            }
            
            // Combines list of custom and standard licences
            __instance.licenceIcons = licenceIcons.ToArray();
            __instance.licenceColours = licenceColors.ToArray();

            // Gets references to the buttn lists
            var licenceButtons = (List<LicenceButton>)AccessTools.Field(typeof(LicenceManager), "licenceButtons").GetValue(__instance);
            var journalButtons = (List<LicenceButton>)AccessTools.Field(typeof(LicenceManager), "journalButtons").GetValue(__instance);
            
            // Fills in information for each licence
            for (int i = 1; i < __instance.allLicences.Length; i++) {
                
                if (i >= LicenceTypesCount) { __instance.allLicences[i] = CustomLicences[i - LicenceTypesCount].info; } 
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
            
            // Initializes certain values for licences
            __instance.setLicenceLevelsAndPrice();
            __instance.licenceIcon.preserveAspect = true;
            GiftedItemWindow.gifted.itemIcon.preserveAspect = true;
            
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

        // If the license is NOT at max level but can't be leveled up any higher, then set description
        // to say that you need to level up skills
        [HarmonyPostfix]
        internal static void fillButtonPostfix(LicenceButton __instance) {
            if (__instance.myLicenceId < LicenceTypesCount) return;
            var currentLevel = LicenceManager.manage.allLicences[__instance.myLicenceId].getCurrentLevel();
            if (currentLevel == LicenceManager.manage.allLicences[__instance.myLicenceId].getCurrentMaxLevel() && 
                currentLevel != LicenceManager.manage.allLicences[__instance.myLicenceId].getMaxLevel())
                __instance.licenceDesc.text = "Level up your skills to unlock further levels";
        }

        // Allows for having multiple skill requirements
        [HarmonyPrefix]
        internal static bool getCurrentMaxLevelPrefix(Licence __instance, ref int __result) {
            
            if ((int) __instance.type >= LicenceTypesCount) {
                
                var index = (int)__instance.type - LicenceTypesCount;
                UpdateLockState(CustomLicences[index]);

                for (int i = 1; i <= __instance.maxLevel; i++) {
                    if (CustomLicences[index].skillRequirements.TryGetValue(i, out var reqs)) {
                        foreach (var req in reqs) {
                            if (CharLevelManager.manage.currentLevels[(int)req.Key] < req.Value) {
                                __result = i - 1;
                                if (__result == 0) { __instance.isUnlocked = false; }
                                return false;
                            }
                        }
                    }
                }
                
                __result = __instance.maxLevel;
                return false;
                
            }
            return true;
        }

        // Allows for having multiple skill requirements
        [HarmonyPrefix]
        internal static bool canAffordNextlevelPrefix(Licence __instance, ref bool __result) {
            if ((int) __instance.type >= LicenceTypesCount) {
                var index = (int)__instance.type - LicenceTypesCount;
                __result = PermitPointsManager.manage.checkIfCanAfford(__instance.getNextLevelPrice());
                if (CustomLicences[index].skillRequirements.TryGetValue(__instance.currentLevel + 1, out var reqs)) {
                    foreach (var req in reqs) { if (CharLevelManager.manage.currentLevels[(int)req.Key] < req.Value) __result = false; }
                }
                return false;
            }
            return true;
        }

        // Patches the function that gets licence level cost
        [HarmonyPrefix]
        internal static bool getNextLevelPricePrefix(Licence __instance, ref int __result) {
            if ((int) __instance.type >= LicenceTypesCount) {
                var licence = CustomLicences[(int) __instance.type - LicenceTypesCount];
                if (!licence.costs.TryGetValue(licence.level + 1, out var val)) return true;
                __result = val;
                return false;
            }
            return true;
        }

        // Patches the function that gets a licence name so that it can understand custom licences
        [HarmonyPrefix]
        internal static bool getLicenceNamePatch(LicenceManager __instance, ref string __result, LicenceManager.LicenceTypes type) {
            if ((int)type >= LicenceTypesCount && (int)type - LicenceTypesCount < CustomLicences.Count) {
                __result = CustomLicences[(int) type - LicenceTypesCount].title;
                return false;
            }
            return true;
        }

        // If this is a custom licence, return our own descriptions
        [HarmonyPrefix]
        internal static bool getLicenceLevelDescriptionPatch(LicenceManager __instance, ref string __result, ref LicenceManager.LicenceTypes type, ref int level) {
            if ((int)type >= LicenceTypesCount) {
                var licence = CustomLicences[(int)type - LicenceTypesCount];
                if (!licence.descriptions.TryGetValue(level, out var desc)) {
                    if (level > 0 && level < licence.maxLevel) { TRTools.Log("Custom Licence " + licence.title + " has no description for level " + level); }
                    __result = "";
                } else { __result = desc; }
                return false;
            }
            return true;

        }

        private static void UpdateLockState(TRCustomLicence licence) {

            // By default, custom licences should be unlocked
            licence.info.isUnlocked = true;
            if (licence.info.currentLevel > 0) return;

            // If the licence isn't purchased yet and has level 1 prerequisites, check if the player meets them
            // If the player doesn't meet them, then lock this licence so it doesn't show up for purchase
            if (licence.skillRequirements.TryGetValue(1, out var skillReqs)) {
                foreach (var skillReq in skillReqs) {
                    if (CharLevelManager.manage.currentLevels[(int)skillReq.Key] < skillReq.Value) {
                        licence.info.isUnlocked = false;
                        break;
                    }
                }
            }

            // Adds ability to keep licence locked until a custom licence has been purchased
            foreach (var customReq in licence.prereqsCustom) {
                if (!CustomLicencesDict.ContainsKey(customReq.Key)) continue;
                if (CustomLicencesDict[customReq.Key].level < customReq.Value) {
                    licence.info.isUnlocked = false;
                    break;
                }
            }

            // Adds ability to keep licence locked until a vanilla licence has been purchased
            foreach (var vanillaReq in licence.prereqsVanilla) {
                if (LicenceManager.manage.allLicences.Length <= (int)vanillaReq.Key) continue;
                if (LicenceManager.manage.allLicences[(int)vanillaReq.Key].currentLevel < vanillaReq.Value) {
                    licence.info.isUnlocked = false;
                    break;
                }
            }
        }
        
        #endregion
        
        #region Saving & Loading Data

        // Makes sure the only licences being saved to the slot are base game onesinternal
        internal static void UnloadLicences() {
            var currentLicences = LicenceManager.manage.allLicences;
            LicenceManager.manage.allLicences = new Licence[LicenceTypesCount];
            for (var i = 0; i < LicenceManager.manage.allLicences.Length; i++) {
                LicenceManager.manage.allLicences[i] = currentLicences[i];
            }
        }

        // Loads in the custom licences and their save data
        internal static void LoadLicences() {
            var currentLicences = LicenceManager.manage.allLicences;
            LicenceManager.manage.allLicences = new Licence[LicenceTypesCount + CustomLicences.Count];
            for (var i = 0; i < LicenceManager.manage.allLicences.Length; i++) {
                if (i < LicenceTypesCount) { LicenceManager.manage.allLicences[i] = currentLicences[i]; }
                else { LicenceManager.manage.allLicences[i] = CustomLicences[i - LicenceTypesCount].info; } 
            }
        }

        internal static void SaveLicenceData() {
            for (var i = 0; i < CustomLicences.Count; i++) {
                Data.SetValue(CustomLicences[i].uniqueID, CustomLicences[i].info.currentLevel);
            }
        }

        internal static void LoadLicenceData() {
            for (var i = 0; i < CustomLicences.Count; i++) {
                var val = Data.GetValue(CustomLicences[i].uniqueID);
                if (val == null) { CustomLicences[i].info.currentLevel = 0; }
                else { CustomLicences[i].info.currentLevel = (int) val; }
                CustomLicences[i].SetInfo();
            }
        }
        
        #endregion

    }

    /// <summary>Information and functions related to your custom licence.</summary>
    [Serializable]
    public class TRCustomLicence {

        /// <summary>The player's currently unlocked level for this licence. If 0, the player has not unlocked this licence.</summary>
        public int level => info.currentLevel;
        internal Licence info;

        internal string uniqueID;
        internal int licenceIndex;
        
        // Appearance
        internal string title;
        internal Color color = new Color(195f / 255f, 135f / 255f, 112f / 255f, 1);
        internal Sprite licenceIcon;
        
        // Descriptions must be set per-level
        internal Dictionary<int, string> descriptions = new Dictionary<int, string>();
        internal Dictionary<int, int> costs = new Dictionary<int, int>();
        internal Dictionary<int, Dictionary<CharLevelManager.SkillTypes, int>> skillRequirements = new Dictionary<int, Dictionary<CharLevelManager.SkillTypes, int>>();
        internal Dictionary<string, int> prereqsCustom = new Dictionary<string, int>();
        internal Dictionary<LicenceManager.LicenceTypes, int> prereqsVanilla = new Dictionary<LicenceManager.LicenceTypes, int>();
        internal int maxLevel = 3;

        /// <summary>Sets the color of the banner for the licence. Keep in mind this will not change the color of the licence icon.</summary>
        public void SetColor(Color col) { color = col; }

        /// <summary>Sets the icon used to represent the licence.</summary>
        /// <param name="relativePath">The path to the image file (including file name and extension) you want to use as an icon for the licence, relative to the BepinEX plugins folder.</param>
        public void SetIcon(string relativePath) { licenceIcon = TRAssets.LoadSprite(relativePath, Vector2.one * 0.5f); }

        /// <summary>Sets the description and cost for a specific level of the licence.</summary>
        /// <param name="setLevel">What level of the licence is being changed.</param>
        /// <param name="description">What description should be shown for that level.</param>
        /// <param name="cost">How many permit points it will cost to buy this level.</param>
        public void SetLevelInfo(int setLevel, string description, int cost) {
            descriptions[setLevel] = description;
            costs[setLevel] = cost;
        }

        /// <summary>Makes a specific level of the licence require a minimum level of a particular skill.</summary>
        /// <param name="licenceLevel">The level of the custom licence for which a prerequisite is being added.</param>
        /// <param name="skill">What skill the player will need to level up in order to buy this level of the licence.</param>
        /// <param name="skillLevelRequirement">What level the needs to be at or higher than in order to buy this level of the licence.</param>
        public void AddSkillRequirement(int licenceLevel, CharLevelManager.SkillTypes skill, int skillLevelRequirement) {
            if (!skillRequirements.ContainsKey(licenceLevel)) { skillRequirements[licenceLevel] = new Dictionary<CharLevelManager.SkillTypes, int>(); }
            skillRequirements[licenceLevel][skill] = skillLevelRequirement;
        }

        /// <summary>Makes this licence only unlock for purchase if another licence has reached a minimum level.</summary>
        /// <param name="requiredLicence">The licence that must be leveled up in order to unlock your licence.</param>
        /// <param name="minimumLevel">The minimum level the required licence must be to unlock your licence.</param>
        public void AddPrerequisite(TRCustomLicence requiredLicence, int minimumLevel = 1) { prereqsCustom[requiredLicence.uniqueID] = minimumLevel; }

        /// <summary>Makes this licence only unlock for purchase if another licence has reached a minimum level.</summary>
        /// <param name="requiredLicence">The licence that must be leveled up in order to unlock your licence.</param>
        /// <param name="minimumLevel">The minimum level the required licence must be to unlock your licence.</param>
        public void AddPrerequisite(LicenceManager.LicenceTypes requiredLicence, int minimumLevel = 1) { prereqsVanilla[requiredLicence] = minimumLevel; }

        internal void SetInfo() {
            info.type = (LicenceManager.LicenceTypes) licenceIndex;
            info.maxLevel = maxLevel;
            info.unlockedWithLevel = false;
        }
        
    }

}
