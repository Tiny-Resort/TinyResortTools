using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace NexusUpdate {

    [BepInPlugin("tinyresort.dinkum.NexusUpdate", "Nexus Update", "0.5.0")]
    public class BepInExPlugin : BaseUnityPlugin {

        public static Dictionary<string, PluginInfo> PluginInfos { get; }

        private void Awake() {
            BepInExPlugin.context = this;
            BepInExPlugin.modEnabled = base.Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            BepInExPlugin.showAllManagedMods = base.Config.Bind<bool>("General", "ShowAllManagedMods", false, "Show all mods that have a nexus ID in the list, even if they are up-to-date");
            BepInExPlugin.createEmptyConfigFiles = base.Config.Bind<bool>("General", "CreateEmptyConfigFiles", false, "Create empty GUID-based config files for mods that don't have them (may cause there to be duplicate config files)");
            BepInExPlugin.nexusID = base.Config.Bind<int>("General", "NexusID", 102, "Nexus mod ID for updates");

            bool flag = !BepInExPlugin.modEnabled.Value;
            if (!flag) {
                //BepInExPlugin.ApplyConfig();
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            }
        }

        private void Start() {
            bool value = BepInExPlugin.modEnabled.Value;
            if (value) { base.StartCoroutine(this.CheckPlugins()); }
        }

        private IEnumerator CheckPlugins() {
            foreach (KeyValuePair<string, PluginInfo> kvp in PluginInfos) {
                SemanticVersioning.Version currentVersion = kvp.Value.Metadata.Version;
                string pluginName = kvp.Value.Metadata.Name;
                string guid = kvp.Value.Metadata.GUID;
                string cfgFile = Path.Combine(new string[] { Directory.GetParent(Path.GetDirectoryName(typeof(BepInProcess).Assembly.Location)).FullName, "config", guid + ".cfg" });
                yield break;
            }
        }

        private static readonly bool isDebug = true;
        private static BepInExPlugin context;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> showAllManagedMods;
        public static ConfigEntry<bool> createEmptyConfigFiles;
        public static ConfigEntry<int> nexusID;
        private static List<NexusUpdatable> nexusUpdatables = new List<NexusUpdatable>();
        private static List<NexusUpdatable> nexusNonupdatables = new List<NexusUpdatable>();
        private static bool finishedChecking = false;
    }
}
