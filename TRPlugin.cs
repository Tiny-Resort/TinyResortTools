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
        public const string pluginName = "Time Management";
        public const string pluginVersion = "1.2.1";

        public static bool forceClearNotification;
        internal void Awake() {
            //TR.Instance = this;
           // new SL();
            Harmony harmony = new Harmony("tinyresort.dinkum.TinyResortTools");
            harmony.PatchAll();
        }
    }
   // public static TRPlugin Instance;

}
