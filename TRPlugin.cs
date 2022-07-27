using System;
using BepInEx;
using HarmonyLib;
using TR.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TR {
    
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TRPlugin : BaseUnityPlugin {
        public const string pluginGuid = "tinyresort.dinkum.TinyResortTools";
        public const string pluginName = "Tiny Resort Tools";
        public const string pluginVersion = "0.1.0";

        public static bool forceClearNotification;
        internal void Awake() {
            Harmony harmony = new Harmony("tinyresort.dinkum.TinyResortTools");
            harmony.PatchAll();
        }
    }
}
