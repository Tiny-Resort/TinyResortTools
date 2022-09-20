using UnityEngine;

public class Task
{
	public int points;

	public int requiredPoints;

	public int taskTypeId;

	public int specificNPC = -1;

	public int reward = 25;

	public TileObject tileObjectToInteract;

	public string missionText = "";

	public bool completed;

	public Task(int taskIdMax)
	{
		generateTask(taskIdMax);
	}

	public Task(int firstDailyTaskNo, int taskIdMax)
	{
		if (firstDailyTaskNo == 0)
		{
			taskTypeId = 1;
			tileObjectToInteract = DailyTaskGenerator.generate.harvestables[0];
			requiredPoints = 3;
			if ((bool)tileObjectToInteract.tileObjectGrowthStages.harvestDrop)
			{
				missionText = "Harvest " + requiredPoints + " " + tileObjectToInteract.tileObjectGrowthStages.harvestDrop.getInvItemName();
			}
			else
			{
				missionText = "Harvest " + tileObjectToInteract.name;
			}
			reward = 10 * requiredPoints;
		}
		if (firstDailyTaskNo == 1)
		{
			taskTypeId = 3;
			requiredPoints = 2;
			missionText = "Catch " + requiredPoints + " Bugs";
			reward = 25 * requiredPoints;
		}
		if (firstDailyTaskNo == 2)
		{
			taskTypeId = 4;
			requiredPoints = 1;
			missionText = "Craft " + requiredPoints + " Items";
			reward = 25 * requiredPoints;
		}
	}

