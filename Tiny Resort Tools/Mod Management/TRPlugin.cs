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
        public void AddCommand(string command, string description, Func<string[], string> method, params string[] argumentNames) { TRChat.AddCommand(plugin.Info.Metadata.Name, chatTrigger.Value.ToLower(), command, description, method, argumentNames); }

        /// <returns>Creates a new item.</returns>
        /// /// <param name="assetBundlePath">The path to your asset bundle, relative to the plugins folder.</param>
        /// <param name="uniqueItemID">A unique ID for your item. Do not change after releasing your mod. Changing will result in save data mixups.</param>
        public TRCustomItem AddCustomItem(string assetBundlePath, int uniqueItemID) {

            if (!LeadPlugin.developerMode.Value && nexusID.Value == -1) {
                TRTools.LogError($"Attempting to create a custom item with an incorrect nexusID. Please contact the mod developer ({plugin.Info.Metadata.Name}) to update their nexus ID.");
                return null;
            }

            if (LeadPlugin.developerMode.Value && nexusID.Value == -1) { TRTools.LogError($"Attempting to create a custom item with an incorrect nexusID. This is allowed since you are in developer mode, but please update the nexus ID before publishing ({plugin.Info.Metadata.Name}) to Nexus. If this is not your mod, please notify the owner of the mod."); }
            return TRItems.AddCustomItem(this, assetBundlePath, uniqueItemID);
        }

        /// <summary>
        /// Creates a new item. The preferred method is to use an asset bundle, but this method allows you to condense the number of assetbundles you create.
        /// </summary>
        /// <param name="uniqueItemID">A unique ID for your item. Do not change after releasing your mod. Changing will result in save data mixups.</param>
        /// <param name="inventoryItem">The InvItem script of an item.</param>
        /// <param name="tileObject">The tileObject script of an item.</param>
        /// <param name="tileObjectSettings">The tileObjectSettings script of an item.</param>
        /// <param name="tileTypes">The tileTypes script of an item.</param>
        /// <param name="vehicle">The vehicle script of an item.</param>
        /// <param name="pickUpAndCarry">The pickUpAndCarry script of an item.</param>
        /// <returns></returns>
        public TRCustomItem AddCustomItem(
            int uniqueItemID, InventoryItem inventoryItem = null, TileObject tileObject = null,
            TileObjectSettings tileObjectSettings = null, TileTypes tileTypes = null, Vehicle vehicle = null, PickUpAndCarry pickUpAndCarry = null
        ) {
            if (!LeadPlugin.developerMode.Value && nexusID.Value == -1) {
                TRTools.LogError($"Attempting to create a custom item with an incorrect nexusID. Please contact the mod developer ({plugin.Info.Metadata.Name}) to update their nexus ID.");
                return null;
            }

            if (LeadPlugin.developerMode.Value && nexusID.Value == -1) { TRTools.LogError($"Attempting to create a custom item with an incorrect nexusID. This is allowed since you are in developer mode, but please update the nexus ID before publishing ({plugin.Info.Metadata.Name}) to Nexus. If this is not your mod, please notify the owner of the mod."); }

            return TRItems.AddCustomItem(this, uniqueItemID, inventoryItem, tileObject, tileObjectSettings, tileTypes, vehicle, pickUpAndCarry);
        }

        /// <summary>Adds a custom licence to the system. Must be done for each custom licence.</summary>
        /// <param name="licenceID">A unique string you are assigning this licence only. Changing this after save data has been made WILL result in save data mixups. Spaces in this are replaced with underscores.</param>
        /// <param name="licenceName">The name that will appear on the licence in-game. (Can be changed at any time without issue)</param>
        /// <param name="maxLevel">The highest unlockable level for this licence. The true maximum is 5 since the game only shows up to 5 dots.</param>
        /// <returns>The custom licence that is created. Save a reference to this in order to access its state at any time.</returns>
        public TRCustomLicence AddLicence(int licenceID, string licenceName, int maxLevel = 1) {
            if (!LeadPlugin.developerMode.Value && nexusID.Value == -1) {
                TRTools.LogError($"Attempting to create a custom licence with an incorrect nexusID. Please contact the mod developer ({plugin.Info.Metadata.Name}) to update their nexus ID.");
                return null;
            }
            if (LeadPlugin.developerMode.Value && nexusID.Value == -1) { TRTools.LogError($"Attempting to create a custom licence with an incorrect nexusID. This is allowed since you are in developer mode, but please update the nexus ID before publishing ({plugin.Info.Metadata.Name}) to Nexus. If this is not your mod, please notify the owner of the mod."); }

            if (maxLevel > 5) {
                maxLevel = 5;
                TRTools.LogError("Custom Licence " + nexusID.Value + "." + licenceID + " " + licenceName + " can not have a max level above 5.");
            }
            return TRLicences.AddLicence(this, licenceID, licenceName, maxLevel);
        }

        /// <summary>
        /// Set a conflicting plugin to warn the user's about if they have it in their folder. 
        /// </summary>
        /// <param name="conflictingPlugin">The GUID for the conflicting plugin.</param>
        public void AddConflictingPlugin(string conflictingPlugin) { TRConflictingPlugins.AddConflictingPlugin(plugin.Info, conflictingPlugin); }

        /// <summary>
        /// If you want to do something specific if they ignore the warning, use this to check in Start()
        /// </summary>
        /// <returns></returns>
        public bool PlayingWithConflicts() { return TRConflictingPlugins.PlayingWithConflicts(plugin.Info); }

        /// <summary>
        /// Lets you set a minimum version of the API required for your mod. 
        /// </summary>
        /// <param name="minVersion">The version you want to set as the minimum. This needs to be in the format X.X.X</param>
        /// <returns>Returns true or false and will throw an error in the BepInEx logs.</returns>
        public bool RequireAPIVersion(string minVersion) {

            string[] compareVersion = minVersion.Split('.');
            if (compareVersion.Length < 3) {
                TRTools.LogError($"Version must be in the format: X.X.X");
                return false;
            }

            int.TryParse(compareVersion[0], out int majorCompareVersion);
            int.TryParse(compareVersion[1], out int minorCompareVersion);
            int.TryParse(compareVersion[2], out int patchCompareVersion);

            string[] versions = LeadPlugin.pluginVersion.Split('.');
            int.TryParse(versions[0], out int majorVersion);
            int.TryParse(versions[1], out int minorVersion);
            int.TryParse(versions[2], out int patchVersion);

            //TRTools.LogError($"Compare Version: ({majorCompareVersion}.{minorCompareVersion}.{patchCompareVersion})");
            //TRTools.LogError($"API Version: ({majorVersion}.{minorVersion}.{patchVersion})");

            if (majorVersion < majorCompareVersion) {
                TRTools.LogError($"{plugin.Info.Metadata.Name} has an API Minimum version of {majorCompareVersion}.{minorCompareVersion}.{patchCompareVersion}. Please update the TR Tools API.");
                return false;
            }
            else if (majorVersion == majorCompareVersion && minorVersion < minorCompareVersion) {
                TRTools.LogError($"{plugin.Info.Metadata.Name} has an API Minimum version of {majorCompareVersion}.{minorCompareVersion}.{patchCompareVersion}. Please update the TR Tools API.");
                return false; }
            else if (majorVersion == majorCompareVersion && minorVersion == minorCompareVersion && patchVersion < patchCompareVersion) {
                TRTools.LogError($"{plugin.Info.Metadata.Name} has an API Minimum version of {majorCompareVersion}.{minorCompareVersion}.{patchCompareVersion}. Please update the TR Tools API.");
                return false; }
            else { return true; }
        }

        /// <summary>
        /// Returns the current plugin version, so you can customize your mod depending on specific versions.
        /// </summary>
        /// <returns>A string in the format X.X.X, where X are numbers.</returns>
        public string GetAPIVersion() { return LeadPlugin.pluginVersion; }
    }

}
