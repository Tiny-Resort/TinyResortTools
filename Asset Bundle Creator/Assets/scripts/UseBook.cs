using System.Collections;
using UnityEngine;

public class UseBook : MonoBehaviour
{
	public bool isBugBook;

	public bool isFishBook;

	public bool isPlantBook;

	public bool isJournal;

	public bool isMachineManual;

	private bool plantBookOpen;

	public NameTag myNameTag;

	public ASound openBookSound;

	public ASound closeBookSound;

	private bool bookOpen;

	private CharMovement myChar;

	private void Start()
	{
		if (isJournal)
		{
			GetComponent<Animator>().SetBool("IsJournal", true);
		}
		if (isPlantBook)
		{
			StartCoroutine(plantBookRoutine());
		}
		myChar = base.transform.GetComponentInParent<CharMovement>();
	}

	public void openBook()
	{
		if (!bookOpen)
		{
			bookOpen = true;
			SoundManager.manage.playASoundAtPoint(openBookSound, base.transform.position);
		}
		if (!isJournal && (bool)myChar && myChar.isLocalPlayer)
		{
			if (isMachineManual)
			{
				BookWindow.book.openBook();
			}
			if (isPlantBook)
			{
				plantBookOpen = true;
			}
			if (isBugBook && !AnimalManager.manage.bugBookOpen)
			{
				AnimalManager.manage.bugBookOpen = true;
				AnimalManager.manage.lookAtBugBook.Invoke();
			}
			if (isFishBook && !AnimalManager.manage.fishBookOpen)
			{
				AnimalManager.manage.fishBookOpen = true;
				AnimalManager.manage.lookAtFishBook.Invoke();
			}
		}
	}

	public void closeBook()
	{
		if (bookOpen)
		{
			bookOpen = false;
			SoundManager.manage.playASoundAtPoint(closeBookSound, base.transform.position);
		}
		if (!isJournal && (bool)myChar && myChar.isLocalPlayer)
		{
			if (isMachineManual)
			{
				BookWindow.book.closeBook();
			}
			if (isPlantBook)
			{
				plantBookOpen = false;
			}
			if (isBugBook && AnimalManager.manage.bugBookOpen)
			{
				AnimalManager.manage.bugBookOpen = false;
				AnimalManager.manage.lookAtBugBook.Invoke();
			}
			if (isFishBook && AnimalManager.manage.fishBookOpen)
			{
				AnimalManager.manage.fishBookOpen = false;
				AnimalManager.manage.lookAtFishBook.Invoke();
			}
		}
	}

	private void OnDestroy()
	{
		closeBook();
	}

	private IEnumerator plantBookRoutine()
	{
		int lastXpos = -1;
		int lastYpos = -1;
		while (true)
		{
			if (plantBookOpen)
			{
				int num = Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position.x / 2f);
				int num2 = Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position.z / 2f);
				if (lastXpos != num || lastYpos != num2)
				{
					if (WorldManager.manageWorld.onTileMap[num, num2] >= 0 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num, num2]].tileObjectGrowthStages && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num, num2]].tileObjectGrowthStages.isPlant && !WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num, num2]].tileObjectGrowthStages.normalPickUp)
					{
						TileObjectGrowthStages tileObjectGrowthStages = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num, num2]].tileObjectGrowthStages;
						if ((bool)tileObjectGrowthStages.harvestDrop)
						{
							BookWindow.book.objectTitle.text = tileObjectGrowthStages.harvestDrop.getInvItemName() + " Plant";
							if (tileObjectGrowthStages.objectStages.Length - WorldManager.manageWorld.onTileStatusMap[num, num2] - 1 == 0)
							{
								BookWindow.book.openPlantBook("Ready for harvest");
							}
							else
							{
								BookWindow.book.openPlantBook("Mature in:\n" + (tileObjectGrowthStages.objectStages.Length - WorldManager.manageWorld.onTileStatusMap[num, num2] - 1) + " days.");
							}
						}
						else
						{
							BookWindow.book.objectTitle.text = "Plant";
							if (tileObjectGrowthStages.objectStages.Length - WorldManager.manageWorld.onTileStatusMap[num, num2] - 1 == 0)
							{
								BookWindow.book.openPlantBook("");
							}
							else
							{
								BookWindow.book.openPlantBook("Mature in:\n" + (tileObjectGrowthStages.objectStages.Length - WorldManager.manageWorld.onTileStatusMap[num, num2] - 1) + " days");
							}
						}
					}
					else
					{
						BookWindow.book.closeBook();
					}
				}
				yield return null;
			}
			else
			{
				BookWindow.book.closeBook();
				while (!plantBookOpen)
				{
					yield return null;
				}
			}
		}
	}
}
