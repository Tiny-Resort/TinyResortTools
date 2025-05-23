﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BepInEx.Unity.Bootstrap;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Version = SemanticVersioning.Version;

namespace TinyResort
{
    internal class TRModUpdater {

        public enum PluginUpdateState { UpdateAvailable, UpToDate, NotSetUp }

        private static List<NexusPlugin> loadedPlugins = new();

        private static ConfigEntry<bool> createEmptyConfigFiles;
        private static ConfigEntry<bool> showUpToDateMods;
        private static ConfigEntry<bool> showUnknownNexusID;

        private static TRButton modsWindowButton;
        private static GameObject modsWindow;

        private static GameObject creditsWindow;
        private static RectTransform updateButtonGrid;

        //private static GameObject menuText;

        private static TRButton updateButton;

        private static float scrollPosition;
        private static float scrollMaxPosition;

        internal static List<QIModInfo> QuickItemInfo = new();

        private static readonly string configDirectory = Application.dataPath.Replace("Dinkum_Data", "BepInEx/config/");

        public static void Initialize() {

            TRTools.sceneSetupEvent += CreateUI;

            createEmptyConfigFiles = LeadPlugin.instance.Config.Bind(
                "Mod Management", "CreateEmptyConfigFiles", false,
                "If true and no config file exists for a mod, one will automatically be created for you to add a nexusID setting to manually."
            );

            showUpToDateMods = LeadPlugin.instance.Config.Bind(
                "Mod Management", "ShowUpToDateMods", true,
                "If true, it will show up to date mods in the Mod Update Checker."
            );

            showUnknownNexusID = LeadPlugin.instance.Config.Bind(
                "Mod Management", "ShowUnknownNexusID", true,
                "If true, it will show mods with an unknown nexus id in the Mod Update Checker."
            );

            ScanPlugins();

        }

        public static void CreateUI() {

            creditsWindow = null;
            TRTools.Log($"Creating UI for Mod Update Checker");

            // Create mod update checker button
            // modsButtonParent needs to be connected to MenuItems, which
            // is currently `GetChild(5) 
            var modsButtonParent = OptionsMenu.options.menuParent.transform.GetChild(5);
            modsWindowButton = TRInterface.CreateButton(ButtonTypes.MainMenu, modsButtonParent, "Mods", ToggleModWindow);
            /*
            If we want to return to having the Mods button on the bottom left corner, we can update the
            GetChild and uncomment the rectTransform overrides. 
            // modsButtonParent needs to be connected to CornerStuff, which
            // is currently `GetChild(11)
            var modsButtonParent = OptionsMenu.options.menuParent.transform.GetChild(11);
            modsWindowButton.rectTransform.sizeDelta = new Vector2(146, 44);
            modsWindowButton.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            modsWindowButton.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            modsWindowButton.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            modsWindowButton.rectTransform.anchoredPosition = new Vector2(-1115, -5f);
            */
            
            modsWindowButton.textMesh.alignment = TextAlignmentOptions.Center;
            modsWindowButton.textMesh.fontSize = 20;
            modsWindowButton.transform.SetSiblingIndex(5);
            
            // Create an update button to work with
            updateButton = TRInterface.CreateButton(ButtonTypes.MainMenu, null, "");
            updateButton.windowAnim.openDelay = 0;
            Object.Destroy(updateButton.buttonAnim);
            updateButton.textMesh.fontSize = 12;
            updateButton.textMesh.rectTransform.sizeDelta = new Vector2(500, 50);
            updateButton.textMesh.lineSpacing = -20;
        }

        public static void Update() {

            // Creates the mods update checker window and button to open window if they aren't created yet
            if (WorldManager.Instance && !creditsWindow) CreateModUpdateButton();

            if (modsWindow && modsWindow.activeInHierarchy) {
                scrollPosition = Mathf.Clamp(scrollPosition - Input.mouseScrollDelta.y, 0, scrollMaxPosition);
                updateButtonGrid.anchoredPosition = new Vector2(0, scrollPosition * 58);
            }

        }

