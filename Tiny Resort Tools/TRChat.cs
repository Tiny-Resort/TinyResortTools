using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {

	/// <summary>Tools for sending messages to the chat box.</summary>
	public class TRChat {
		
		internal static string currentChatText = "";

		internal static List<TRChatCommand> allCommands = new List<TRChatCommand>();

		internal static void AddCommand(string pluginName, string trigger, string command, string description, Func<string[], string> method, params string[] argumentNames) {
			allCommands.Add(
				new TRChatCommand {
					pluginName = pluginName, 
					trigger = trigger, 
					command = command, 
					argumentNames = argumentNames,
					helpDescription = description, 
					method = method
				});
		}

		/// <summary>Creates a new chat message with the specified text.</summary>
		/// <param name="message">The text you want shown in the chat message.</param>
		/// <param name="name">The name of the speaker for the chat message. Can be left blank to have no speaker shown.</param>
		public static void SendMessage(string message, string name = "") { SendMessage(Color.white, message, name);  }

		/// <summary>Creates a new chat message with the specified text. As an error, the text will be red.</summary>
		/// <param name="message">The text you want shown in the chat message.</param>
		/// <param name="name">The name of the speaker for the chat message. Can be left blank to use "ERROR" as the name. </param>
		public static void SendError(string message, string name = "ERROR") { SendMessage(Color.red, message, name); }

		/// <summary>Creates a new chat message with the specified text of the specified color.</summary>
		/// <param name="color">The color of the chat message shown.</param>
		/// <param name="message">The text you want shown in the chat message.</param>
		/// <param name="name">The name of the speaker for the chat message. Can be left blank to have no speaker shown.</param>
		public static void SendMessage(Color color, string message, string name = "") {
			string col = ColorUtility.ToHtmlStringRGBA(color);
			ChatBubble chatBubble = UnityEngine.Object.Instantiate(ChatBox.chat.chatBubble, ChatBox.chat.chatBubbleWindow).GetComponent<ChatBubble>();
			var chatMessage = "<color=#" + col + ">" + message + "</color>";
			chatBubble.fillBubble(name, chatMessage);
			ChatBox.chat.chatLog.Add(chatBubble);
			SoundManager.manage.play2DSound(ChatBox.chat.chatSend);
		}

		internal static void GetHelpDescription(string trigger, string[] args) {

			if (trigger == "help") {
				TRChat.SendMessage("For any chat trigger, you can get more information about available commands by typing /tr help (where tr is the mod's trigger found in its config file)");
				TRChat.SendMessage("You can also get info on a particular command and its arguments by typing /tr help [Command]");
				return;
			}

			var foundCommands = GetMatchingCommands(trigger, args.Length > 0 ? args[0] : "");
			
			// If no argument was made, give info on commands usable for this trigger
			if (args.Length <= 0) {

				// Show an error if there are no valid commands for this trigger
				if (foundCommands.Count <= 0) { TRChat.SendError("No commands exist for the specified trigger."); }

				// Otherwise, list all valid commands for this trigger
				else {
					var str = "The following commands exist for /" + trigger + ":\n";
					for (var i = 0; i < foundCommands.Count; i++) {
						str += "<color=orange>" + foundCommands[i].command + "</color>";
						if (i < foundCommands.Count - 1) { str += ", "; }
					}
					TRChat.SendMessage(str);
				}
				
				return;
				
			}

			// If an argument was given but no command was found matching it, show an error
			if (foundCommands.Count <= 0) {
				TRChat.SendError("The specified command does not exist for the specified trigger.");
				return;
			}
			
			// Show help info for each matching command
			if (foundCommands.Count > 1) { TRChat.SendMessage("Multiple matching commands exist. Info on each below."); }
			foreach (var chatCommand in foundCommands) {
				var str = "<color=orange>" + chatCommand.pluginName + "</color>\n" 
				        + "/" + chatCommand.trigger + " " + chatCommand.command + " ";
				foreach (var arg in chatCommand.argumentNames) { str += "[" + arg + "] "; }
				TRChat.SendMessage(str + "\n<i><color=#A1A1A1FF>" + chatCommand.helpDescription + "</color></i>");
			}

		}

		// Find all commands with the specified trigger and command
		internal static List<TRChatCommand> GetMatchingCommands(string trigger, string command = "") {
			List<TRChatCommand> foundCommands = new List<TRChatCommand>();
			foreach (var chatCommand in TRChat.allCommands)
				if (chatCommand.trigger == trigger && (string.IsNullOrEmpty(command) || chatCommand.command == command)) { foundCommands.Add(chatCommand); }
			return foundCommands;
		}

	}

	internal class TRChatCommand {
		public string pluginName;
		public string trigger;
		public string command;
		public string[] argumentNames;
		public string helpDescription;
		public Func<string[], string> method;
	}
	
}
