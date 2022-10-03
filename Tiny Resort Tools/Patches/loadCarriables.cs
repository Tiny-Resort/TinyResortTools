using System.Collections.Generic;
using System.Net;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(SaveLoad), "loadCarriables")]
    internal class loadCarriables {
        
        [HarmonyPostfix]
        public static void Postfix() {
            /*TRTools.Log("Did this get patched????????\n\n\n");
            TRData.injectDataEvent?.Invoke();
            TRData.postLoadEvent?.Invoke();*/
            /*if (NetworkMapSharer.share.localChar) return;

            TRItems.LoadModSavedData(TRItems.ToLoad.AfterNetwork, true);
            
            foreach (var item in TRItems.savedItemDataLate) {
                if (!TRItems.customItems.TryGetValue(item.uniqueID, out var customItem)) continue;
                if (item.vehicle != null) { item.vehicle.Restore(); }

            } */
            TRItems.LoadCustomItemPostLoad();
        }
    }
}