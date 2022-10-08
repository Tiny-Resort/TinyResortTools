using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadCarriables")]
    internal class loadCarriables {
        
        public static void Postfix() {
            TRTools.InMainMenu = false;
            TRData.postLoadEvent?.Invoke();
            TRData.injectDataEvent?.Invoke();
            //TRItems.LoadCustomItemPostLoad();
        }
        
    }
    
}