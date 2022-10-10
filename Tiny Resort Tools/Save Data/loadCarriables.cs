using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadCarriables")]
    internal class loadCarriables {
        
        [HarmonyPostfix]
        public static void Postfix() {
            TRTools.Log($"Running loadCarriables");

            if (TRTools.InMainMenu) {
                TRTools.Log($"Running loadCarriables: Main menu");
                TRData.initialLoadEvent?.Invoke();
                TRTools.InMainMenu = false;
            }
            
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke(); 


        }
        
    }
    
}