        internal static void CreateModUpdateButton() {

            if (!creditsWindow) {

                creditsWindow = GameObject.Find("MapCanvas/MenuScreen/Credits");

                if (creditsWindow) {
                    // Create and setup a window for displaying update buttons for mods
                    modsWindow = Object.Instantiate(creditsWindow, creditsWindow.transform.parent);
                    modsWindow.name = "Mod Loader Window";

                    modsWindow.transform.GetChild(0).name = "Mod Window Internals";

                    // Add the Dinkum Mods logo at the top of the updater window
                    var modLogo = modsWindow.transform.GetChild(0).GetChild(7).GetComponent<Image>();
                    modLogo.rectTransform.anchoredPosition += new Vector2(0, -30);
                    modLogo.rectTransform.sizeDelta = new Vector2(modLogo.rectTransform.sizeDelta.x, 250);
                    modLogo.sprite = TRInterface.ModLogo;
                    modLogo.name = "Dinkum Mods Logo";

                    // Add credits at the bottom of the dinkum mods window
                    var modCredits = Object.Instantiate(
                        modsWindow.transform.GetChild(0).GetChild(2), modsWindow.transform.GetChild(0)
                    );
                    Object.Destroy(modCredits.transform.GetChild(0).gameObject);
                    modCredits.transform.SetAsLastSibling();
                    var modCreditsText = modCredits.GetComponent<TextMeshProUGUI>();
                    modCreditsText.name = "Mod Credits";
                    modCreditsText.text =
                        "This Update Checker and the DINKUM MODS logo are both unofficial.\nLogo created by Duvinn, Paris, and Row.";
                    modCreditsText.fontStyle = FontStyles.Italic;
                    modCreditsText.rectTransform.anchoredPosition = new Vector2(0, 25);
                    modCreditsText.rectTransform.anchorMax = new Vector2(0.5f, 0);
                    modCreditsText.rectTransform.anchorMin = new Vector2(0.5f, 0);
                    modCreditsText.rectTransform.pivot = new Vector2(0.5f, 0);
                    modCreditsText.rectTransform.sizeDelta = new Vector2(500, 35);
                    modCreditsText.fontSize = 11;
                    
                    // I2.Loc.Localize automatically translates the text and re-replaces the text. We don't want that.
                    Object.Destroy(modCreditsText.GetComponent<I2.Loc.Localize>());
                    
                    // Destroy all unused children
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(2).gameObject); // Title  
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(3).gameObject); // Music
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(4).gameObject); // VoicesBy
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(5).gameObject); // Special Thanks
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(6).gameObject); // Acknowledgements
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(8).gameObject); // Additional Dialogue
                    Object.Destroy(modsWindow.transform.GetChild(1).gameObject); // License Button (top right)

                    var scrollArea = new GameObject("Mod Update Buttons Scroll Area");
                    scrollArea.transform.SetParent(modsWindow.transform.GetChild(0));
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
                    var updateButtonObj = new GameObject("Mod Update Button Grid");
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

        }

        private static void openWebpage(string url) => Application.OpenURL(url);

        private static void PopulateModList() {

            scrollPosition = 0;
            scrollMaxPosition = Mathf.Max(loadedPlugins.Count - 6, 0);

            foreach (var mod in loadedPlugins) {

                // If a button already exists for this mod, move on
                if (mod.updateButton != null) continue;

                // Create a button for each mod, indicating if it has an update available with link to mod page on nexus
                if (mod.updateState == PluginUpdateState.NotSetUp) {
                    if (!showUnknownNexusID.Value) continue;

                    mod.updateButton = updateButton.Copy(
                        updateButtonGrid.transform,
                        $"<size=15>{mod.name}</size>\n<color=#787877FF>Status Unknown: Missing NexusID</color>",
                        delegate {
                            openWebpage(
                                "https://modding.wiki/en/dinkum/TRTools/ModManager#why-isnt-x-mod-showing-up-in-the-update-checker"
                            );
                        }
                    );
                }

                // Create a button for each mod, indicating if it has an update available with link to mod page on nexus
                else {
                    if (mod.updateState == PluginUpdateState.UpToDate && !showUpToDateMods.Value) continue;

                    mod.updateButton = updateButton.Copy(
                        updateButtonGrid.transform,
                        mod.updateState == PluginUpdateState.UpdateAvailable
                            ? $"<size=15>{mod.name}</size>\n<color=#00ff00ff><b>UPDATE AVAILABLE</b></color> (<color=#ff7226ff>{mod.modVersion}</color> -> <color=#00ff00ff>{mod.nexusVersion}</color>)"
                            : $"<size=15>{mod.name}</size>\n<color=#787877FF>UP TO DATE ({mod.modVersion})</color>",
                        delegate {
                            openWebpage(string.Format("https://www.nexusmods.com/dinkum/mods/{0}/?tab=files", mod.id));
                        }
                    );
                }

            }

            // Organize the mod update buttons with mods that have an update available at the top
            loadedPlugins = loadedPlugins.OrderBy(i => i.updateState).ThenBy(i => i.name).ToList();
            for (var i = 0; i < loadedPlugins.Count; i++) loadedPlugins[i].updateButton.transform.SetSiblingIndex(i);

        }

        internal static void ToggleModWindow() {
            modsWindow.gameObject.SetActive(!modsWindow.gameObject.activeSelf);
            PopulateModList();
        }

        private static void ScanPlugins() {

            // Scan the quickitem version files.
            foreach (var mod in QuickItemInfo) {
                if (mod.nexusID == -1)
                    loadedPlugins.Add(
                        new NexusPlugin(
                            mod.modName, mod.nexusID, new Version(mod.version), null, PluginUpdateState.NotSetUp
                        )
                    );
                LeadPlugin.instance.StartCoroutine(GetNexusInfo(mod.modName, mod.nexusID, new Version(mod.version)));
            }

            // Gets existing plugins
            var pluginInfos = UnityChainloader.Instance.Plugins.Values;

            foreach (var kvp in pluginInfos.Select(kvp => kvp.Metadata)) {

                // Look for the plugin's config file. One none exists, create one if desired
                var cfgFile = Path.Combine(configDirectory, kvp.GUID + ".cfg");
                if (!File.Exists(cfgFile)) {
                    if (createEmptyConfigFiles.Value) File.Create(cfgFile);
                    continue;
                }

                // Search through the config file for a nexusID for this plugin
                var id = -1;
                foreach (var line in File.ReadAllLines(cfgFile)) {

                    // Added a better regex for grabbing the Nexus ID. This one allows for white spaces. Credit: SadGirlRika#8141
                    var lineMatch = Regex.Match(line.Trim().ToLower(), @"^\s*nexus\s*id\s*=\s*(\d+)");
                    if (lineMatch.Success) {
                        if (line.Contains("-")) continue;
                        id = int.Parse(lineMatch.Groups[1].Value);
                        break;
                    }
                }

                // If a nexusID was found for this plugin, then get it's version information from the webpage
                if (id == -1)
                    loadedPlugins.Add(
                        new NexusPlugin(kvp.Name, id, kvp.Version, null, PluginUpdateState.NotSetUp)
                    );
                LeadPlugin.instance.StartCoroutine(GetNexusInfo(kvp.Name, id, kvp.Version));

                //TRTools.Log($"{kvp.Metadata.Name} {id} current version: {kvp.Metadata.Version}");
            }
        }

        private static IEnumerator GetNexusInfo(string plugName, int id, Version modVersion) {

            // Download nexus mod page to get information from it
            var uwr = UnityWebRequest.Get($"https://www.nexusmods.com/dinkum/mods/{id}");
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success) {
                TRTools.LogError($"Error While Sending: {uwr.error}");
                yield break;
            }

            // Search nexus mod page for version number
            // Note: Refactored to lower complexity. 
            var check = false;
            foreach (var line in uwr.downloadHandler.text.Split(
                         new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None
                     )) {
                if (line.Contains("<li class=\"stat-version\">")) check = true;
                if (!check || !line.Contains("<div class=\"stat\">")) continue;

                var match = Regex.Match(line, "<[^>]+>[^0-9.]*([0-9.]+)[^0-9.]*<[^>]+>");
                Version nexusVersion;
                if (!match.Success) break;

                // BepInEx only keeps track of the first three version numbers (Semantic Versioning: https://semver.org/)
                // I take any version found on nexus with longer version and reduce them to the first three numbers. 
                var matchSplit = match.Groups[1].Value.Split('.');
                var matchedVersion = matchSplit.Length > 3
                    ? $"{matchSplit[0]}.{matchSplit[1]}.{matchSplit[2]}"
                    : match.Groups[1].Value;

                nexusVersion = new Version(matchedVersion);
                loadedPlugins.Add(
                    new NexusPlugin(
                        plugName, id, modVersion, nexusVersion,
                        nexusVersion > modVersion ? PluginUpdateState.UpdateAvailable : PluginUpdateState.UpToDate
                    )
                );
                break;

            }

            // If the mods window is open, repopulate the mod list to keep it up to date
            if (modsWindow != null && modsWindow.gameObject.activeSelf) PopulateModList();

        }

        private class NexusPlugin {
            public readonly int id;
            public readonly Version modVersion;

            public readonly string name;
            public readonly Version nexusVersion;
            public readonly PluginUpdateState updateState;
            public TRButton updateButton;

            public NexusPlugin(string name, int id, Version modVersion, Version nexusVersion, PluginUpdateState state) {
                this.name = name;
                this.id = id;
                this.modVersion = modVersion;
                this.nexusVersion = nexusVersion;
                updateState = state;
            }
        }
    }
}
