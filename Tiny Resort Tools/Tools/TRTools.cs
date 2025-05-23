﻿using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Core.Logging.Interpolation;
using HarmonyLib;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace TinyResort
{
    /// <summary>Used mostly for initialization and internal control of the API's features.</summary>
    public static class TRTools {

        private static readonly Dictionary<BaseUnityPlugin, TRPlugin> HookedPlugins = new();
        internal static bool _InMainMenu = true;

        internal static bool LeavingMainMenu;

        /// <summary>Tells you if the player is in the main menu or not.</summary>
        public static bool InMainMenu => _InMainMenu;

        /// <summary> For events related to scene changes. </summary>
        public delegate void SceneEvent();

        /// <summary>
        /// Quitting to main menu will reload the main scene. If you want specific objects (user interface elements) to persist,
        /// you need to recreate them when the scene is reloaded. So, you should create a method for making the objects, and subscribe it to this event.
        /// </summary>
        public static SceneEvent sceneSetupEvent;

        /// <summary> Initializes the Tiny Resort toolset </summary>
        /// <param name="plugin">Your plugin. When calling this from your plugin, simply use 'this'.</param>
        /// <param name="nexusID">
        ///     The ID of your mod on nexus. This is the number at the end of the URL for your mod's nexus page.
        ///     (A mod page does not need to be published in order to have an ID)
        /// </param>
        /// <param name="chatTrigger">
        ///     What short text you want associated with your mod when using chat commands. If you put 'tr'
        ///     for example, then all your chat commands start with /tr
        /// </param>
        public static TRPlugin Initialize(this BaseUnityPlugin plugin, int nexusID = -1, string chatTrigger = "") {

            if (HookedPlugins.ContainsKey(plugin)) Debug.LogWarning(plugin.Info.Metadata.Name + " is being loaded twice!");

            HookedPlugins[plugin] = new TRPlugin();
            HookedPlugins[plugin].plugin = plugin;
            HookedPlugins[plugin].harmony = new Harmony(plugin.Info.Metadata.GUID);

            // Add config entry for nexus ID. If the nexus ID is invalid, set it to -1
            var id = nexusID <= 0 ? -1 : nexusID;
            HookedPlugins[plugin].nexusID = plugin.Config.Bind(
                "Developer", "NexusID", id, "Nexus Mod ID. You can find it in the URL on the mod's Nexus page."
            );

            // If the nexus ID passed by initialize is valid, then force that value into the config
            if (id > 0 || HookedPlugins[plugin].nexusID.Value < -1) {
                HookedPlugins[plugin].nexusID.Value = id;
                HookedPlugins[plugin].plugin.Config.Save();
            }

            // If the ID was invalid give a warning
            if (id == -1)
                LogError(
                    "We highly recommend adding the nexusID from the URL of your mod page. "
                    + "This will allow you to use all of the TR Tool's features and shows users when there is an update for your mod. "
                    + "This error can be safely ignored if you are in a testing phase, but please add one before the release.",
                    true
                );

            // Add debugmode and chat command config entries
            HookedPlugins[plugin].debugMode = plugin.Config.Bind(
                "Developer", "DebugMode", false,
                "If true, the BepinEx console will print out debug messages related to this mod."
            );
            if (!string.IsNullOrEmpty(chatTrigger) && chatTrigger.ToLower() != "help")
                HookedPlugins[plugin].chatTrigger =
                    plugin.Config.Bind(
                        "Chat", "ChatTrigger", chatTrigger.ToLower(),
                        "What comes after the / in the chat when using chat commands for this mod. Example: If the chat trigger is 'tr' then all chat commands for this mod would start with /tr"
                    );

            HookedPlugins[plugin].Logger = Logger.CreateLogSource(plugin.Info.Metadata.Name);
            var handler = new BepInExInfoLogInterpolatedStringHandler(18, 1, out var flag);
            if (flag)
                handler.AppendLiteral(
                    "Plugin " + plugin.Info.Metadata.GUID + " (v" + plugin.Info.Metadata.Version + ") loaded!"
                );
            HookedPlugins[plugin].Logger.LogInfo(handler);

            return HookedPlugins[plugin];

        }

        internal static string TRAscii() {
            string asciiArt;
            asciiArt =
                "==========================================================================================================\n";
            asciiArt +=
                "    _____ _____ _   ___   __ ______ _____ _____  ___________ _____   _____ _____  _____ _      _____ \n";
            asciiArt +=
                "   |_   _|_   _| \\ | \\ \\ / / | ___ \\  ___/  ___||  _  | ___ \\_   _| |_   _|  _  ||  _  | |    /  ___|\n";
            asciiArt +=
                "     | |   | | |  \\| |\\ V /  | |_/ / |__ \\ `--. | | | | |_/ / | |     | | | | | || | | | |    \\ `--. \n";
            asciiArt +=
                "     | |   | | | . ` | \\ /   |    /|  __| `--. \\| | | |    /  | |     | | | | | || | | | |     `--. \\\n";
            asciiArt +=
                "     | |  _| |_| |\\  | | |   | |\\ \\| |___/\\__/ /\\ \\_/ / |\\ \\  | |     | | \\ \\_/ /\\ \\_/ / |____/\\__/ /\n";
            asciiArt +=
                "     \\_/  \\___/\\_| \\_/ \\_/   \\_| \\_\\____/\\____/  \\___/\\_| \\_| \\_/     \\_/  \\___/  \\___/\\_____/\\____/ \n\n";
            asciiArt +=
                "==========================================================================================================\n\n";

            return asciiArt;
        }

        internal static string TRDeveloperMode() {
            string devArt;
            devArt =
                "\n==========================================================================================================\n";
            devArt += "______ _____ _   _ _____ _     ___________ ___________  ___  ______________ _____ \n";
            devArt += "|  _  \\  ___| | | |  ___| |   |  _  | ___ \\  ___| ___ \\ |  \\/  |  _  |  _  \\  ___|\n";
            devArt += "| | | | |__ | | | | |__ | |   | | | | |_/ / |__ | |_/ / | .  . | | | | | | | |__  \n";
            devArt += "| | | |  __|| | | |  __|| |   | | | |  __/|  __||    /  | |\\/| | | | | | | |  __| \n";
            devArt += "| |/ /| |___\\ \\_/ / |___| |___\\ \\_/ / |   | |___| |\\ \\  | |  | \\ \\_/ / |/ /| |___ \n";
            devArt += "|___/ \\____/ \\___/\\____/\\_____/\\___/\\_|   \\____/\\_| \\_| \\_|  |_/\\___/|___/ \\____/ \n\n";
            devArt +=
                "==========================================================================================================\n\n";

            return devArt;
        }

        internal static void Log(string text, bool debugModeOnly = true, bool useASCII = false) {
            if (useASCII) text = $"\n\n\n{TRAscii()}{text.ToUpper()}\n\n";
            LeadPlugin.plugin.Log(text, debugModeOnly);

        }

        internal static void LogWarning(string text, bool debugModeOnly = true, bool useASCII = false) {
            if (useASCII) text = $"\n\n\n{TRAscii()}{text.ToUpper()}\n\n";
            LeadPlugin.plugin.LogWarning(text, debugModeOnly);
        }

        internal static void LogError(string text, bool useASCII = false) {
            if (useASCII) text = $"\n\n\n{TRAscii()}{text.ToUpper()}\n\n";
            LeadPlugin.plugin.LogError(text);
        }

        internal static void QuickPatch(
            Type sourceClassType, string sourceMethod, Type patchClassType, string prefixMethod, string postfixMethod = ""
        ) =>
            LeadPlugin.plugin.QuickPatch(sourceClassType, sourceMethod, patchClassType, prefixMethod, postfixMethod);

        #region Easy Notifications

        /// <summary>
        ///     Displays a notification at the top of the screen right away rather than waiting on any previous
        ///     notifications.
        /// </summary>
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

            if (!WorldManager.Instance) { return "Game not fully loaded. Can not compare version numbers."; }
            currentGameVersion = "v0." + WorldManager.Instance.masterVersionNumber.ToString() + "." + WorldManager.Instance.versionNumber.ToString();

            return myModGameVersion == currentGameVersion ?
                       "Mod was created using this version of the game." :
                       "Mod was created for a different version of the game than is running. Issues may occur.";

        }

        #endregion*/
    }
}
