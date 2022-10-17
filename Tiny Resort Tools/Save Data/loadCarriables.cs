using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadCarriables")]
    internal class loadCarriables {
        
        [HarmonyPostfix]
        public static void Postfix() {
            var inMainMenu = TRTools.InMainMenu;
            TRTools.InMainMenu = false;
            TRData.trueLoadEvent?.Invoke();
            if (inMainMenu) {
                TRTools.Log($"Test Initial Load");
                TRData.initialLoadEvent?.Invoke();
                TRTools.Log($"After Initial Load");
            }
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
        }
        
    }
    
}