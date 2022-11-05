using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort; 

// loadInv is required because clients only run this when joining a server.

[HarmonyPatch(typeof(LoadingScreen), "disappear")]
[HarmonyPriority(1)]
internal class disappear {

    // This goes nuts and doesn't really work. We probably can get away with waiting till they are actually loaded in and loading in the items? 
    // I think some of the stuff isn't accesible? Prob mail?
    [HarmonyPostfix]
    public static void Postfix() {
        if (TRItems.runningClient) {
            TRData.postClientLoadEvent?.Invoke();
            TRItems.runningClient = false;
        }

        //TRData.injectClientDataEvent?.Invoke();
    }
}
