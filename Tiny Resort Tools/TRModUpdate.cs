using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Bootstrap;
using UnityEngine;
using UnityEngine.Networking;
using Version = SemanticVersioning.Version;

namespace TinyResort {

    public class TRModUpdates : BaseUnityPlugin {

        public static ConfigEntry<bool> showAllManagedMods;
        private static bool finishedChecking = false;

        private static List<DinkumPlugin> pluginsOutOfDate = new List<DinkumPlugin>();
        private static List<DinkumPlugin> pluginsUpToDate = new List<DinkumPlugin>();

        private static string configDirectory = Application.dataPath.Replace("Dinkum_Data", "BepInEx/config/");

        private void Start() { this.StartCoroutine(CheckPlugins()); }

        // TODO: We can probably modify this to use data from the developer initializing it instead of adding the data to the config file
        // TODO: But that would mean we can't have users add the value later. 
        private IEnumerator CheckPlugins() {

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
                string cfgFile = configDirectory + guid + ".cfg";

                //string cfgFile = Path.Combine(Directory.GetParent(Path.GetDirectoryName(typeof(BepInProcess).Assembly.Location)).FullName, "config", guid + ".cfg");
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

                TRTools.Log(($"{plugName} {id} current version: {modVersion}"));

                #endregion

                #region Get nexus mod page

                // new WWWForm();
                UnityWebRequest uwr = UnityWebRequest.Get($"https://www.nexusmods.com/dinkum/mods/{id}");
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success) {
                    TRTools.Log($"Error While Sending: {uwr.error}");
                    continue;
                }
                string[] nexusText = uwr.downloadHandler.text.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                #endregion

                #region Read nexus mod page and look for version number

                bool check = false;
                foreach (string line in nexusText) {
                    if (check && line.Contains("<div class=\"stat\">")) {

                        Match match = Regex.Match(line, "<[^>]+>[^0-9.]*([0-9.]+)[^0-9.]*<[^>]+>");
                        if (!match.Success) { break; }

                        Version nexusVersion = new Version(match.Groups[1].Value);
                        TRTools.Log($"{plugName} remote version: {nexusVersion}.");
                        if (nexusVersion > modVersion) {
                            TRTools.Log($"{plugName} new remote version: {nexusVersion}!");
                            pluginsOutOfDate.Add(new DinkumPlugin(plugName, id, modVersion, nexusVersion));
                        }
                        else if (showAllManagedMods.Value) { pluginsUpToDate.Add(new DinkumPlugin(plugName, id, modVersion, nexusVersion)); }
                        break;
                    }
                    if (line.Contains("<li class=\"stat-version\">")) { check = true; }
                }

                #endregion

            }
            finishedChecking = true;
        }
    }

    internal class DinkumPlugin {

        public DinkumPlugin(string name, int id, Version modVersion, Version nexusVersion) {
            this.name = name;
            this.id = id;
            this.modVersion = modVersion;
            this.nexusVersion = nexusVersion;
        }

        public string name;
        public int id;
        public Version modVersion;
        public Version nexusVersion;

    }

}
