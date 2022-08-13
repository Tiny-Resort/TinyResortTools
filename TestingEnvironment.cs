using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

namespace TinyResort {

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TestingEnvironment : BaseUnityPlugin {

        public static TRPlugin Plugin;
        public const string pluginName = "TRTesting";
        public const string pluginGuid = "tinyresort.dinkum." + pluginName;
        public const string pluginVersion = "0.1.0";
        private static ModData data;
        
        public void Awake() {
            Plugin = TRTools.Initialize(this, Logger, -1, pluginGuid, pluginName, pluginVersion);
            data = TRData.Subscribe(pluginGuid);
        }
        
        public void Update() {

            //Plugin.LogToConsole(TRTools.InMainMenu.ToString(), LogSeverity.Standard, false);

            if (Input.GetKeyDown(KeyCode.F5)) { data.SetValue("testVal", 32); }
            if (Input.GetKeyDown(KeyCode.F6)) { Plugin.LogToConsole(data.GetValue("testVal").ToString(), LogSeverity.Standard, false); }
            if (Input.GetKeyDown(KeyCode.F7)) { Plugin.LogToConsole(((int) data.GetValue("testVal")).ToString(), LogSeverity.Standard, false); }

            if (Input.GetKeyDown(KeyCode.F3)) { TRData.Save(pluginGuid); }

        }
        
    }

}
