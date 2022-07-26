using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using Mirror;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using UnityEngine.InputSystem;

namespace TR {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Tools : BaseUnityPlugin {

        public const string pluginGuid = "tinyresort.dinkum.TinyResortTools";
        public const string pluginName = "Tiny Resort Tools";
        public const string pluginVersion = "0.0.5";
        public static ManualLogSource StaticLogger;
        public static bool forceClearNotification;
        public static string modGameVersion = null;
        public static string currentGameVersion;

        private void Awake() {

            #region Logging
            StaticLogger = Logger;
            BepInExInfoLogInterpolatedStringHandler handler = new BepInExInfoLogInterpolatedStringHandler(18, 1, out bool flag);
            if (flag) { handler.AppendLiteral("Plugin " + pluginGuid + " (v" + pluginVersion + ") loaded!"); }
            StaticLogger.LogInfo(handler);
            #endregion

            #region Patching
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo makeTopNotification = AccessTools.Method(typeof(NotificationManager), "makeTopNotification");
            MethodInfo makeTopNotificationPrefix = AccessTools.Method(typeof(Tools), "makeTopNotificationPrefix");
            //harmony.Patch(makeTopNotification, new HarmonyMethod(makeTopNotificationPrefix));
            #endregion

        }

        // Forcibly clears the top notification so that it can be replaced immediately
        [HarmonyPrefix]
        public static bool makeTopNotificationPrefix(NotificationManager __instance) {

            if (forceClearNotification) {
                forceClearNotification = false;

                var toNotify = (List<string>)AccessTools.Field(typeof(NotificationManager), "toNotify").GetValue(__instance);
                var subTextNot = (List<string>)AccessTools.Field(typeof(NotificationManager), "subTextNot").GetValue(__instance);
                var soundToPlay = (List<ASound>)AccessTools.Field(typeof(NotificationManager), "soundToPlay").GetValue(__instance);
                var topNotificationRunning = AccessTools.Field(typeof(NotificationManager), "topNotificationRunning");
                var topNotificationRunningRoutine = topNotificationRunning.GetValue(__instance);

                // Clears existing notifications in the queue
                toNotify.Clear();
                subTextNot.Clear();
                soundToPlay.Clear();

                // Stops the current coroutine from continuing
                if (topNotificationRunningRoutine != null) {
                    __instance.StopCoroutine((Coroutine)topNotificationRunningRoutine);
                    topNotificationRunning.SetValue(__instance, null);
                }

                // Resets all animations related to the notificatin bubble appearing/disappearing
                __instance.StopCoroutine("closeWithMask");
                __instance.topNotification.StopAllCoroutines();
                var Anim = __instance.topNotification.GetComponent<WindowAnimator>();
                Anim.StopAllCoroutines();
                Anim.maskChild.enabled = false;
                Anim.contents.gameObject.SetActive(false);
                Anim.gameObject.SetActive(false);

                return true;

            }
            else return true;
        }


        public void Start()
        {
            modGameVersion = "v0.4.5";
            #region Logging

            StaticLogger = Logger;
            BepInExInfoLogInterpolatedStringHandler handler =
                new BepInExInfoLogInterpolatedStringHandler(18, 1, out var flag);
            if (flag)
            {
                handler.AppendLiteral("Plugin " + pluginGuid + " (v" + pluginVersion + ") loaded!");
            }

            StaticLogger.LogInfo(handler);

            #endregion

            ManualLogSource logger = Logger;
            
            Logger.LogInfo("Current Game Version: "+ currentGameVersion);
            
            
        }

        public static bool compareGameVersions(WorldManager __instance, string myModGameVersion)
        {
            currentGameVersion = "v0." + WorldManager.manageWorld.masterVersionNumber.ToString() + "." + WorldManager.manageWorld.versionNumber.ToString();
            return modGameVersion == currentGameVersion;
        }
        /*[HarmonyPrefix]
        public static void loadVersionNumberPrefix(SaveLoad __instance)
        {
            currentGameVersion = WorldManager.manageWorld.versionNumber;
            Logger.LogInfo("Current Game Version: " + currentGameVersion);
        }*/

    }
}
