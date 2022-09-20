using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Conversation : MonoBehaviour
{
	public enum ConversationSaidBy
	{
		Generic = 0,
		Randomised = 1,
		Shopping = 2,
		Sign = 3,
		Player = 4,
		Npc0 = 5,
		Npc1 = 6,
		Npc2 = 7,
		Npc3 = 8,
		Npc4 = 9,
		Npc5 = 10,
		Npc6 = 11,
		Npc7 = 12,
		Npc8 = 13,
		Npc9 = 14,
		Npc10 = 15,
		Villager = 16,
		RandomisedRequests = 17,
		RandomisedComments = 18,
		Npc11 = 19
	}

	public ConversationSaidBy saidBy;

	[Header("Opening Sentences --------------------------------")]
	public ConversationSequenceAlt startLineAlt;

	[Header("Response options ---------------------------------")]
	public string[] optionNames;

	[Header("Response Sentences -------------------------------")]
	public ConversationSequenceAlt[] responesAlt;

	private static string[] fullstop = new string[1] { ". " };

	private static string[] questionmark = new string[1] { "? " };

	private static string[] exclaim = new string[1] { "! " };

	private static string[] elipse = new string[1] { "... " };

	public string[] getStartLine()
	{
		if ((bool)GetComponent<ConverstationSequence>())
		{
			MonoBehaviour.print(base.name + "has an old conversation componenet");
		}
		if ((string)(LocalizedString)getIntroName(0) != null)
		{
			return getTranslatedIntro();
		}
		if (Application.isEditor && saidBy != 0)
		{
			fillConvoTranslations();
		}
		return startLineAlt.aConverstationSequnce;
	}

	public string[] getResponse(int responseNo)
	{
		if ((bool)GetComponent<ConverstationSequence>())
		{
			MonoBehaviour.print(base.name + "has an old conversation componenet");
		}
		if ((string)(LocalizedString)getResponseName(responseNo, 0) != null)
		{
			return getTranslatedResponse(responseNo);
		}
		return responesAlt[responseNo].aConverstationSequnce;
	}

	public string getOption(int optionNo)
	{
		if ((string)(LocalizedString)getOptionName(optionNo) != null)
		{
			return (LocalizedString)getOptionName(optionNo);
		}
		return optionNames[optionNo];
	}

	public string[] getTranslatedIntro()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < startLineAlt.aConverstationSequnce.Length; i++)
		{
			list.Add((LocalizedString)getIntroName(i));
			if (list[i] == null)
			{
				MonoBehaviour.print("Got a line from untranslated text box");
				list[i] = startLineAlt.aConverstationSequnce[i];
			}
		}
		return list.ToArray();
	}

	public string[] getTranslatedResponse(int responseNo)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < responesAlt[responseNo].aConverstationSequnce.Length; i++)
		{
			list.Add((LocalizedString)getResponseName(responseNo, i));
			if (list[i] == null)
			{
				MonoBehaviour.print("Got a line from untranslated text box");
				list[i] = responesAlt[responseNo].aConverstationSequnce[i];
			}
		}
		return list.ToArray();
	}

	public ConversationManager.specialAction checkSpecialAction(int optionNo)
	{
		if (optionNo == -1)
		{
			return startLineAlt.specialAction;
		}
		return responesAlt[optionNo].specialAction;
	}

	private string[] splitUpIntoSentences(ConversationSequenceAlt splitMeUp)
	{
		string[] toBeSplitUp = returnSplitConvo(splitMeUp.aConverstationSequnce, fullstop);
		toBeSplitUp = returnSplitConvo(toBeSplitUp, questionmark, "?");
		toBeSplitUp = returnSplitConvo(toBeSplitUp, exclaim, "!");
		return returnSplitConvo(toBeSplitUp, elipse, "...");
	}

	private string[] returnSplitConvo(string[] toBeSplitUp, string[] splitAt, string endOfSentenceCharacter = ".")
	{
		List<string> list = new List<string>();
		for (int i = 0; i < toBeSplitUp.Length; i++)
		{
			string[] array = toBeSplitUp[i].Split(splitAt, StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].LastIndexOf('.') != array[j].Length - 1 && array[j].LastIndexOf('?') != array[j].Length - 1 && array[j].LastIndexOf('!') != array[j].Length - 1)
				{
					array[j] += endOfSentenceCharacter;
				}
				list.Add(array[j]);
			}
		}
		return list.ToArray();
	}

	public string getIntroName(int i)
	{
		return saidBy.ToString() + "/" + base.gameObject.name + "_Intro_" + i.ToString("D3");
	}

	public string getOptionName(int i)
	{
		return saidBy.ToString() + "/" + base.gameObject.name + "_Option_" + i.ToString("D3");
	}

	public string getResponseName(int i, int r)
	{
		return saidBy.ToString() + "/" + base.gameObject.name + "_Response_" + i.ToString("D3") + "_" + r.ToString("D3");
	}

	public void fillConvoTranslations()
	{
		string text = saidBy.ToString() + "/";
		for (int i = 0; i < startLineAlt.aConverstationSequnce.Length; i++)
		{
			LocalizationManager.Sources[0].AddTerm(getIntroName(i)).Languages[0] = startLineAlt.aConverstationSequnce[i];
		}
		for (int j = 0; j < optionNames.Length; j++)
		{
			if (!optionNames[j].Contains("<"))
			{
				LocalizationManager.Sources[0].AddTerm(getOptionName(j)).Languages[0] = optionNames[j];
			}
		}
		for (int k = 0; k < responesAlt.Length; k++)
		{
			for (int l = 0; l < responesAlt[k].aConverstationSequnce.Length; l++)
			{
				LocalizationManager.Sources[0].AddTerm(getResponseName(k, l)).Languages[0] = responesAlt[k].aConverstationSequnce[l];
			}
		}
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	public void forceNewFillConvoTranslations()
	{
		string text = saidBy.ToString() + "/";
		for (int i = 0; i < startLineAlt.aConverstationSequnce.Length; i++)
		{
			LocalizationManager.Sources[0].RemoveTerm(getIntroName(i));
			LocalizationManager.Sources[0].AddTerm(getIntroName(i)).Languages[0] = startLineAlt.aConverstationSequnce[i];
		}
		for (int j = 0; j < optionNames.Length; j++)
		{
			if (!optionNames[j].Contains("<"))
			{
				LocalizationManager.Sources[0].RemoveTerm(getOptionName(j));
				LocalizationManager.Sources[0].AddTerm(getOptionName(j)).Languages[0] = optionNames[j];
			}
		}
		for (int k = 0; k < responesAlt.Length; k++)
		{
			for (int l = 0; l < responesAlt[k].aConverstationSequnce.Length; l++)
			{
				LocalizationManager.Sources[0].RemoveTerm(getResponseName(k, l));
				LocalizationManager.Sources[0].AddTerm(getResponseName(k, l)).Languages[0] = responesAlt[k].aConverstationSequnce[l];
			}
		}
		LocalizationManager.Sources[0].UpdateDictionary();
	}
}
