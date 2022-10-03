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
            if (NetworkMapSharer.share.localChar) return;
            
            TRItems.savedVehicleData = (List<ItemSaveData>)TRItems.Data.GetValue("CurrentVehicles", new List<ItemSaveData>());

            foreach (var vehicle in TRItems.savedVehicleData) {
                if (!TRItems.customItems.TryGetValue(vehicle.uniqueID, out var customItem)) continue;
                vehicle.vehicle.RestoreVehicle(); } 
        }
    }
}