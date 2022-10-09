using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadCarriables")]
    internal class loadCarriables {
        
        public static void Postfix() {
            
            TRData.injectDataEvent?.Invoke();

            if (TRTools.InMainMenu) {
                TRData.initialLoadEvent?.Invoke();
                TRTools.InMainMenu = false;
            }
            
            TRData.postLoadEvent?.Invoke();
            
        }
        
    }
    
}