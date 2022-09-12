using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {
    
    public static class TRTools {

        private static Dictionary<BaseUnityPlugin, TRPlugin> HookedPlugins = new Dictionary<BaseUnityPlugin, TRPlugin>();

        /// <summary>Tells you if the player is in the main menu or not.</summary>
        public static bool InMainMenu = true;

        /// <summary> Initializes the Tiny Resort toolset </summary>
        /// <param name="plugin">Your plugin. When calling this from your plugin, simply use 'this'.</param>
        /// <param name="nexusID">The ID of your mod on nexus. This is the number at the end of the URL for your mod's nexus page. (A mod page does not need to be published in order to have an ID)</param>
        public static TRPlugin Initialize(this BaseUnityPlugin plugin, int nexusID = -1, string chatTrigger = "") {

            if (HookedPlugins.ContainsKey(plugin)) { Debug.LogWarning(plugin.Info.Metadata.Name + " is being loaded twice!"); }

            HookedPlugins[plugin] = new TRPlugin();
            HookedPlugins[plugin].plugin = plugin;
            HookedPlugins[plugin].harmony = new Harmony(plugin.Info.Metadata.GUID);

            if (nexusID > 0) { HookedPlugins[plugin].nexusID = plugin.Config.Bind("Developer", "NexusID", nexusID, "Nexus Mod ID. You can find it on the mod's page on Nexus."); }
            HookedPlugins[plugin].debugMode = plugin.Config.Bind("Developer", "DebugMode", false, "If true, the BepinEx console will print out debug messages related to this mod.");
            if (!string.IsNullOrEmpty(chatTrigger)) {
                HookedPlugins[plugin].chatTrigger = 
                    plugin.Config.Bind("Developer", "Chat Trigger", chatTrigger, "What comes after the / in the chat when using chat commands for this mod. Example: If the chat trigger is 'tr' then all chat commands for this mod would start with /tr");
            }

            HookedPlugins[plugin].Logger = BepInEx.Logging.Logger.CreateLogSource(plugin.Info.Metadata.Name);
            var handler = new BepInExInfoLogInterpolatedStringHandler(18, 1, out var flag);
            if (flag) { handler.AppendLiteral("Plugin " + plugin.Info.Metadata.GUID + " (v" + plugin.Info.Metadata.Version + ") loaded!"); }
            HookedPlugins[plugin].Logger.LogInfo(handler);

            return HookedPlugins[plugin];

        }

        internal static void Log(string text, LogSeverity severity = LogSeverity.Standard, bool debugModeOnly = true) {
            LeadPlugin.Plugin.Log(text, severity, debugModeOnly);
        }

        internal static void QuickPatch(Type sourceClassType, string sourceMethod, Type patchClassType, string prefixMethod, string postfixMethod = "") {
            LeadPlugin.Plugin.QuickPatch(sourceClassType, sourceMethod, patchClassType, prefixMethod, postfixMethod);
        }

        #region Easy Notifications

        /// <summary> Displays a notification at the top of the screen right away rather than waiting on any previous notifications. </summary>
        /// <param name="title">Large text at the top of the notification.</param>
        /// <param name="subtitle">Smaller descriptive text below the title.</param>
        /// <param name="playSFX">If true, a sound effect will be played.</param>
        public static void TopNotification(string title, string subtitle, bool playSFX = false) {
            // TODO: Play sound effect
            NotificationPatch.forceClearNotification = true;
            NotificationManager.manage.makeTopNotification(title, subtitle);
            NotificationPatch.forceClearNotification = false;
        }
        
        #endregion
        
        /*#region Version Checking

        private static string currentGameVersion;

        public static string CheckGameVersion(string myModGameVersion) {
                
            if (!WorldManager.manageWorld) { return "Game not fully loaded. Can not compare version numbers."; }
            currentGameVersion = "v0." + WorldManager.manageWorld.masterVersionNumber.ToString() + "." + WorldManager.manageWorld.versionNumber.ToString();

            return myModGameVersion == currentGameVersion ? 
                       "Mod was created using this version of the game." : 
                       "Mod was created for a different version of the game than is running. Issues may occur.";
            
        }
        
        #endregion*/

    }

}
