using TMPro;
using UnityEngine;

public class MuseumDisplay : MonoBehaviour
{
	public static MuseumDisplay display;

	[Header("Fish Display spawn locations ------")]
	public GameObject displayFishPrefab;

	public Transform fishTankPos1;

	public Transform fishTankPos2;

	public Transform billabongPos;

	public Transform mangrovePos;

	[Header("Display spawn locations ------")]
	public GameObject bugExhibitPrefab;

	public Transform backTank;

	public Vector2 backTankSize;

	public Transform middleTank;

	public Vector2 middleTankSize;

	public Transform desertTank;

	public Vector2 desertTankSize;

	[Header("Shopping stuff ------")]
	public TextMeshPro bugAmount;

	public TextMeshPro fishAmount;

	public GameObject shopStall1;

	public GameObject shopStall2;

	private int fishDonated;

	private int bugsDonated;

	[Header("Paintings ------")]
	public MuseumPainting[] paintingsOnWall;

	[Header("CurrentlyDisplayed ------")]
	public GameObject[] fishOnDisplay;

	public GameObject[] bugsOnDisplay;

	public GameObject[] underWaterCreaturesOnDisplay;

	private void Start()
	{
		display = this;
		fishOnDisplay = new GameObject[MuseumManager.manage.fishDonated.Length];
		bugsOnDisplay = new GameObject[MuseumManager.manage.bugsDonated.Length];
		underWaterCreaturesOnDisplay = new GameObject[MuseumManager.manage.underWaterCreaturesDonated.Length];
		updateExhibits();
	}

	public void updateExhibits()
	{
		updatePhotoExhibits();
		upDateFishTank();
		upDateBugs();
		upDateUnderWaterCreatures();
		if (fishDonated + bugsDonated >= 10)
		{
			shopStall1.gameObject.SetActive(true);
		}
		else
		{
			shopStall1.gameObject.SetActive(false);
		}
		if (fishDonated + bugsDonated >= 45)
		{
			shopStall2.gameObject.SetActive(true);
		}
		else
		{
			shopStall2.gameObject.SetActive(false);
		}
		fishAmount.text = fishDonated.ToString() ?? "";
		bugAmount.text = bugsDonated.ToString() ?? "";
		MuseumManager.manage.onExhibitUpdate.Invoke();
	}

	public void updatePhotoExhibits()
	{
		for (int i = 0; i < paintingsOnWall.Length; i++)
		{
			paintingsOnWall[i].updatePainting();
		}
	}

	private void upDateFishTank()
	{
		fishDonated = 0;
		for (int i = 0; i < MuseumManager.manage.fishDonated.Length; i++)
		{
			if (MuseumManager.manage.fishDonated[i])
			{
				fishDonated++;
			}
			if (MuseumManager.manage.fishDonated[i] && fishOnDisplay[i] == null)
			{
				createFishDisplayAtPos(i);
			}
		}
	}

	private void upDateBugs()
	{
		bugsDonated = 0;
		for (int i = 0; i < MuseumManager.manage.bugsDonated.Length; i++)
		{
			if (MuseumManager.manage.bugsDonated[i])
			{
				if (MuseumManager.manage.bugsDonated[i])
				{
					bugsDonated++;
				}
				if (MuseumManager.manage.bugsDonated[i] && bugsOnDisplay[i] == null)
				{
					createBugDisplaceAtPos(i);
				}
			}
		}
	}

	private void upDateUnderWaterCreatures()
	{
		for (int i = 0; i < MuseumManager.manage.underWaterCreaturesDonated.Length; i++)
		{
			if (MuseumManager.manage.underWaterCreaturesDonated[i] && MuseumManager.manage.underWaterCreaturesDonated[i] && underWaterCreaturesOnDisplay[i] == null)
			{
				createUnderWaterDisplayAtPos(i);
			}
		}
	}

