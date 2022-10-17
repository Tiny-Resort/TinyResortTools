using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Bootstrap;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Version = SemanticVersioning.Version;

namespace TinyResort {

    internal class TRConflictingPlugins {

        internal static Dictionary<string, PluginInfo> pluginInfos;
        internal static List<ConflictingMods> conflictingMods = new List<ConflictingMods>();

        private static TRButton modsWindowButton;
        private static GameObject ConflictingModsWindow;

        private static GameObject creditsWindow;
        private static RectTransform updateButtonGrid;
        private static GameObject menuText;

        private static TRButton updateButton;

        private static float scrollPosition;
        private static float scrollMaxPosition;

        internal static void Initialize() {
            // Create an update button to work with
            updateButton = TRInterface.CreateButton(ButtonTypes.MainMenu, null, "");
            updateButton.windowAnim.openDelay = 0;
            Object.Destroy(updateButton.buttonAnim);
            updateButton.textMesh.fontSize = 12;
            updateButton.textMesh.rectTransform.sizeDelta = new Vector2(500, 50);
            updateButton.textMesh.lineSpacing = -20;
            
            LoadPluginsAndCheckConflicts();
        }
        
        // Set the plugins that are sent by the mod developers to be conflicting
        internal static void AddConflictingPlugin(PluginInfo currentMod, string pluginName) {
            var foundMod = conflictingMods.Find(i => i.newMod == currentMod);
            if (foundMod != null) { foundMod.conflictingModsStringList.Add(pluginName); }
            else {
                var newMod = new ConflictingMods();
                newMod.newMod = currentMod;
                newMod.conflictingModsList = new List<PluginInfo>();
                newMod.conflictingModsStringList = new List<string>();
                newMod.conflictingModsStringList.Add(pluginName);
                conflictingMods.Add(newMod);
            }
        } 
        
        // Gets all loaded plugins
        internal static void LoadPluginsAndCheckConflicts() {
            pluginInfos = UnityChainloader.Instance.Plugins;

            foreach (var data in conflictingMods) {
                foreach (var modString in data.conflictingModsStringList) {
                    PluginInfo oldModInfo;
                    if (pluginInfos.TryGetValue(modString, out oldModInfo)) { data.conflictingModsList.Add(oldModInfo); }
                }
            }
        }
        
        public static void Update() {
            // Creates the mods update checker window and button to open window if they aren't created yet
            if (WorldManager.manageWorld && !creditsWindow) { CreateAndShowConflictsWindow(); }

            if (ConflictingModsWindow && ConflictingModsWindow.activeInHierarchy) {
                scrollPosition = Mathf.Clamp(scrollPosition - Input.mouseScrollDelta.y, 0, scrollMaxPosition);
                updateButtonGrid.anchoredPosition = new Vector2(0, scrollPosition * 58);
            }
        }
        
        internal static void ShowConflictWindow() {
            if (ConflictingModsWindow) {
                ConflictingModsWindow.gameObject.SetActive(!ConflictingModsWindow.gameObject.activeSelf);
                PopulateConflictList();
            }
        }

        private static void PopulateConflictList() {
            scrollPosition = 0;
            scrollMaxPosition = Mathf.Max(conflictingMods.Count - 6, 0);

            foreach (var plugin in conflictingMods) {
                plugin.modsToShow = $"{plugin.newMod.Metadata.GUID}:\n";
                
                foreach (var mod in plugin.conflictingModsList) { plugin.modsToShow += $"\t<color=#ff7226ff>{mod.Metadata.Name} | {mod.Metadata.GUID}</color>\n"; }
                TRTools.Log(plugin.modsToShow);

                if (plugin.conflictButton != null) continue;

                // Create a button for each mod, indicating if it has an update available with link to mod page on nexus
                plugin.conflictButton = updateButton.Copy(updateButtonGrid.transform, plugin.modsToShow);
            }
        }

