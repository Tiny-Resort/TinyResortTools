using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(ChatBox), "Update")]
    internal class Update {

        [HarmonyPostfix]
        public static void Postfix(ChatBox __instance) {
            if (__instance.chatOpen && !InputMaster.input.UISelectActiveConfirmButton()) {
                if (Input.GetKeyDown(KeyCode.DownArrow)) {
                    var histIndex = (int)AccessTools.Field(typeof(ChatBox), "showingHistoryNo").GetValue(__instance);
                    var histList = (List<string>)AccessTools.Field(typeof(ChatBox), "history").GetValue(__instance);
                    if (histIndex == histList.Count) { __instance.chatBox.text = TRChat.currentChatText; }
                }

                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow)) {
                    __instance.chatBox.caretPosition = __instance.chatBox.text.Length;
                }
            }
        }

        [HarmonyPrefix]
        public static void Prefix(ChatBox __instance) {

            if (!__instance.chatOpen) return;

            if (!InputMaster.input.UISelectActiveConfirmButton()) {
                var histIndex = (int)AccessTools.Field(typeof(ChatBox), "showingHistoryNo").GetValue(__instance);
                var histList = (List<string>)AccessTools.Field(typeof(ChatBox), "history").GetValue(__instance);
                if (histIndex == histList.Count) { TRChat.currentChatText = __instance.chatBox.text; }
                return;
            }
            
            if (!__instance.chatBox.text.StartsWith("/") || __instance.chatBox.text == "/") return;

            // Get the parameters entered for the chat command and clear the entry
            string[] parameters = __instance.chatBox.text.ToLower().Split(' ');
            var trigger = parameters[0].Remove(0, 1);
            __instance.chatBox.text = null;
            
            var args = parameters.Length > 2 ? parameters.Skip(2).ToArray() : new string[0];

            // Get the help description for a command or for a trigger in general
            if (trigger == "help" || parameters.Length == 1 || parameters[1] == "help") {
                TRChat.GetHelpDescription(trigger, args);
                return;
            }

            // Search for valid commands matching this trigger and command
            List<TRChatCommand> foundCommands = TRChat.GetMatchingCommands(trigger, parameters[1]);
            
            if (foundCommands.Count <= 0) {
                if (TRChat.predictedTriggers.Count > 0) {
                    var message = $"Did you mean to use the command: ";
                    for (int i = 0; i < TRChat.predictedTriggers.Count; i++) {
                        message += $"/{TRChat.predictedTriggers[i].trigger} {TRChat.predictedTriggers[i].command}";
                        if (i != TRChat.predictedTriggers.Count - 1) message += ", ";
                    }
                    TRChat.SendError(message);
                }
                else {
                    TRChat.SendError("\"/" + trigger + " " + parameters[1] + "\" is not a valid command.");
                }
                return;
            }

            // If more than one command is found, there is a conflict
            if (foundCommands.Count > 1) {
                string str = "Multiple mods use this trigger and command. Please change the chat trigger used in one of the following mods' config files: ";
                for (var i = 0; i < foundCommands.Count; i++) {
                    str += foundCommands[i].pluginName;
                    if (i < foundCommands.Count - 1) { str += ", "; }
                }
                TRChat.SendError(str);
                return;
            }

            // Call the specified command with the arguments, then display any returned string in a chat bubble
            var returnStr = foundCommands[0].method.Invoke(args);
            if (!string.IsNullOrEmpty(returnStr)) { TRChat.SendMessage(returnStr, foundCommands[0].pluginName); }
            
        }
        
    }

}