	public void createFishDisplayAtPos(int pos)
	{
		if (MuseumManager.manage.allFish[pos].fish.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.NorthOcean || MuseumManager.manage.allFish[pos].fish.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.SouthOcean)
		{
			fishOnDisplay[pos] = Object.Instantiate(displayFishPrefab, fishTankPos1);
			fishOnDisplay[pos].GetComponent<FishTankFish>().skinMeshAndSetAnimation(Inventory.inv.getInvItemId(MuseumManager.manage.allFish[pos]), backTankSize);
		}
		else if (MuseumManager.manage.allFish[pos].fish.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.Billabongs)
		{
			fishOnDisplay[pos] = Object.Instantiate(displayFishPrefab, billabongPos);
			fishOnDisplay[pos].GetComponent<FishTankFish>().skinMeshAndSetAnimation(Inventory.inv.getInvItemId(MuseumManager.manage.allFish[pos]), desertTankSize);
		}
		else if (MuseumManager.manage.allFish[pos].fish.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.Mangroves)
		{
			fishOnDisplay[pos] = Object.Instantiate(displayFishPrefab, mangrovePos);
			fishOnDisplay[pos].GetComponent<FishTankFish>().skinMeshAndSetAnimation(Inventory.inv.getInvItemId(MuseumManager.manage.allFish[pos]), desertTankSize);
		}
		else
		{
			fishOnDisplay[pos] = Object.Instantiate(displayFishPrefab, fishTankPos2);
			fishOnDisplay[pos].GetComponent<FishTankFish>().skinMeshAndSetAnimation(Inventory.inv.getInvItemId(MuseumManager.manage.allFish[pos]), middleTankSize);
		}
	}

	public void createUnderWaterDisplayAtPos(int pos)
	{
		if (MuseumManager.manage.allUnderWaterCreatures[pos].underwaterCreature.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.NorthOcean || MuseumManager.manage.allUnderWaterCreatures[pos].underwaterCreature.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.SouthOcean)
		{
			underWaterCreaturesOnDisplay[pos] = Object.Instantiate(bugExhibitPrefab, fishTankPos1);
			underWaterCreaturesOnDisplay[pos].GetComponent<BugExhibit>().placeBugAndShowDisplay(Inventory.inv.getInvItemId(MuseumManager.manage.allUnderWaterCreatures[pos]), backTankSize);
		}
		else if (MuseumManager.manage.allUnderWaterCreatures[pos].underwaterCreature.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.Billabongs)
		{
			underWaterCreaturesOnDisplay[pos] = Object.Instantiate(bugExhibitPrefab, billabongPos);
			underWaterCreaturesOnDisplay[pos].GetComponent<BugExhibit>().placeBugAndShowDisplay(Inventory.inv.getInvItemId(MuseumManager.manage.allUnderWaterCreatures[pos]), desertTankSize);
		}
		else if (MuseumManager.manage.allUnderWaterCreatures[pos].underwaterCreature.mySeason.myWaterLocation[0] == SeasonAndTime.waterLocation.Mangroves)
		{
			underWaterCreaturesOnDisplay[pos] = Object.Instantiate(bugExhibitPrefab, mangrovePos);
			underWaterCreaturesOnDisplay[pos].GetComponent<BugExhibit>().placeBugAndShowDisplay(Inventory.inv.getInvItemId(MuseumManager.manage.allUnderWaterCreatures[pos]), desertTankSize);
		}
		else
		{
			underWaterCreaturesOnDisplay[pos] = Object.Instantiate(bugExhibitPrefab, fishTankPos2);
			underWaterCreaturesOnDisplay[pos].GetComponent<BugExhibit>().placeBugAndShowDisplay(Inventory.inv.getInvItemId(MuseumManager.manage.allUnderWaterCreatures[pos]), middleTankSize);
		}
	}

	public void createBugDisplaceAtPos(int pos)
	{
		if (MuseumManager.manage.allBugs[pos].bug.mySeason.myLandLocation[0] == SeasonAndTime.landLocation.Desert)
		{
			bugsOnDisplay[pos] = Object.Instantiate(bugExhibitPrefab, desertTank);
			bugsOnDisplay[pos].GetComponent<BugExhibit>().placeBugAndShowDisplay(Inventory.inv.getInvItemId(MuseumManager.manage.allBugs[pos]), desertTankSize);
		}
		else if (MuseumManager.manage.allBugs[pos].bug.insectType.name.Contains("Flying") && MuseumManager.manage.allBugs[pos].bug.control == null)
		{
			bugsOnDisplay[pos] = Object.Instantiate(bugExhibitPrefab, middleTank);
			bugsOnDisplay[pos].GetComponent<BugExhibit>().placeBugAndShowDisplay(Inventory.inv.getInvItemId(MuseumManager.manage.allBugs[pos]), middleTankSize);
		}
		else
		{
			bugsOnDisplay[pos] = Object.Instantiate(bugExhibitPrefab, backTank);
			bugsOnDisplay[pos].GetComponent<BugExhibit>().placeBugAndShowDisplay(Inventory.inv.getInvItemId(MuseumManager.manage.allBugs[pos]), backTankSize);
		}
	}
}
