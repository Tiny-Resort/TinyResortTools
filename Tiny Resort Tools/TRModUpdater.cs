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
    
    public enum GraphicType { MainMenuButton, CreditsWindow, OptionsSlider }

    internal class TRModUpdater {

        private static List<NexusPlugin> loadedPlugins = new List<NexusPlugin>();

        private static ConfigEntry<bool> createEmptyConfigFiles;

        private static GameObject modsButton;
        private static GameObject modsWindow;
        private static GameObject creditsButton;
        private static GameObject creditsWindow;
        private static RectTransform updateButtonGrid;
        private static GameObject menuText;
        
        private static float scrollPosition;
        private static float scrollMaxPosition;
        
        private static string configDirectory = Application.dataPath.Replace("Dinkum_Data", "BepInEx/config/");

        public static void Initialize() {
            
            createEmptyConfigFiles = LeadPlugin.instance.Config.Bind(
                "Mod Management", "Create Empty Config Files", false, 
                "If true and no config file exists for a mod, one will automatically be created for you to add a nexusID setting to manually.");
            ScanPlugins();
            
        }

        public static void Update() {
            
            // Creates the mods update checker window and button to open window if they aren't created yet
            if (WorldManager.manageWorld && (!creditsButton || !creditsWindow)) { CreateModUpdateButton(); }

            if (modsWindow && modsWindow.activeInHierarchy) {
                scrollPosition = Mathf.Clamp(scrollPosition - Input.mouseScrollDelta.y, 0, scrollMaxPosition);
                updateButtonGrid.anchoredPosition = new Vector2(0, scrollPosition * 58);
            }
            
        }

        private static void CreateModUpdateButton() {

            if (!creditsButton) {

                var menuTextObj = GameObject.Find("MapCanvas/MenuScreen/MenuButtons/New Game/Text");
                creditsButton = GameObject.Find("MapCanvas/MenuScreen/CornerStuff/CreditsButton");

                if (creditsButton) {

                    // Create and setup a button for opening the mod window
                    modsButton = Object.Instantiate(creditsButton, creditsButton.transform.parent);
                    modsButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 25);

                    // Takes the text from the normal menu buttons so that it has an outline
                    var child = modsButton.transform.GetChild(0);
                    Object.Destroy(child.gameObject);
                    menuText = Object.Instantiate(menuTextObj, modsButton.transform);
                    var text = menuText.GetComponent<TextMeshProUGUI>();
                    text.alignment = TextAlignmentOptions.Center;
                    text.raycastTarget = false;
                    text.fontSize = 12;
                    text.text = "MODS";
                    modsButton.name = "ModLoaderButton";

                    // Make the button open the mod window when clicked
                    var modsInvButton = modsButton.GetComponent<InvButton>();
                    modsInvButton.onButtonPress = new UnityEvent();
                    modsInvButton.onButtonPress.AddListener(ToggleModWindow);

                }
                
            }
            
            if (!creditsWindow) { 
                
                creditsWindow = GameObject.Find("MapCanvas/MenuScreen/Credits"); 

                if (creditsWindow) {

                    // Create and setup a window for displaying update buttons for mods
                    modsWindow = Object.Instantiate(creditsWindow, creditsWindow.transform.parent);
                    modsWindow.name = "Mod Loader Window";

                    // Add the Dinkum Mods logo at the top of the updater window
                    var modLogo = modsWindow.transform.GetChild(0).GetChild(7).GetComponent<Image>();
                    modLogo.rectTransform.anchoredPosition += new Vector2(0, -30);
                    modLogo.rectTransform.sizeDelta = new Vector2(modLogo.rectTransform.sizeDelta.x, 250);
                    modLogo.sprite = TRAssets.ImportSprite("TR Tools/textures/mod_logo.png", Vector2.one * 0.5f);

                    // Add credits at the bottom of the dinkum mods window
                    var modCredits = Object.Instantiate(modsWindow.transform.GetChild(0).GetChild(2), modsWindow.transform.GetChild(0));
                    Object.Destroy(modCredits.transform.GetChild(0).gameObject);
                    modCredits.transform.SetAsLastSibling();
                    var modCreditsText = modCredits.GetComponent<TextMeshProUGUI>();
                    modCreditsText.text = "This Update Checker and the DINKUM MODS logo are both unofficial.\nLogo created by Duvinn, Paris, and Row.";
                    modCreditsText.fontStyle = FontStyles.Italic;
                    modCreditsText.rectTransform.anchoredPosition = new Vector2(0, 25);
                    modCreditsText.rectTransform.anchorMax = new Vector2(0.5f, 0);
                    modCreditsText.rectTransform.anchorMin = new Vector2(0.5f, 0);
                    modCreditsText.rectTransform.pivot = new Vector2(0.5f, 0);
                    modCreditsText.rectTransform.sizeDelta = new Vector2(500, 35);
                    modCreditsText.fontSize = 11;

                    // Destroy all unused children
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(2).gameObject); // Title  
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(3).gameObject); // Music
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(4).gameObject); // VoicesBy
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(5).gameObject); // Special Thanks
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(6).gameObject); // Acknowledgements
                    Object.Destroy(modsWindow.transform.GetChild(0).GetChild(8).gameObject); // Additional Dialogue

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

        private static void openWebpage(int id) { Application.OpenURL(string.Format("https://www.nexusmods.com/dinkum/mods/{0}/?tab=files", id)); }

        private static void PopulateModList() {

            scrollPosition = 0;
            scrollMaxPosition = Mathf.Max(loadedPlugins.Count - 6, 0);
            
            foreach (var mod in loadedPlugins) {

                // If a button already exists for this mod, move on
                if (mod.updateButton != null) continue;

                // Create a button for each mod, indicating if it has an update available
                mod.updateButton = Object.Instantiate(creditsButton, updateButtonGrid.transform);
                mod.updateButton.GetComponent<WindowAnimator>().openDelay = 0;
                Object.Destroy(mod.updateButton.GetComponent<ButtonAnimation>());
                Object.Destroy(mod.updateButton.transform.GetChild(0).gameObject);
                
                var text = Object.Instantiate(menuText, mod.updateButton.transform).GetComponent<TextMeshProUGUI>();
                text.fontSize = 13;
                text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                text.rectTransform.sizeDelta = new Vector2(500, 50);
                text.rectTransform.anchoredPosition = new Vector2(0, 0);
                text.lineSpacing = -20;
                text.text = mod.outOfDate
                                ? $"<size=16>{mod.name}</size>\n<color=#00ff00ff><b>UPDATE AVAILABLE</b></color> (<color=#ff7226ff>{mod.modVersion}</color> -> <color=#00ff00ff>{mod.nexusVersion}</color>)"
                                : $"<size=16>{mod.name}</size>\n<color=#787877FF>UP TO DATE ({mod.modVersion})</color>";

                // When the button is clicked, open a web browser to that mod's file page
                var nexusLink = mod.updateButton.GetComponent<InvButton>();
                nexusLink.onButtonPress = new UnityEvent();
                nexusLink.onButtonPress.AddListener(delegate { openWebpage(mod.id); });

            }

            // Organize the mod update buttons with mods that have an update available at the top
            loadedPlugins = loadedPlugins.OrderByDescending(i => i.outOfDate).ThenBy(i => i.name).ToList();
            for (var i = 0; i < loadedPlugins.Count; i++) { loadedPlugins[i].updateButton.transform.SetSiblingIndex(i); }

            // TODO
            var obj = TRAssets.ImportBundle("TR Tools/main menu button");
            obj.transform.SetParent(updateButtonGrid.transform);

        }

        internal static void ToggleModWindow() {
            modsWindow.gameObject.SetActive(!modsWindow.gameObject.activeSelf);
            PopulateModList();
        }

        private static void ScanPlugins() {

            // Gets existing plugins
            var pluginInfos = UnityChainloader.Instance.Plugins.Values;
            foreach (var kvp in pluginInfos) {

                // Look for the plugin's config file. One none exists, create one if desired
                string cfgFile = Path.Combine(configDirectory, kvp.Metadata.GUID + ".cfg");
                if (!File.Exists(cfgFile)) {
                    if (createEmptyConfigFiles.Value) { File.Create(cfgFile); }
                    continue;
                }

                // Search through the config file for a nexusID for this plugin
                int id = -1;
                foreach (string line in File.ReadAllLines(cfgFile)) {
                    if (line.Trim().ToLower().StartsWith("nexusid")) {
                        Match match = Regex.Match(line, "[0-9]+");
                        if (match.Success) { id = int.Parse(match.Value); }
                        break;
                    }
                }
                
                // If a nexusID was found for this plugin, then get it's version information from the webpage
                if (id == -1) { continue; }
                LeadPlugin.instance.StartCoroutine(GetNexusInfo(kvp.Metadata.Name, id, kvp.Metadata.Version));

                //TRTools.Log($"{kvp.Metadata.Name} {id} current version: {kvp.Metadata.Version}");

            }
            
        }

        private static IEnumerator GetNexusInfo(string plugName, int id, Version modVersion) {

            // Download nexus mod page to get information from it
            UnityWebRequest uwr = UnityWebRequest.Get($"https://www.nexusmods.com/dinkum/mods/{id}");
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success) {
                TRTools.LogError($"Error While Sending: {uwr.error}");
                yield break;
            }
            
            // Search nexus mod page for version number
            bool check = false;
            foreach (string line in uwr.downloadHandler.text.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None)) {
                if (check && line.Contains("<div class=\"stat\">")) {
                    Match match = Regex.Match(line, "<[^>]+>[^0-9.]*([0-9.]+)[^0-9.]*<[^>]+>");
                    if (!match.Success) { break; }
                    Version nexusVersion = new Version(match.Groups[1].Value);
                    loadedPlugins.Add(new NexusPlugin(plugName, id, modVersion, nexusVersion, nexusVersion > modVersion));
                    break;
                }
                if (line.Contains("<li class=\"stat-version\">")) { check = true; }
            }

            // If the mods window is open, repopulate the mod list to keep it up to date
            if (modsWindow != null && modsWindow.gameObject.activeSelf) { PopulateModList(); }
            
        }

        private class NexusPlugin {

            public NexusPlugin(string name, int id, Version modVersion, Version nexusVersion, bool outOfDate) {
                this.name = name;
                this.id = id;
                this.modVersion = modVersion;
                this.nexusVersion = nexusVersion;
                this.outOfDate = outOfDate;
            }

            public string name;
            public int id;
            public Version modVersion;
            public Version nexusVersion;
            public bool outOfDate;
            public GameObject updateButton;

        }
        
    }

}
