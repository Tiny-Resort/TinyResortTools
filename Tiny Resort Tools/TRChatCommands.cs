using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace TinyResort {

	public class TRChatCommands {
		
		internal static Dictionary<string, TRChatCommand> Data = new Dictionary<string, TRChatCommand>();

		internal static void Initialize() { }

		/// <summary>Subscribes to the save system so that your mod data is saved and loaded properly.</summary>
		/// <param name="trigger">Mod-specific command for triggering chat commands.</param>
		/// <param name="command">Subcommand to run.</param>
		/// <param name="description">Description to show if the user runs the help command.</param>
		/// <param name="method">Method to run when chat command is done.</param>
		/// <returns>A reference to all the commands for your mod.</returns>
		public static void CreateCommand(string trigger, string command, string description, Action<string[]> method) {
			string uniqueKey = trigger + "_" +  command;
			Data[uniqueKey] = new TRChatCommand {
				trigger = trigger,
				name = command,
				helpDescription = description,
				method = method
			};
		}

		public static void SendMessage(string message, string name = "") {
			ChatBubble component = UnityEngine.Object.Instantiate(ChatBox.chat.chatBubble, ChatBox.chat.chatBubbleWindow).GetComponent<ChatBubble>();
			component.fillBubble(name, message);
			ChatBox.chat.chatLog.Add(component);
			SoundManager.manage.play2DSound(ChatBox.chat.chatSend);
		}

		public static void SendErrorMessage(string message, string name = "") {
			ChatBubble component = UnityEngine.Object.Instantiate(ChatBox.chat.chatBubble, ChatBox.chat.chatBubbleWindow).GetComponent<ChatBubble>();
			var errorMessage = "<color=red>" + message + "</color>";
			component.fillBubble(name, errorMessage);
			ChatBox.chat.chatLog.Add(component);
			SoundManager.manage.play2DSound(ChatBox.chat.chatSend);
		}

		internal static void GetHelpDescription(string[] args) {
			if (args.Length <= 0) { return; }
			var methodName = args[0];

		}

		internal static void ReadChat(string chatText) {

			var parts = chatText.Split(' ');

			// TODO: Get Mod
			// parts[0]

			// TODO: Get command
			// parts[1]

			// TODO: Invoke command with remaining arguments
			// parts[2+]
			//allCommands[0].method.Invoke(remainingParts);

		}

		internal static void ChatMessage() { }
	}

	public class TRChatCommand {
		public string trigger;
		public string name;
		public string helpDescription;
		public Action<string[]> method;
		
	}
}
#region commentedout
	/*

	public static void Initialize() { TRTools.QuickPatch(typeof(ChatBox), "Update", typeof(TRChatCommands), "UpdatePrefix"); }

	[HarmonyPrefix]
	internal static void UpdatePrefix(ChatBox __instance) {

		if (!__instance.chatOpen || !InputMaster.input.UISelectActiveConfirmButton() || !__instance.chatBox.text.StartsWith("/")) return;
		
		List<string> parameters = __instance.chatBox.text.Remove(0, CommandPrefix.get_Value().Length).Split(' ').ToList();
		
		__instance.chatBox.text = null;
		if (!parameters.Any()) return;
		
		BaseCommand baseCommand = allCommands.Find((command) => command.Name.ToLower().Equals(parameters[0].ToLower()) || command.Name.ToLower().Contains(parameters[0].ToLower()));
		if (baseCommand == null) { ChatBoxUtils.SendErrorMessage("Unknown command \"" + parameters[0] + "\"."); }
		else {
			if (!NetworkMapSharer.share.isServer && baseCommand.IsCheat)
			{
				return;
			}
			if (parameters.Count > 1 && baseCommand.SubCommands.Count > 0)
			{
				SubCommand? subCommand2 = baseCommand.SubCommands.Find((SubCommand? subCommand) => subCommand.HasValue && (subCommand.Value.Name.ToLower().Equals(parameters[1].ToLower()) || subCommand.Value.Name.ToLower().Contains(parameters[1].ToLower())));
				if (subCommand2.HasValue)
				{
					if (NetworkMapSharer.share.isServer || !subCommand2.Value.IsCheat)
					{
						parameters.RemoveRange(0, 2);
						subCommand2.Value.Handler(parameters);
					}
					return;
				}
			}
			parameters.RemoveRange(0, 1);
			baseCommand.HandleCommand(parameters);
		}
	}

	public abstract class BaseCommand {
		
		public string Name { get; set; }
		public string Description { get; set; }
		public string Category { get; set; }
		public bool IsCheat { get; set; }

		public Dictionary<string, (Type, bool)> Arguments { get; set; }
		public List<SubCommand?> SubCommands { get; set; } = new List<SubCommand?>();

		protected BaseCommand(string name, string description, string category = null, bool isCheat = false) {
			Name = name;
			Description = description;
			Category = category ?? "Unknown";
			IsCheat = isCheat;
		}

		public virtual void HandleCommand(List<string> parameters) { ChatBoxUtils.SendErrorMessage("Use <color=yellow>" + Plugin.CommandPrefix.get_Value() + "help " + Name + "</color> to get help about this command."); }
		
	}

	public struct SubCommand {
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsCheat { get; set; }
		public Dictionary<string, (Type, bool)> Arguments { get; set; }
		public Action<List<string>> Handler { get; set; }
	}

	public static class ChatBoxUtils {
		
		private static ChatBubble CreateMessageBubble(string message, string name = null) {
			ChatBubble component = UnityEngine.Object.Instantiate(ChatBox.chat.chatBubble, ChatBox.chat.chatBubbleWindow).GetComponent<ChatBubble>();
			if (string.IsNullOrEmpty(name)) {
				IEnumerator routine = (IEnumerator)Traverse.Create((object)component).Method("killSelf", Array.Empty<object>()).GetValue();
				IEnumerator routine2 = (IEnumerator)Traverse.Create((object)component).Method("fadeBackgroundAndText", new object[1] { true }).GetValue();
				component.contents.text = message;
				component.StartCoroutine(routine);
				component.StartCoroutine(routine2);
			}
			else { component.fillBubble(name, message); }
			return component;
		}

		public static void SendMessage(string message) {
			ChatBubble item = CreateMessageBubble(message);
			ChatBox.chat.chatLog.Add(item);
			SoundManager.manage.play2DSound(ChatBox.chat.chatSend);
		}

		public static void SendErrorMessage(string message) {
			ChatBubble item = CreateMessageBubble("<color=red>" + message + "</color>");
			ChatBox.chat.chatLog.Add(item);
			SoundManager.manage.play2DSound(ChatBox.chat.chatSend);
		}
		
	}

	public class BarbershopCommand : BaseCommand {
		public BarbershopCommand()
			: base("barbershop", "Want a fresh haircut?", "Utility") {
			base.Arguments = new Dictionary<string, (Type, bool)> { { "colour", (typeof(bool), false) } };
		}

		public override void HandleCommand(List<string> parameters) {
			bool colorSelector = false;
			if (parameters.Count > 0) { colorSelector = !string.IsNullOrEmpty(parameters[0]); }
			HairDresserMenu.menu.openHairCutMenu(colorSelector);
		}
	}
	*/
	#endregion
