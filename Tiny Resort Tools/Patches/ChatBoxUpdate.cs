using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

    [HarmonyPatch(typeof(ChatBox), "Update")]
    public class Update {

        [HarmonyPrefix]
        public static void Prefix(ChatBox __instance) {
            
            if (!__instance.chatOpen || !InputMaster.input.UISelectActiveConfirmButton() || !__instance.chatBox.text.StartsWith("/")) return;

            // Get the parameters entered for the chat command and clear the entry
            string[] parameters = __instance.chatBox.text.Split(' ');
            var trigger = parameters[0].Remove(0, 1);
            __instance.chatBox.text = null;

            // Get the help description for a command
            if (parameters[1] == "help") {
                TRChat.GetHelpDescription(trigger, parameters);
                return;
            }

            // Search for valid commands matching this trigger and command
            List<TRChatCommand> foundCommands = TRChat.GetMatchingCommands(trigger, parameters[1]);

            // If no command is found, this command is invalid
            if (foundCommands.Count <= 0) { 
                TRChat.SendError("\"/" + trigger + " " + parameters[1] + "\" is not a valid command.");
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

            // Call the specified command with the arguments
            var args = parameters.Skip(2).ToArray();
            foundCommands[0].method.Invoke(args);
            
        }
        
    }

}
