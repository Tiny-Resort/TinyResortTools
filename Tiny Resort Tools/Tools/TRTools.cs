using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    /// <summary>Used mostly for initialization and internal control of the API's features.</summary>
    public static class TRTools {

        private static Dictionary<BaseUnityPlugin, TRPlugin> HookedPlugins = new Dictionary<BaseUnityPlugin, TRPlugin>();

        /// <summary>Tells you if the player is in the main menu or not.</summary>
        public static bool InMainMenu = true;
        internal static bool LeavingMainMenu;

        /// <summary> Initializes the Tiny Resort toolset </summary>
        /// <param name="plugin">Your plugin. When calling this from your plugin, simply use 'this'.</param>
        /// <param name="nexusID">The ID of your mod on nexus. This is the number at the end of the URL for your mod's nexus page. (A mod page does not need to be published in order to have an ID)</param>
        /// <param name="chatTrigger">What short text you want associated with your mod when using chat commands. If you put 'tr' for example, then all your chat commands start with /tr</param>
        public static TRPlugin Initialize(this BaseUnityPlugin plugin, int nexusID = -1, string chatTrigger = "") {

            if (HookedPlugins.ContainsKey(plugin)) { Debug.LogWarning(plugin.Info.Metadata.Name + " is being loaded twice!"); }

            HookedPlugins[plugin] = new TRPlugin();
            HookedPlugins[plugin].plugin = plugin;
            HookedPlugins[plugin].harmony = new Harmony(plugin.Info.Metadata.GUID);
            
            if (nexusID > 0 || nexusID == -1) {
                HookedPlugins[plugin].nexusID = plugin.Config.Bind("Developer", "NexusID", nexusID, "Nexus Mod ID. You can find it on the mod's page on Nexus.");
                // Enforce nexus ID if the developer set one
                HookedPlugins[plugin].nexusID.Value = nexusID;
                HookedPlugins[plugin].plugin.Config.Save();
            }
            if (nexusID == -1) {
                TRTools.LogError($"We highly recommend adding the nexusID from the URL of your mod page. This will allow you to use all of the TR Tool's features and shows users when there is an update for your mod. This error can be safely ignored if you are in a testing phase, but please add one before the release.", useASCII:true);
            }
            HookedPlugins[plugin].debugMode = plugin.Config.Bind("Developer", "DebugMode", false, "If true, the BepinEx console will print out debug messages related to this mod.");
            if (!string.IsNullOrEmpty(chatTrigger) && chatTrigger.ToLower() != "help") {
                HookedPlugins[plugin].chatTrigger =
                    plugin.Config.Bind("Chat", "ChatTrigger", chatTrigger.ToLower(), "What comes after the / in the chat when using chat commands for this mod. Example: If the chat trigger is 'tr' then all chat commands for this mod would start with /tr");
            }

            HookedPlugins[plugin].Logger = BepInEx.Logging.Logger.CreateLogSource(plugin.Info.Metadata.Name);
            var handler = new BepInExInfoLogInterpolatedStringHandler(18, 1, out var flag);
            if (flag) { handler.AppendLiteral("Plugin " + plugin.Info.Metadata.GUID + " (v" + plugin.Info.Metadata.Version + ") loaded!"); }
            HookedPlugins[plugin].Logger.LogInfo(handler);

            return HookedPlugins[plugin];

        }

        internal static string TRAscii() {
            string asciiArt;
            asciiArt = "==========================================================================================================\n";
            asciiArt += "    _____ _____ _   ___   __ ______ _____ _____  ___________ _____   _____ _____  _____ _      _____ \n";
            asciiArt += "   |_   _|_   _| \\ | \\ \\ / / | ___ \\  ___/  ___||  _  | ___ \\_   _| |_   _|  _  ||  _  | |    /  ___|\n";
            asciiArt += "     | |   | | |  \\| |\\ V /  | |_/ / |__ \\ `--. | | | | |_/ / | |     | | | | | || | | | |    \\ `--. \n";
            asciiArt += "     | |   | | | . ` | \\ /   |    /|  __| `--. \\| | | |    /  | |     | | | | | || | | | |     `--. \\\n";
            asciiArt += "     | |  _| |_| |\\  | | |   | |\\ \\| |___/\\__/ /\\ \\_/ / |\\ \\  | |     | | \\ \\_/ /\\ \\_/ / |____/\\__/ /\n";
            asciiArt += "     \\_/  \\___/\\_| \\_/ \\_/   \\_| \\_\\____/\\____/  \\___/\\_| \\_| \\_/     \\_/  \\___/  \\___/\\_____/\\____/ \n\n";
            asciiArt += "==========================================================================================================\n\n";

            return asciiArt;
        }

        internal static string TRDeveloperMode() {
            string devArt;
            devArt = "\n==========================================================================================================\n";
            devArt += "______ _____ _   _ _____ _     ___________ ___________  ___  ______________ _____ \n";
            devArt += "|  _  \\  ___| | | |  ___| |   |  _  | ___ \\  ___| ___ \\ |  \\/  |  _  |  _  \\  ___|\n";
            devArt += "| | | | |__ | | | | |__ | |   | | | | |_/ / |__ | |_/ / | .  . | | | | | | | |__  \n";
            devArt += "| | | |  __|| | | |  __|| |   | | | |  __/|  __||    /  | |\\/| | | | | | | |  __| \n";
            devArt += "| |/ /| |___\\ \\_/ / |___| |___\\ \\_/ / |   | |___| |\\ \\  | |  | \\ \\_/ / |/ /| |___ \n";
            devArt += "|___/ \\____/ \\___/\\____/\\_____/\\___/\\_|   \\____/\\_| \\_| \\_|  |_/\\___/|___/ \\____/ \n\n";
            devArt += "==========================================================================================================\n\n";
            
            return devArt;
        }

        internal static void Log(string text, bool debugModeOnly = true, bool useASCII = false) {
            if (useASCII) { text = $"\n\n\n{TRAscii()}{text.ToUpper()}\n\n"; }
            LeadPlugin.plugin.Log(text, debugModeOnly);

        }

        internal static void LogWarning(string text, bool debugModeOnly = true, bool useASCII = false) {
            if (useASCII) { text = $"\n\n\n{TRAscii()}{text.ToUpper()}\n\n"; }
            LeadPlugin.plugin.LogWarning(text, debugModeOnly);
        }

        internal static void LogError(string text, bool useASCII = false) {
            if (useASCII) { text = $"\n\n\n{TRAscii()}{text.ToUpper()}\n\n"; }
            LeadPlugin.plugin.LogError(text);
        }

        internal static void QuickPatch(Type sourceClassType, string sourceMethod, Type patchClassType, string prefixMethod, string postfixMethod = "") { LeadPlugin.plugin.QuickPatch(sourceClassType, sourceMethod, patchClassType, prefixMethod, postfixMethod); }
        
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
