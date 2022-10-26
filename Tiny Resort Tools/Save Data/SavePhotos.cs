using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "savePhotos")]
    internal class SavePhotos {
        
        [HarmonyPrefix] public static void Prefix(bool isClient) { TRBackup.clientInServer = isClient; }

    }

}
