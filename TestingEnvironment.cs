using BepInEx;
using UnityEngine;

namespace TR {

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TestingEnvironment : BaseUnityPlugin {

        public const string pluginGuid = "tinyresort.dinkum.tinyresorttools";
        public const string pluginName = "Tiny Resort Tools";
        public const string pluginVersion = "0.1.0";

        public void Update() {

            if (Input.GetKeyDown(KeyCode.Alpha0)) { Tools.Notify("Test", "Working"); }
            //if (Input.GetKeyDown(KeyCode.Alpha1)) { Tools.CheckGameVersion(); }
            
        }
        
    }

}
