using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    /// <summary>Information and functions relating to your plugin with respect to the TR toolset.</summary>
    public class TRPlugin {

        /// <summary>A harmony instance created specifically for your plugin to use.</summary>
        public Harmony harmony;
        internal BaseUnityPlugin plugin;
        
        internal ManualLogSource Logger;
        internal ConfigEntry<int> nexusID;
        internal ConfigEntry<bool> debugMode;
        internal ConfigEntry<string> chatTrigger;

        /// <summary>Logs to the BepInEx console.</summary>
        /// <param name="text">The text to post in the console.</param>
        /// <param name="debugModeOnly">If true, this message will only show in the console if the config file has DebugMode set to true.</param>
        public void Log(string text, bool debugModeOnly = true) {
            if (debugModeOnly && !debugMode.Value) return;
            Logger.LogInfo(text);
        }

        /// <summary>Logs a warning to the BepInEx console.</summary>
        /// <param name="text">The text to post in the console.</param>
        /// <param name="debugModeOnly">If true, this message will only show in the console if the config file has DebugMode set to true.</param>
        public void LogWarning(string text, bool debugModeOnly = true) {
            if (debugModeOnly && !debugMode.Value) return;
            Logger.LogWarning(text);
        }

        /// <summary>Logs an error to the BepInEx console.</summary>
        /// <param name="text">The text to post in the console.</param>
        public void LogError(string text) { Logger.LogError(text); }

        /// <summary>
        /// Allows you to patch methods using only one line of code instead of three per method patched.
        /// </summary>
        /// <param name="sourceClassType">Typically typeOf(className) where className is the class you are patching in Dinkum.</param>
        /// <param name="sourceMethod">The name of the method in Dinkum being patched.</param>
        /// <param name="patchClassType">Typically typeOf(className) where className is the name of your class (the one that contains the patch method).</param>
        /// <param name="prefixMethod">The name of the prefix method doing the patching.</param>
        /// <param name="postfixMethod">The name of the postfix method doing the patching.</param>
        public void QuickPatch(Type sourceClassType, string sourceMethod, Type patchClassType, string prefixMethod, string postfixMethod = "") {
            MethodInfo sourceMethodInfo = AccessTools.Method(sourceClassType, sourceMethod);
            MethodInfo prefixMethodInfo = prefixMethod.IsNullOrWhiteSpace() ? null : AccessTools.Method(patchClassType, prefixMethod);
            MethodInfo postfixMethodInfo = postfixMethod.IsNullOrWhiteSpace() ? null : AccessTools.Method(patchClassType, postfixMethod);
            var prefixHarmonyMethod = prefixMethod.IsNullOrWhiteSpace() ? null : new HarmonyMethod(prefixMethodInfo);
            var postfixHarmonyMethod = postfixMethod.IsNullOrWhiteSpace() ? null : new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(sourceMethodInfo, prefixHarmonyMethod, postfixHarmonyMethod);
        }

        /// <summary>Subscribes to the save system so that your mod data is saved and loaded properly.</summary>
        /// <param name="command">Subcommand to run.</param>
        /// <param name="description">Description to show if the user runs the help command.</param>
        /// <param name="method">Method to run when chat command is done.</param>
        /// <param name="argumentNames">The names of each argument your command takes. Used purely for the help description.</param>
        /// <returns>A reference to all the commands for your mod.</returns>
        public void AddCommand(string command, string description, Func<string[], string> method, params string[] argumentNames) {
            TRChat.AddCommand(plugin.Info.Metadata.Name, chatTrigger.Value.ToLower(), command, description, method, argumentNames);
        }

        /// <returns>Creates a new item.</returns>
        /// /// <param name="assetBundlePath">The path to your asset bundle, relative to the plugins folder.</param>
        /// <param name="uniqueItemID">A unique ID for your item. Do not change after releasing your mod. Changing will result in save data mixups.</param>
        public void AddCustomItem(string assetBundlePath, string uniqueItemID) { TRItems.AddCustomItem(this, assetBundlePath, uniqueItemID); }

        /// <summary>Adds a custom license to the system. Must be done for each custom license.</summary>
        /// <param name="licenseID">A unique string you are assigning this license only. Changing this after save data has been made WILL result in save data mixups. Spaces in this are replaced with underscores.</param>
        /// <param name="licenseName">The name that will appear on the license in-game. (Can be changed at any time without issue)</param>
        /// <param name="maxLevel">The highest unlockable level for this license.</param>
        /// <returns>The custom license that is created. Save a reference to this in order to access its state at any time.</returns>
        public TRCustomLicense AddLicense(int licenseID, string licenseName, int maxLevel = 1) {
            return TRLicenses.AddLicense(nexusID.Value, licenseID, licenseName, maxLevel);
        }
        
    }

}
