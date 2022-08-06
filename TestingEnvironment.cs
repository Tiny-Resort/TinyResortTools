using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TestingEnvironment : BaseUnityPlugin {

        public static TRPlugin Plugin;
        public const string pluginName = "TRTesting";
        public const string pluginGuid = "tinyresort.dinkum." + pluginName;
        public const string pluginVersion = "0.1.0";

        public void Awake() {
            TRTools.Initialize(this, Logger, -1, pluginGuid, pluginName, pluginVersion);
        }

        public void Update() {

            if (Input.GetKeyDown(KeyCode.Alpha0)) { TRTools.TopNotification("Test", "Working"); }
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                //ModWindow.CreateOptionsMenu();
            }
            //if (Input.GetKeyDown(KeyCode.Alpha1)) { Tools.CheckGameVersion(); }
            
        }
        
    }

}
