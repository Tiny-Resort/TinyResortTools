using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadCarriables")]
    internal class loadCarriables {
        
        [HarmonyPostfix]
        public static void Postfix() {
            TRData.trueLoadEvent?.Invoke();
            if (TRTools.InMainMenu) { TRData.initialLoadEvent?.Invoke(); }
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
            TRTools.InMainMenu = false;
        }
        
    }
    
}