        private static void CreateAndShowConflictsWindow() {
            #region Create Window For Showing Conflicts
            if (!creditsWindow) {

                creditsWindow = GameObject.Find("MapCanvas/MenuScreen/Credits");

                if (creditsWindow) {

                    // Create and setup a window for displaying update buttons for mods
                    ConflictingModsWindow = Object.Instantiate(creditsWindow, creditsWindow.transform.parent);
                    ConflictingModsWindow.name = "Conflicting Mods Window";

                    // Add the Dinkum Mods logo at the top of the updater window
                    var modLogo = ConflictingModsWindow.transform.GetChild(0).GetChild(7).GetComponent<Image>();
                    modLogo.rectTransform.anchoredPosition += new Vector2(0, -30);
                    modLogo.rectTransform.sizeDelta = new Vector2(modLogo.rectTransform.sizeDelta.x, 250);
                    modLogo.sprite = TRInterface.ModLogo;

                    // Destroy all unused children
                    Object.Destroy(ConflictingModsWindow.transform.GetChild(0).GetChild(2).gameObject); // Title  
                    Object.Destroy(ConflictingModsWindow.transform.GetChild(0).GetChild(3).gameObject); // Music
                    Object.Destroy(ConflictingModsWindow.transform.GetChild(0).GetChild(4).gameObject); // VoicesBy
                    Object.Destroy(ConflictingModsWindow.transform.GetChild(0).GetChild(5).gameObject); // Special Thanks
                    Object.Destroy(ConflictingModsWindow.transform.GetChild(0).GetChild(6).gameObject); // Acknowledgements
                    Object.Destroy(ConflictingModsWindow.transform.GetChild(0).GetChild(8).gameObject); // Additional Dialogue

                    var scrollArea = new GameObject("Conflicting Mods Buttons Scroll Area");
                    scrollArea.transform.SetParent(ConflictingModsWindow.transform.GetChild(0));
                    scrollArea.transform.SetAsLastSibling();
                    var scrollAreaImage = scrollArea.AddComponent<Image>();
                    scrollAreaImage.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    scrollAreaImage.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    scrollAreaImage.rectTransform.pivot = new Vector2(0.5f, 1f);
                    scrollAreaImage.rectTransform.sizeDelta = new Vector2(550, 340);
                    scrollAreaImage.rectTransform.anchoredPosition = new Vector2(0, -155f);
                    scrollAreaImage.color = Color.red;
                    scrollAreaImage.rectTransform.localScale = Vector3.one;
                    scrollArea.AddComponent<Mask>().showMaskGraphic = false;

                    // Create a UI grid for the update buttons to go in
                    var updateButtonObj = new GameObject("Conflicting Mods Grid");
                    updateButtonObj.transform.SetParent(scrollArea.transform);
                    updateButtonObj.transform.SetAsLastSibling();
                    var gridLayoutGroup = updateButtonObj.AddComponent<GridLayoutGroup>();
                    gridLayoutGroup.cellSize = new Vector2(500, 50);
                    gridLayoutGroup.spacing = new Vector2(8, 8);
                    gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                    gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    gridLayoutGroup.constraintCount = 1;
                    updateButtonGrid = updateButtonObj.GetComponent<RectTransform>();
                    updateButtonGrid.pivot = new Vector2(0.5f, 1);
                    updateButtonGrid.anchorMax = new Vector2(0.5f, 1);
                    updateButtonGrid.anchorMin = new Vector2(0.5f, 1);
                    updateButtonGrid.localScale = Vector3.one;
                    updateButtonGrid.anchoredPosition = new Vector2(0, 0);
                }
            }
            
            #endregion
            
            var checkIfAnyConflictsExist = conflictingMods.Find(i => i.conflictingModsList.Count > 0);
            if (checkIfAnyConflictsExist != null) ShowConflictWindow();
        }

        internal static bool PlayingWithConflicts(PluginInfo currentMod) {
            var foundMod = conflictingMods.Find(i => i.newMod == currentMod && i.conflictingModsList.Count > 0);
            TRTools.Log($"Checking if I found a mod: {foundMod}");
            if (foundMod != null) return true;
            return false;
        }
    }

    // One class is made per mod that runs `AddConflictingPlugin`
    internal class ConflictingMods {
        internal PluginInfo newMod;
        internal List<PluginInfo> conflictingModsList;
        internal List<string> conflictingModsStringList;
        internal string modsToShow;
        internal TRButton conflictButton;
    }
}
