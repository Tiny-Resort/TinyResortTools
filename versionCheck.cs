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

namespace TR.Tools {
    
    public class versionCheck {

        public static string currentGameVersion;

        public static bool verifyGameVersions(string myModGameVersion) {
            currentGameVersion = "v0." + WorldManager.manageWorld.masterVersionNumber.ToString() + "." + WorldManager.manageWorld.versionNumber.ToString();
            return myModGameVersion == currentGameVersion;
        }

        /*public bool verifyModVersions(currentModVersion) {
            currentGameVersion = "v0." + WorldManager.manageWorld.masterVersionNumber.ToString() + "." + WorldManager.manageWorld.versionNumber.ToString();
            return modGameVersion == currentGameVersion;
        }*/

        /*[HarmonyPrefix]
        public static void loadVersionNumberPrefix(SaveLoad __instance)
        {
            currentGameVersion = WorldManager.manageWorld.versionNumber;
            Logger.LogInfo("Current Game Version: " + currentGameVersion);
        }*/

    }
}
