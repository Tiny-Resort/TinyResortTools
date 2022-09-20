using System;
using UnityEngine;

[Serializable]
public class NPCStatus
{
	public int relationshipLevel;

	public int moneySpentAtStore;

	public bool hasMet;

	public bool hasAskedToMoveIn;

	public bool hasMovedIn;

	public bool hasBeenTalkedToToday;

	public bool hasGossipedToday;

	public bool hasHungOutToday;

	public bool acceptedRequest;

	public bool completedRequest;

	public int howManyDaysSincePlayerInteract;

	public int[] last3TextSaid = new int[3];

	public int lastTextSaidSlot;

	public int[] dateMovedIn;

	public bool[] charactersGreatedToday = new bool[4];

	public int[] getLastTextSaid()
	{
		if (last3TextSaid == null || last3TextSaid.Length < 3)
		{
			last3TextSaid = new int[3];
		}
		return last3TextSaid;
	}

	public void addLastTextSaidToList(int newLastSaid)
	{
		last3TextSaid[lastTextSaidSlot] = newLastSaid;
		lastTextSaidSlot++;
		if (lastTextSaidSlot == 3)
		{
			lastTextSaidSlot = 0;
		}
	}

	public void addToRelationshipLevel(int toAdd)
	{
		relationshipLevel = Mathf.Clamp(relationshipLevel + toAdd, 0, 100);
	}

	public void refreshCharactersGreeted()
	{
		charactersGreatedToday = new bool[4];
	}

	public bool checkIfHasBeenGreeted(int characterID)
	{
		return charactersGreatedToday[characterID];
	}

	public void greetCharacter(int characterID)
	{
		charactersGreatedToday[characterID] = true;
	}

	public bool checkIfHasMovedIn()
	{
		return hasMovedIn;
	}

	public void moveInNPC()
	{
		dateMovedIn = new int[4]
		{
			WorldManager.manageWorld.day,
			WorldManager.manageWorld.week,
			WorldManager.manageWorld.month,
			WorldManager.manageWorld.year
		};
		hasMovedIn = true;
	}
}
