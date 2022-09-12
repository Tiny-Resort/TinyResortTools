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

            // Grab List of arguements including mod
            string[] parameters = __instance.chatBox.text.Split(' ');
            __instance.chatBox.text = null;

            // For Debugging
            for (int i = 0; i < parameters.Length; i++) { LeadPlugin.Plugin.Log($"Parameter {i} = {parameters[i]}"); }

            var trigger = parameters[0];
            var command = parameters[1];
            var uniqueKey = trigger.Remove(0, 1) + "_" + command;

            var args = parameters.Skip(2).ToArray();

            // For Debugging
            for (int i = 0; i < args.Length; i++) { LeadPlugin.Plugin.Log($"Parameter {i} = {args[i]}"); }


            // Prob pass in the TRChatCommand class to get help description
            if (command == "help") {
                TRChatCommands.GetHelpDescription(parameters);
                return;
            }
            
            TRChatCommand baseCommand = TRChatCommands.Data[uniqueKey];
            // = TRChatCommands.allCommands.Find((checkCommand) => checkCommand.name.ToLower().Equals(command.ToLower()) || checkCommand.name.ToLower().Contains(command.ToLower()));
            if (baseCommand == null) { TRChatCommands.SendErrorMessage("Unknown command \"" + parameters[1] + "\"."); } // SEND ERROR MESSAGE ChatBoxUtils.SendErrorMessage("Unknown command \"" + parameters[0] + "\"."); }
            else {
                if (args.Length >= 1) { baseCommand.method.Invoke(args); }
            }
            
        }
        
    }

}
