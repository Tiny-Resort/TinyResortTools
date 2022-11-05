using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort; 

// loadInv is required because clients only run this when joining a server.

[HarmonyPatch(typeof(CustomNetworkManager), "OnClientConnect")]
[HarmonyPriority(1)]
internal class OnClientConnect {

    [HarmonyPostfix]
    public static void Prefix(CustomNetworkManager __instance, bool ___hosting) {
        TRItems.runningClient = false;
        if (!___hosting) TRItems.runningClient = true;
    }
}
