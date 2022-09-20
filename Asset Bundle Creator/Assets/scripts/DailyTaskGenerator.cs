using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyTaskGenerator : MonoBehaviour
{
	public enum genericTaskType
	{
		None = 0,
		Harvest = 1,
		TalkToPeople = 2,
		CatchBugs = 3,
		CraftAnything = 4,
		EatSomething = 5,
		TravelDistance = 6,
		BuyItems = 7,
		SellItems = 8,
		CookMeat = 9,
		PlantSeeds = 10,
		WaterCrops = 11,
		SmashRocks = 12,
		CollectOre = 13,
		SmeltOre = 14,
		GrindObjects = 15,
		ChopDownTree = 16,
		ChopDownStump = 17,
		CutPlanks = 18,
		CatchFish = 19,
		CutGrass = 20,
		HuntAnimals = 21,
		TrapAnAnimal = 22,
		BuyFurniture = 23,
		BuyWallpaper = 24,
		BuyFlooring = 25,
		BuySeeds = 26,
		HarvestCrops = 27,
		BuyShirt = 28,
		CookSomeFruit = 29,
		CookAtCookingTable = 30,
		PlantTreeSeed = 31,
		SellFish = 32,
		SellBugs = 33,
		SellShells = 34,
		SellCrops = 35,
		DigUpTreasure = 36,
		BuryFruit = 37,
		DiveForTreasure = 38,
		PetAnimals = 39,
		FeedAnimals = 40,
		EnterMines = 41,
		DoAFavourSomeone = 42,
		GetPoisoned = 43,
		TakeDamage = 44,
		Faint = 45,
		GetLetters = 46,
		UpgradeHouse = 47,
		PlaceBaseTent = 48,
		PlaceOwnTent = 49,
		CompleteABuilding = 50,
		BuyATool = 51,
		BreakATool = 52,
		CompleteABulletinBoardRequest = 53,
		AddNewBugToPedia = 54,
		AddNewFishToPedia = 55,
		AddNewAnimalToPedia = 56,
		ProcessAnimalProduct = 57,
		SellAnEgg = 58,
		PlantWildSeeds = 59,
		SoilMover = 60,
		CraftATool = 61,
		TravelDistanceOnVehicle = 62,
		GetAHairCut = 63,
		DepositMoneyIntoBank = 64,
		BuyAnAnimal = 65,
		EmptyCrabPot = 66,
		FossileFinder = 67,
		EggTheif = 68,
		HiveFinder = 69,
		CollectHoney = 70,
		PaintVehicle = 71,
		SharkHunter = 72,
		CollectShells = 73,
		BrewSomething = 74,
		TeleportSomewhere = 75,
		UseCompostBin = 76,
		Wheelbarrow = 77,
		LawnMower = 78,
		FlyAHelicopter = 79,
		GetATractor = 80,
		PutBucketOnHead = 81,
		Photographer = 82,
		HangGlide = 83,
		UnlockTreasure = 84,
		MilkAnimal = 85,
		ShearAnimal = 86,
		SellFruit = 87,
		SellAmber = 88
	}

	public static DailyTaskGenerator generate;

	public TaskIcon[] taskIcons;

	public Task[] currentTasks;

	public TileObject[] harvestables;

	private InventoryItem[] seeds;

	public Sprite[] taskSprites;

	private int[] loadedTaskCompletion;

	public List<int> doublesCheck = new List<int>();

	private void Awake()
	{
		generate = this;
	}

	private void Start()
	{
	}

	public void startDistanceChecker()
	{
		StartCoroutine(distanceTracker());
	}

	public void generateFirstDailyTasks()
	{
		doublesCheck.Clear();
		currentTasks = new Task[3];
		for (int i = 0; i < 3; i++)
		{
			currentTasks[i] = new Task(i, 1);
			doublesCheck.Add(currentTasks[i].taskTypeId);
			taskIcons[i].fillWithDetails(currentTasks[i]);
			taskIcons[i].gameObject.SetActive(true);
		}
		loadDailyTaskCompletion();
	}

	private void loadDailyTaskCompletion()
	{
		if (loadedTaskCompletion != null)
		{
			for (int i = 0; i < loadedTaskCompletion.Length; i++)
			{
				currentTasks[i].points = loadedTaskCompletion[i];
				taskIcons[i].fillWithDetails(currentTasks[i]);
			}
		}
		if (loadedTaskCompletion != null)
		{
			for (int j = 0; j < loadedTaskCompletion.Length; j++)
			{
				loadedTaskCompletion[j] = 0;
			}
		}
	}

	public void fillDailyTasksFromLoad(int[] newLoadedPoints)
	{
		loadedTaskCompletion = newLoadedPoints;
	}

	public void generateNewDailyTasks()
	{
		UnityEngine.Random.InitState(NetworkMapSharer.share.mineSeed + NetworkMapSharer.share.tomorrowsMineSeed);
		doublesCheck.Clear();
		currentTasks = new Task[3];
		int taskIdMax = Enum.GetNames(typeof(genericTaskType)).Length;
		for (int i = 0; i < 3; i++)
		{
			currentTasks[i] = new Task(taskIdMax);
			doublesCheck.Add(currentTasks[i].taskTypeId);
			taskIcons[i].fillWithDetails(currentTasks[i]);
			taskIcons[i].gameObject.SetActive(true);
		}
		CurrencyWindows.currency.closeJournal();
		loadDailyTaskCompletion();
	}

	public bool checkIfTaskDoubled(int toCheck)
	{
		return doublesCheck.Contains(toCheck);
	}

	public TileObject generateRandomHarvestable()
	{
		return harvestables[UnityEngine.Random.Range(0, harvestables.Length)];
	}

	public void doATask(genericTaskType taskType, int addAmount = 1)
	{
		if (taskType == genericTaskType.None)
		{
			return;
		}
		MilestoneManager.manage.doATaskAndCountToMilestone(taskType, addAmount);
		if (currentTasks == null)
		{
			return;
		}
		for (int i = 0; i < currentTasks.Length; i++)
		{
			if (currentTasks[i].taskTypeId == (int)taskType)
			{
				currentTasks[i].points += addAmount;
				if (currentTasks[i].points >= currentTasks[i].requiredPoints && !currentTasks[i].completed)
				{
					currentTasks[i].completed = true;
					taskIcons[i].completeTask();
				}
				taskIcons[i].fillWithDetails(currentTasks[i]);
			}
		}
	}

	public void doATaskTileObject(genericTaskType taskType, int tileId, int addAmount = 1)
	{
		if (taskType == genericTaskType.None)
		{
			return;
		}
		MilestoneManager.manage.doATaskAndCountToMilestone(taskType, addAmount);
		if (currentTasks == null)
		{
			return;
		}
		for (int i = 0; i < currentTasks.Length; i++)
		{
			if (currentTasks[i].taskTypeId == (int)taskType && currentTasks[i].tileObjectToInteract.tileObjectId == tileId)
			{
				currentTasks[i].points += addAmount;
				if (currentTasks[i].points >= currentTasks[i].requiredPoints && !currentTasks[i].completed)
				{
					currentTasks[i].completed = true;
					taskIcons[i].completeTask();
				}
				taskIcons[i].fillWithDetails(currentTasks[i]);
			}
		}
	}

	public IEnumerator distanceTracker()
	{
		while (!NetworkMapSharer.share.localChar)
		{
			yield return null;
		}
		Transform charTrans = NetworkMapSharer.share.localChar.transform;
		Vector3 lastPos = charTrans.position;
		float distance = 0f;
		while (true)
		{
			if (charTrans == null)
			{
				yield return null;
			}
			float num = Vector3.Distance(lastPos, charTrans.position);
			if (num > 0.1f && num < 3f)
			{
				distance += num;
			}
			lastPos = charTrans.position;
			if (distance >= 1f)
			{
				if (NetworkMapSharer.share.localChar.myPickUp.drivingVehicle || charTrans.parent != null)
				{
					doATask(genericTaskType.TravelDistanceOnVehicle);
					if ((bool)NetworkMapSharer.share.localChar.myPickUp.currentlyDriving && NetworkMapSharer.share.localChar.myPickUp.currentlyDriving.saveId == 6)
					{
						doATask(genericTaskType.FlyAHelicopter);
					}
				}
				else if (NetworkMapSharer.share.localChar.usingHangGlider)
				{
					doATask(genericTaskType.HangGlide);
				}
				else
				{
					doATask(genericTaskType.TravelDistance);
				}
				distance = 0f;
			}
			yield return null;
		}
	}
}