	public void generateTask(int taskIdMax)
	{
		specificNPC = -1;
		bool flag = false;
		while (!flag)
		{
			taskTypeId = Random.Range(1, taskIdMax);
			if (1 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 6);
				tileObjectToInteract = DailyTaskGenerator.generate.generateRandomHarvestable();
				requiredPoints *= tileObjectToInteract.tileObjectGrowthStages.harvestSpots.Length;
				if ((bool)tileObjectToInteract.tileObjectGrowthStages.harvestDrop)
				{
					missionText = "Harvest " + requiredPoints + " " + tileObjectToInteract.tileObjectGrowthStages.harvestDrop.getInvItemName();
				}
				else
				{
					missionText = "Harvest " + tileObjectToInteract.name;
				}
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (2 == taskTypeId && NPCManager.manage.getNoOfNPCsMovedIn() > 0)
			{
				int noOfNPCsMovedIn = NPCManager.manage.getNoOfNPCsMovedIn();
				requiredPoints = Random.Range(1, 4);
				Mathf.Clamp(requiredPoints, 1, noOfNPCsMovedIn);
				missionText = "Chat with " + requiredPoints + " residents";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (37 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 5);
				missionText = "Bury " + requiredPoints + " Fruit";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (73 == taskTypeId)
			{
				requiredPoints = Random.Range(5, 10);
				missionText = "Collect " + requiredPoints + " Shells";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.manageWorld.day != 1 && 34 == taskTypeId)
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = Random.Range(5, 10);
					missionText = "Sell " + requiredPoints + " Shells";
					reward = 10 * requiredPoints;
					flag = true;
				}
			}
			else if (WorldManager.manageWorld.day != 1 && 87 == taskTypeId)
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = Random.Range(3, 5);
					missionText = "Sell " + requiredPoints + " Fruit";
					reward = 10 * requiredPoints;
					flag = true;
				}
			}
			else if (42 == taskTypeId)
			{
				requiredPoints = 1;
				missionText = "Do a job for someone";
				reward = 100;
				flag = true;
			}
			else if (59 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 5);
				missionText = "Plant " + requiredPoints + " Wild Seeds";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (60 == taskTypeId && LicenceManager.manage.allLicences[16].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 5);
				missionText = "Dig up dirt " + requiredPoints + " times";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (3 == taskTypeId)
			{
				requiredPoints = Random.Range(3, 5);
				missionText = "Catch " + requiredPoints + " Bugs";
				reward = 20 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.manageWorld.day != 1 && 33 == taskTypeId)
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = Random.Range(3, 5);
					missionText = "Sell " + requiredPoints + " Bugs";
					reward = 20 * requiredPoints;
					flag = true;
				}
			}
			else if (4 == taskTypeId)
			{
				requiredPoints = Random.Range(2, 4);
				missionText = "Craft " + requiredPoints + " Items";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (5 == taskTypeId)
			{
				requiredPoints = 1;
				missionText = "Eat something";
				reward = 10;
				flag = true;
			}
			else if (WorldManager.manageWorld.day != 1 && 8 == taskTypeId && !NPCManager.manage.NPCDetails[1].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
			{
				requiredPoints = Random.Range(2, 6);
				requiredPoints *= 1000;
				missionText = "Make " + requiredPoints + " " + Inventory.inv.moneyItem.getInvItemName();
				reward = 100 * (requiredPoints / 1000);
				flag = true;
			}
			else if (7 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 4);
				requiredPoints *= 1000;
				missionText = "Spend " + requiredPoints + " " + Inventory.inv.moneyItem.getInvItemName();
				reward = 100 * (requiredPoints / 1000);
				flag = true;
			}
			else if (6 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 3);
				requiredPoints *= 500 / Random.Range(1, 2);
				missionText = "Travel " + requiredPoints + "m on foot.";
				reward = 50;
				flag = true;
			}
			else if (62 == taskTypeId && LicenceManager.manage.allLicences[7].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(1, 3);
				requiredPoints *= 500 / Random.Range(1, 2);
				missionText = "Travel " + requiredPoints + "m by vehicle";
				reward = 50;
				flag = true;
			}
			else if (9 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 3);
				missionText = "Cook " + requiredPoints + " meat";
				reward = 20;
				flag = true;
			}
			else if (29 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 3);
				missionText = "Cook " + requiredPoints + " fruit";
				reward = 20;
				flag = true;
			}
			else if (30 == taskTypeId)
			{
				requiredPoints = 1;
				missionText = "Cook something at the Cooking table";
				reward = 100;
				flag = true;
			}
			else if (31 == taskTypeId)
			{
				requiredPoints = Random.Range(1, 4);
				missionText = "Plant " + requiredPoints + " tree seeds";
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (30 == taskTypeId)
			{
				requiredPoints = 1;
				missionText = "Cook something at the Cooking table";
				reward = 100;
				flag = true;
			}
			else if (10 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 7);
				missionText = "Plant " + requiredPoints + "crop seeds";
				reward = 45 * requiredPoints;
				flag = true;
			}
			else if (11 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher() && !WeatherManager.manage.raining)
			{
				requiredPoints = Random.Range(3, 7);
				missionText = "Water " + requiredPoints + " crops";
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (12 == taskTypeId && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 7);
				missionText = "Smash " + requiredPoints + " rocks";
				reward = 15 * requiredPoints;
				flag = true;
			}
			else if (13 == taskTypeId && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 7);
				missionText = "Smash " + requiredPoints + " ore rocks";
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (14 == taskTypeId && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(1, 3);
				missionText = "Smelt some ore into a bar";
				reward = 50 * requiredPoints;
				flag = true;
			}
			else if (15 == taskTypeId && NPCManager.manage.npcStatus[1].checkIfHasMovedIn() && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 4);
				missionText = "Grind " + requiredPoints + " stones";
				reward = 15 * requiredPoints;
				flag = true;
			}
			else if (16 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 5);
				missionText = "Cut down " + requiredPoints + " trees";
				reward = 15 * requiredPoints;
				flag = true;
			}
			else if (17 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 5);
				missionText = "Clear " + requiredPoints + " tree stumps";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (18 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(5, 9);
				missionText = "Saw " + requiredPoints + " planks";
				reward = 5 * requiredPoints;
				flag = true;
			}
			else if (19 == taskTypeId && LicenceManager.manage.allLicences[3].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(3, 5);
				missionText = "Catch " + requiredPoints + " Fish";
				reward = 35 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.manageWorld.day != 1 && 32 == taskTypeId && LicenceManager.manage.allLicences[3].hasALevelOneOrHigher())
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = Random.Range(3, 5);
					missionText = "Sell " + requiredPoints + " Fish";
					reward = 35 * requiredPoints;
					flag = true;
				}
			}
			else if (20 == taskTypeId && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(8, 15);
				missionText = "Clear " + requiredPoints + " grass";
				reward = 25;
				flag = true;
			}
			else if (39 == taskTypeId && FarmAnimalManager.manage.isThereAtleastOneActiveAgent())
			{
				requiredPoints = 1;
				missionText = "Pet an animal";
				reward = 10;
				flag = true;
			}
			else if (28 == taskTypeId && (bool)TownManager.manage.allShopFloors[6])
			{
				if (!NPCManager.manage.NPCDetails[4].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = 1;
					missionText = "Buy some new clothes";
					reward = 25;
					flag = true;
				}
			}
			else if (23 == taskTypeId && (bool)TownManager.manage.allShopFloors[10])
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = 1;
					missionText = "Buy some new furniture";
					reward = 25;
					flag = true;
				}
			}
			else if (24 == taskTypeId && (bool)TownManager.manage.allShopFloors[10])
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = 1;
					missionText = "Buy some new wallpaper";
					reward = 25;
					flag = true;
				}
			}
			else if (25 == taskTypeId && (bool)TownManager.manage.allShopFloors[10])
			{
				if (!NPCManager.manage.NPCDetails[3].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = 1;
					missionText = "Buy some new flooring";
					reward = 25;
					flag = true;
				}
			}
			else if (WorldManager.manageWorld.getNoOfCompletedCrops() >= 4 && 27 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
			{
				requiredPoints = Mathf.Clamp(Random.Range(4, 11), 4, WorldManager.manageWorld.getNoOfCompletedCrops());
				missionText = "Harvest " + requiredPoints + " crops";
				reward = 45 * requiredPoints;
				flag = true;
			}
			else if (WorldManager.manageWorld.day != 1 && WorldManager.manageWorld.getNoOfCompletedCrops() >= 4 && 35 == taskTypeId && LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
			{
				requiredPoints = Mathf.Clamp(Random.Range(4, 11), 4, WorldManager.manageWorld.getNoOfCompletedCrops());
				missionText = "Sell " + requiredPoints + " crops";
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (76 == taskTypeId && LicenceManager.manage.allLicences[11].getCurrentLevel() >= 2)
			{
				requiredPoints = 1;
				missionText = "Compost something";
				reward = 25;
				flag = true;
			}
			else if (61 == taskTypeId)
			{
				requiredPoints = 1;
				missionText = "Craft a new tool";
				reward = 100;
				flag = true;
			}
			else if (26 == taskTypeId && (bool)TownManager.manage.allShopFloors[11])
			{
				if (!NPCManager.manage.NPCDetails[0].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
				{
					requiredPoints = Random.Range(3, 7);
					missionText = "Buy " + requiredPoints + " seeds";
					reward = 5 * requiredPoints;
					flag = true;
				}
			}
			else if (22 == taskTypeId && LicenceManager.manage.allLicences[15].hasALevelOneOrHigher())
			{
				requiredPoints = 1;
				missionText = "Trap an animal and deliver it";
				reward = 100;
				flag = true;
			}
			else if (21 == taskTypeId && LicenceManager.manage.allLicences[4].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(2, 6);
				missionText = "Hunt " + requiredPoints + " animals";
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (51 == taskTypeId && !NPCManager.manage.NPCDetails[1].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
			{
				requiredPoints = 1;
				missionText = "Buy a new tool";
				reward = 10 * requiredPoints;
				flag = true;
			}
			else if (Inventory.inv.checkIfToolNearlyBroken() && 52 == taskTypeId)
			{
				requiredPoints = 1;
				missionText = "Break a tool";
				reward = 25 * requiredPoints;
				flag = true;
			}
			else if (36 == taskTypeId && LicenceManager.manage.allLicences[6].hasALevelOneOrHigher())
			{
				requiredPoints = Random.Range(1, 3);
				missionText = "Find some burried treasure";
				reward = 30 * requiredPoints;
				flag = true;
			}
			else
			{
				missionText = "No mission set";
				flag = false;
			}
			if (flag && DailyTaskGenerator.generate.checkIfTaskDoubled(taskTypeId))
			{
				flag = false;
			}
		}
	}
}
