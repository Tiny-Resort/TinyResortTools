using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    public class TRPlugin {

        public Harmony harmony;
        internal BaseUnityPlugin plugin;
        
        internal ManualLogSource Logger;
        internal ConfigEntry<int> nexusID;
        internal ConfigEntry<bool> debugMode;

        /// <summary>Logs to the BepInEx console.</summary>
        /// <param name="text"></param>
        /// <param name="severity"></param>
        /// <param name="debugModeOnly">If true, this message will only show in the console if the config file has DebugMode set to true.</param>
        public void Log(string text, LogSeverity severity = LogSeverity.Standard, bool debugModeOnly = true) {
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
        /// <param name="prefixMethod">The name of the prefix method doing the patching.</param>
        /// <param name="postfixMethod">The name of the postfix method doing the patching.</param>
        public void QuickPatch(Type sourceClassType, string sourceMethod, Type patchClassType, string prefixMethod, string postfixMethod = "") {
            MethodInfo sourceMethodInfo = AccessTools.Method(sourceClassType, sourceMethod);
            MethodInfo prefixMethodInfo = prefixMethod.IsNullOrWhiteSpace() ? null : AccessTools.Method(patchClassType, prefixMethod);
            MethodInfo postfixMethodInfo = postfixMethod.IsNullOrWhiteSpace() ? null : AccessTools.Method(patchClassType, postfixMethod);
            var prefixHarmonyMethod = prefixMethod.IsNullOrWhiteSpace() ? null : new HarmonyMethod(prefixMethodInfo);
            var postfixHarmonyMethod = postfixMethod.IsNullOrWhiteSpace() ? null : new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(sourceMethodInfo, prefixHarmonyMethod, postfixHarmonyMethod);
        }
        
    }

    public enum LogSeverity { Standard, Warning, Error }

}
