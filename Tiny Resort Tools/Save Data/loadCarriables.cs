/*using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    // LoadCarriables is only run OnServerStart, so all of these will only run when loading your file.

    [HarmonyPatch(typeof(SaveLoad), "loadCarriables")]
    [HarmonyPriority(1)]
    internal class loadCarriables {

        [HarmonyPostfix]
        public static void Postfix() {
            TRTools.LeavingMainMenu = true;
            TRData.trueLoadEvent?.Invoke();
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
            TRTools._InMainMenu = false;

            TRTools.LogError($"Run from loadCarriables");
        }
    }
}*/



