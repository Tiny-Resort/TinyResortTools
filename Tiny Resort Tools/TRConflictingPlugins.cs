using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.Bootstrap;
using HarmonyLib;

namespace TinyResort {

	public class TRConflictingPlugins {
		
		public Dictionary<string, PluginInfo> pluginInfos;
		internal static Dictionary<string, string> Data = new Dictionary<string, string>();
		internal static List<PluginInfo> conflicts = new List<PluginInfo>();
		internal static string allConflicts;
		
		internal void GetAllLoadedPlugins() {
			pluginInfos = UnityChainloader.Instance.Plugins;
		}

		internal bool CheckIfModsConflicts() {
			foreach (var data in Data) {
				PluginInfo oldModInfo;
				if (pluginInfos.TryGetValue(data.Value, out oldModInfo)) {
					conflicts.Add(oldModInfo);
				}
			}
			if (conflicts != null) return true;
			return false;
		}

		internal void DisplayAllConflictingMods() {
			for (int i = 0; i < conflicts.Count; i++) {
				LeadPlugin.Plugin.Log($"Mod {i}: {conflicts[i].Metadata.Name}");
				allConflicts = allConflicts == null ? $"{conflicts[i].Metadata.Name}" : $",{conflicts[i].Metadata.Name}";
			}
		}
		
		public void SetConflictingPlugin(string currentMod, string pluginName) {
			Data[currentMod] = pluginName;
		}

		public void RunSequence() {
			GetAllLoadedPlugins();
			CheckIfModsConflicts();
			DisplayAllConflictingMods();
		}


	}


}
