//using System;
//using HarmonyLib;
//using TR;
using UnityEngine;
//using UnityEngine.SceneManagement;

namespace TR {
    
    //[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Tools : MonoBehaviour { // : BaseUnityPlugin {
        
        //public const string pluginGuid = "tinyresort.dinkum.TinyResortTools";
        //public const string pluginName = "Tiny Resort Tools";
        //public const string pluginVersion = "0.1.0";

        public static string currentGameVersion;
        public static bool isModVersionChecked;
        public static bool isGameVersionChecked;
        public static string gameVersionString;
        public static string gameVersionReturnString;

        public static void Initialize() {
            //Harmony harmony = new Harmony(pluginID);
         //   Debug.Log("TESTTESTTEST");            
            //harmony.PatchAll();
        }

        public static bool forceClearNotification;
       /* public static void topNotification(string title, string subtitle) {
            forceClearNotification = true;
            NotificationManager.manage.makeTopNotification(title, subtitle);
        }*/

/*        public static string verifyGameVersion(string myModGameVersion) {
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
        }*/

    }
}
