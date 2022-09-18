using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Bootstrap;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Version = SemanticVersioning.Version;

namespace TinyResort {

    internal class TRModUpdates {

        private static List<DinkumPlugin> loadedPlugins = new List<DinkumPlugin>();

        public static List<string> downloadURI = new List<string>();
        
        public static ConfigEntry<bool> createEmptyConfigFiles;
        private static bool finishedChecking = false;
        internal static bool lookForMapCanvas = true;
        internal static GameObject ModLoaderButton;
        internal static GameObject ModLoaderWindow;
        internal static bool ModWindowOpen;
        public static GameObject button;
        public static GameObject ModGrid;

        private static string responseString;

        private static HttpClient client = new HttpClient();
        private static string configDirectory = Application.dataPath.Replace("Dinkum_Data", "BepInEx/config/");

        internal static void Initialize() { DoDownload(); } //LeadPlugin.instance.StartCoroutine(CheckPlugins()); }

        internal static bool CreateModUpdateButton() {
            //TRTools.Log("Looking for Credits Button to Instantiate");
            button = GameObject.Find("MapCanvas/MenuScreen/CornerStuff/CreditsButton");
            var window = GameObject.Find("MapCanvas/MenuScreen/Credits");
            if (button && window) {

                ModLoaderButton = GameObject.Instantiate(button, button.transform.parent);
                ModLoaderButton.name = "ModLoaderButton";

                //TRTools.Log("Instantiated the Credits Button and Window");

                var Im = ModLoaderButton.GetComponent<Image>();
                Im.rectTransform.anchoredPosition += new Vector2(0, 25);

                var MLBText = ModLoaderButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                MLBText.text = "Mods";

                var ModWindow = ModLoaderButton.GetComponent<InvButton>();

                // We have to set this to a new UnityEvent. Setting to Null breaks it and we can't remove the original events. 
                ModWindow.onButtonPress = new UnityEvent();
                ModWindow.onButtonPress.AddListener(ToggleModWindow);

                ModLoaderWindow = GameObject.Instantiate(window, window.transform.parent);
                ModLoaderWindow.name = "ModLoaderWindow";

                var modLogo = ModLoaderWindow.transform.GetChild(0).GetChild(7).GetComponent<Image>();
                modLogo.rectTransform.anchoredPosition += new Vector2(0, -20);
                modLogo.rectTransform.sizeDelta = new Vector2(modLogo.rectTransform.sizeDelta.x, 250);
                modLogo.sprite = TRAssets.ImportSprite("TR Tools/textures/mod_logo.png", Vector2.one * 0.5f);

                // Destroy all unused children
                GameObject.Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(2).gameObject); // Title 
                GameObject.Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(3).gameObject); // Music
                GameObject.Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(4).gameObject); // VoicesBy
                GameObject.Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(5).gameObject); // Special Thanks
                GameObject.Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(6).gameObject); // Acknowledgements

                //Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(7).gameObject); // Dinkum Logo
                GameObject.Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(8).gameObject); // Additional Dialogue

                //Destroy(ModLoaderWindow.transform.GetChild(0).GetChild(9).gameObject); // GameObject (for closing? - breaks if I Destroy it)

                // TODO: Import Image to replacce Dinkum Logo to be a Mod Logo

                ModGrid = new GameObject();
                ModGrid.transform.SetParent(ModLoaderWindow.transform.GetChild(0));
                ModGrid.transform.SetAsLastSibling();
                var gridLayoutGroup = ModGrid.AddComponent<GridLayoutGroup>();

                gridLayoutGroup.cellSize = new Vector2(250, 70);
                gridLayoutGroup.spacing = new Vector2(20, 10);
                gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = 2;

                var rect = ModGrid.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.localScale = Vector3.one;
                rect.anchoredPosition = new Vector2(0, -155);

                return true;
            }
            return false;
        }

        internal static void openWebpage(int id) { Application.OpenURL(string.Format("https://www.nexusmods.com/dinkum/mods/{0}/?tab=files", id)); }

        internal static void PopulateModList() {

            foreach (var mod in loadedPlugins) {

                if (mod.updateButton != null) continue;

                mod.updateButton = GameObject.Instantiate(button, ModGrid.transform);
                var ModButtonText = mod.updateButton.GetComponentInChildren<TextMeshProUGUI>();
                ModButtonText.fontStyle = FontStyles.Normal;
                ModButtonText.text = mod.outOfDate
                                         ? $"{mod.name}\nUPDATE AVAILABLE\n({mod.modVersion} -> {mod.nexusVersion})"
                                         : $"{mod.name}\nUp to Date\n({mod.modVersion})";

                var nexusLink = mod.updateButton.GetComponent<InvButton>();
                nexusLink.onButtonPress = new UnityEvent();
                nexusLink.onButtonPress.AddListener(delegate { openWebpage(mod.id); });
                ModButtonText.rectTransform.sizeDelta = new Vector2(225, 50);

            }

            loadedPlugins = loadedPlugins.OrderByDescending(i => i.outOfDate).ThenBy(i => i.name).ToList();

            for (var i = 0; i < loadedPlugins.Count; i++) { loadedPlugins[i].updateButton.transform.SetSiblingIndex(i); }

        }

        internal static void ToggleModWindow() {
            ModLoaderWindow.gameObject.SetActive(!ModWindowOpen);
            PopulateModList();
        }

        /*private static IEnumerator CheckPlugins() {

            // Gets existing plugins
            var pluginInfos = UnityChainloader.Instance.Plugins.Values;
            if (pluginInfos == null) yield break;

            foreach (var kvp in pluginInfos) {

                #region Get plugin info

                // Get Plugin Info
                Version modVersion = kvp.Metadata.Version;
                string plugName = kvp.Metadata.Name;
                string guid = kvp.Metadata.GUID;

                // Find the config file, create one if its not there
                string cfgFile = Path.Combine(Directory.GetParent(Path.GetDirectoryName(typeof(BepInProcess).Assembly.Location)).FullName, "config", guid + ".cfg");
                if (!File.Exists(cfgFile)) {
                    File.Create(cfgFile);
                    continue;
                }

                #endregion

                #region Find nexusID from Config file

                int id = -1;
                string[] cfgLines = File.ReadAllLines(cfgFile);
                foreach (string line in cfgLines) {
                    if (line.Trim().ToLower().StartsWith("nexusid")) {
                        Match match = Regex.Match(line, "[0-9]+");
                        if (match.Success) { id = int.Parse(match.Value); }
                        break;
                    }
                }
                if (id == -1) { continue; }

                toDownload.Add(new DownloadPluginData(plugName, id, modVersion));

                TRTools.Log($"{plugName} {id} current version: {modVersion}");

                #endregion

                UnityWebRequest uwr = UnityWebRequest.Get($"https://www.nexusmods.com/dinkum/mods/{id}");
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success) {
                    TRTools.Log($"Error While Sending: {uwr.error}");
                    continue;
                }
                string[] nexusText = uwr.downloadHandler.text.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                #region Read nexus mod page and look for version number

                bool check = false;
                foreach (string line in nexusText) {
                    if (check && line.Contains("<div class=\"stat\">")) {

                        Match match = Regex.Match(line, "<[^>]+>[^0-9.]*([0-9.]+)[^0-9.]*<[^>]+>");
                        if (!match.Success) { break; }

                        Version nexusVersion = new Version(match.Groups[1].Value);
                        loadedPlugins.Add(new DinkumPlugin(plugName, id, modVersion, nexusVersion, nexusVersion > modVersion));
                        break;
                    }
                    if (line.Contains("<li class=\"stat-version\">")) { check = true; }
                }

                #endregion

                if (ModLoaderWindow != null && ModLoaderWindow.gameObject.activeSelf) { PopulateModList(); }

            }
            finishedChecking = true;
        }*/

        private static async void DoDownload() {

            
            var pluginInfos = UnityChainloader.Instance.Plugins.Values;
            var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 50 }; // setting higher than 1 seems to cause instability if opening menu to soon
            var block = new ActionBlock<PluginInfo>(MyMethodAsync, options);

            foreach (var workLoad in pluginInfos) {
                TRTools.Log($"Posted {workLoad.Metadata.Name}");
                block.Post(workLoad);
            }

            block.Complete();
            await block.Completion;
            
        }


        public static async void MyMethodAsync(PluginInfo data) {

            Stopwatch sw = new Stopwatch();
            sw.Restart();
            Version modVersion = data.Metadata.Version;
            string plugName = data.Metadata.Name;
            string guid = data.Metadata.GUID;

            // Find the config file, create one if its not there
            string cfgFile = configDirectory + guid + ".cfg";
            if (!File.Exists(cfgFile)) { File.Create(cfgFile); }

            #region Find nexusID from Config file

            int id = -1;
            string[] cfgLines = File.ReadAllLines(cfgFile);
            foreach (string line in cfgLines) {
                if (line.Trim().ToLower().StartsWith("nexusid")) {
                    Match match = Regex.Match(line, "[0-9]+");
                    if (match.Success) { id = int.Parse(match.Value); }
                    break;
                }
            }
            if (id != -1) {
                TRTools.Log($"{plugName} {id} current version: {modVersion}");

                #endregion

                // var uwr = await client.GetStringAsync($"https://www.nexusmods.com/dinkum/mods/{id}");

                var uwr = await client.GetStringAsync($"https://www.nexusmods.com/dinkum/mods/{id}");

                string[] nexusText = uwr.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                //string[] nexusText2 = uwr2.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                bool check = false;
                foreach (string line in nexusText) {
                    if (check && line.Contains("<div class=\"stat\">")) {

                        Match match = Regex.Match(line, "<[^>]+>[^0-9.]*([0-9.]+)[^0-9.]*<[^>]+>");
                        if (!match.Success) { break; }

                        Version nexusVersion = new Version(match.Groups[1].Value);
                        loadedPlugins.Add(new DinkumPlugin(plugName, id, modVersion, nexusVersion, nexusVersion > modVersion));
                        break;
                    }
                    if (line.Contains("<li class=\"stat-version\">")) { check = true; }
                }

                TRTools.Log($"Mod Downloaded {data.Metadata.Name}");
                sw.Stop();
                TRTools.Log($"Elapsed Time: {sw.Elapsed}");
                PopulateModList();

            }

        }

    }

    internal class DinkumPlugin {

        public DinkumPlugin(string name, int id, Version modVersion, Version nexusVersion, bool outOfDate) {
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
