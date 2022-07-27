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
        public static bool isModVersionChecked;
        public static bool isGameVersionChecked;
        public static string gameVersionString;
        public static string gameVersionReturnString;

        public static string verifyGameVersion(string myModGameVersion) {
            if (!isGameVersionChecked) {
                try {
                    currentGameVersion = "v0." + WorldManager.manageWorld.masterVersionNumber.ToString() + "." + WorldManager.manageWorld.versionNumber.ToString();
                }
                catch (MissingReferenceException e) { return null; }
                if (myModGameVersion == currentGameVersion) {
                    isGameVersionChecked = !isGameVersionChecked;
                    return "The game version's and mod's game version match.";
                    
                }
                isGameVersionChecked = !isGameVersionChecked;
                return "The game version's and mod's game version not match.";
            }
            return null;
        }

        /*public bool verifyModVersions(currentModVersion) {
            currentGameVersion = "v0." + WorldManager.manageWorld.masterVersionNumber.ToString() + "." + WorldManager.manageWorld.versionNumber.ToString();
            return modGameVersion == currentGameVersion;
        }*/

    }
}
