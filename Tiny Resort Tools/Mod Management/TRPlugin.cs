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
        public TRCustomItem AddCustomItem(string assetBundlePath, int uniqueItemID) { return TRItems.AddCustomItem(this, assetBundlePath, uniqueItemID); }

        /// <summary>Adds a custom licence to the system. Must be done for each custom licence.</summary>
        /// <param name="licenceID">A unique string you are assigning this licence only. Changing this after save data has been made WILL result in save data mixups. Spaces in this are replaced with underscores.</param>
        /// <param name="licenceName">The name that will appear on the licence in-game. (Can be changed at any time without issue)</param>
        /// <param name="maxLevel">The highest unlockable level for this licence. The true maximum is 5 since the game only shows up to 5 dots.</param>
        /// <returns>The custom licence that is created. Save a reference to this in order to access its state at any time.</returns>
        public TRCustomLicence AddLicence(int licenceID, string licenceName, int maxLevel = 1) {
            if (maxLevel > 5) {
                maxLevel = 5;
                TRTools.LogError("Custom Licence " + nexusID.Value + "." + licenceID + " " + licenceName + " can not have a max level above 5.");
            }
            return TRLicences.AddLicence(nexusID.Value, licenceID, licenceName, maxLevel);
        }
        
        /// <summary>
        /// Looks for a custom item that is currently using the item ID.
        /// </summary>
        /// <param name="ID">The ID of the item.</param>
        /// <returns>A string of the custom item's uniqueID or the item's ID. It returns null if one isn't found.</returns>
        public string GetSaveableItemID(int ID) { return TRItems.GetSaveableItemID(ID); } 

        /// <summary>
        /// Finds the current item ID for the item based on the uniqueID given.
        /// </summary>
        /// <param name="uniqueID">Takes in a uniqueID (normally obtained by GetSaveableItemID().</param>
        /// <returns>Returns the custom item's unique ID or the vanilla item if item id is passed in as a string. This returns -2 if it wasn't found. This is either due to the custom item being removed or a string being used that is neither a custom item or vanilla item. </returns>
        public int GetLoadableItemID(string uniqueID) { return TRItems.GetLoadableItemID(uniqueID); }

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

    }

}
