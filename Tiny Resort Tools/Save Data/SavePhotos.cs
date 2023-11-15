using HarmonyLib;

namespace TinyResort {
    // This runs and checks if the user is a client. It is used fr the backup manager to check if the person saving is on a server. 

    [HarmonyPatch(typeof(SaveAndLoad), "SavePhotos")]
    internal class SavePhotosPrefix {

        [HarmonyPrefix] public static void Prefix(bool isClient) => TRBackup.clientInServer = isClient;
    }
}
