using HarmonyLib;
using UnityEngine;

namespace TR {
    
    public class Tools : MonoBehaviour {

        public static string currentGameVersion;
        public static bool isModVersionChecked;
        public static bool isGameVersionChecked;
        public static string gameVersionString;
        public static string gameVersionReturnString;

        public static void Initialize(Harmony harmony) { harmony.PatchAll(); }

        public static bool forceClearNotification;
        public static void topNotification(string title, string subtitle) {
            forceClearNotification = true;
            NotificationManager.manage.makeTopNotification(title, subtitle);
        }

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

    }
}
