using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace TinyResort {

    public class TRPlugin {

        public BaseUnityPlugin plugin;
        public Harmony harmony;
        
        public string pluginGuid;
        public string pluginName;
        public string pluginVersion;
        
        public ManualLogSource Logger;
        public ConfigEntry<int> nexusID;
        public ConfigEntry<bool> debugMode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="severity"></param>
        /// <param name="debugModeOnly">If true, this message will only show in the console if the config file has DebugMode set to true.</param>
        public void LogToConsole(string text, LogSeverity severity = LogSeverity.Standard, bool debugModeOnly = true) {
            if (debugModeOnly && !debugMode.Value) return;
            switch (severity) {
                case LogSeverity.Standard: Logger.LogInfo(text); break;
                case LogSeverity.Warning: Logger.LogWarning(text); break;
                case LogSeverity.Error: Logger.LogError(text); break;
            }
        }

        /// <summary>
        /// Allows you to patch methods using only one line of code instead of three per method patched.
        /// </summary>
        /// <param name="sourceClassType">Typically typeOf(className) where className is the class you are patching in Dinkum.</param>
        /// <param name="sourceMethod">The name of the method in Dinkum being patched.</param>
        /// <param name="patchClassType">Typically typeOf(className) where className is the name of your class (the one that contains the patch method).</param>
        /// <param name="patchMethod">The name of the method doing the patching.</param>
        public void QuickPatch(Type sourceClassType, string sourceMethod, Type patchClassType, string patchMethod) {
            MethodInfo sourceMethodInfo = AccessTools.Method(sourceClassType, sourceMethod);
            MethodInfo patchMethodInfo = AccessTools.Method(patchClassType, patchMethod);
            harmony.Patch(sourceMethodInfo, new HarmonyMethod(patchMethodInfo));
        }
        
    }

    public enum LogSeverity { Standard, Warning, Error }

}
