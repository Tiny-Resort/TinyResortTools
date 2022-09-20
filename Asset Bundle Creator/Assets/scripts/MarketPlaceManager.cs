using System.Collections.Generic;
using UnityEngine;

public class MarketPlaceManager : MonoBehaviour
{
	public static MarketPlaceManager manage;

	public int[] marketPos;

	private int currentShopId = -1;

	private List<int> randomVisitorNPCs = new List<int>();

	public int lastVisitor;

	public void Awake()
	{
		manage = this;
	}

	public string getCurrentVisitorsName()
	{
		if (currentShopId > -1)
		{
			return NPCManager.manage.NPCDetails[WorldManager.manageWorld.onTileStatusMap[marketPos[0], marketPos[1]] - 1].NPCName;
		}
		return "no one";
	}

	public bool someoneVisiting()
	{
		if (currentShopId > -1)
		{
			return true;
		}
		return false;
	}

	public void placeMarketStallAndSpawnNPC()
	{
		if (marketPos.Length != 2)
		{
			return;
		}
		int num = currentShopId;
		int num2 = WorldManager.manageWorld.onTileMap[marketPos[0], marketPos[1]];
		if (!NPCManager.manage.npcStatus[1].checkIfHasMovedIn())
		{
			num = 1;
		}
		else if (currentShopId == 2 && !NPCManager.manage.npcStatus[2].checkIfHasMovedIn() && CraftsmanManager.manage.craftsmanHasItemReady())
		{
			num = 2;
		}
		else if (currentShopId < 0)
		{
			if (!NPCManager.manage.npcStatus[9].hasAskedToMoveIn && !NPCManager.manage.npcStatus[9].checkIfHasMovedIn())
			{
				num = 9;
			}
			else if (!checkIfAllNPCHaveMovedIn())
			{
				num = (lastVisitor = getANewVisitor());
			}
			else if (Random.Range(0, 3) == 2)
			{
				num = 8;
			}
		}
		else if (currentShopId == 9 && NPCManager.manage.npcStatus[currentShopId].hasAskedToMoveIn)
		{
			num = -1;
		}
		else if (NPCManager.manage.npcStatus[currentShopId].checkIfHasMovedIn())
		{
			num = -1;
		}
		else if (TownManager.manage.daysInTent == 0)
		{
			num = -1;
		}
		spawnMarketNPC(num);
		currentShopId = num;
		NetworkMapSharer.share.RpcGiveOnTileStatus(num + 1, marketPos[0], marketPos[1]);
		if (num >= 0)
		{
			NetworkMapSharer.share.RpcShowOffBuilding(manage.marketPos[0], manage.marketPos[1]);
		}
	}

	private void spawnMarketNPC(int shopId)
	{
		if (shopId >= 0)
		{
			NPCManager.manage.npcsOnMap.Add(new NPCMapAgent(shopId, 0, 0));
		}
	}

	public void getNpcOnLoad()
	{
		if (marketPos.Length == 2)
		{
			int shopId = (lastVisitor = WorldManager.manageWorld.onTileStatusMap[marketPos[0], marketPos[1]] - 1);
			currentShopId = WorldManager.manageWorld.onTileStatusMap[marketPos[0], marketPos[1]] - 1;
			spawnMarketNPC(shopId);
			checkForSpecialVisitors();
		}
	}

	public void checkForSpecialVisitors()
	{
		if (NPCManager.manage.npcStatus[1].checkIfHasMovedIn() && WeatherManager.manage.raining && BankMenu.menu.accountBalance >= 1000000)
		{
			spawnJimmysBoat();
		}
		else
		{
			despawnJimmiesBoat();
		}
		if (currentShopId < 0)
		{
			if (WorldManager.manageWorld.day == 1 || WorldManager.manageWorld.day == 7)
			{
				if (LicenceManager.manage.allLicences[4].getCurrentLevel() >= 2)
				{
					AnimalManager.manage.trapperCanSpawn = true;
				}
			}
			else
			{
				AnimalManager.manage.trapperCanSpawn = false;
			}
		}
		else
		{
			AnimalManager.manage.trapperCanSpawn = false;
		}
	}

	public bool checkIfAllNPCHaveMovedIn()
	{
		if (NPCManager.manage.npcStatus[0].checkIfHasMovedIn() && NPCManager.manage.npcStatus[4].checkIfHasMovedIn() && NPCManager.manage.npcStatus[7].checkIfHasMovedIn() && NPCManager.manage.npcStatus[2].checkIfHasMovedIn() && NPCManager.manage.npcStatus[3].checkIfHasMovedIn())
		{
			return true;
		}
		return false;
	}

	public int howManyNPCsCanVisit()
	{
		int num = 0;
		for (int i = 0; i < randomVisitorNPCs.Count; i++)
		{
			if (!NPCManager.manage.npcStatus[i].checkIfHasMovedIn() && i != 5)
			{
				num++;
			}
		}
		return num;
	}

	public int getANewVisitor()
	{
		generateAvaliableVisitors();
		if (howManyNPCsCanVisit() == 1)
		{
			for (int i = 0; i < randomVisitorNPCs.Count; i++)
			{
				if (!NPCManager.manage.npcStatus[i].checkIfHasMovedIn())
				{
					return i;
				}
			}
		}
		int num = 0;
		bool flag = false;
		int num2 = 0;
		while (!flag)
		{
			num = randomVisitorNPCs[Random.Range(0, randomVisitorNPCs.Count)];
			if (NPCManager.manage.npcStatus[num].checkIfHasMovedIn())
			{
				continue;
			}
			if (num != lastVisitor && num != 5)
			{
				flag = true;
				continue;
			}
			num2++;
			if (num2 >= 500)
			{
				num = -1;
				break;
			}
		}
		return num;
	}

	public bool noOneVisiting()
	{
		return currentShopId == -1;
	}

	public void spawnJimmysBoat()
	{
		for (int i = 0; i < SaveLoad.saveOrLoad.vehiclesToSave.Count; i++)
		{
			if (SaveLoad.saveOrLoad.vehiclesToSave[i].saveId == 10)
			{
				NPCManager.manage.npcsOnMap.Add(new NPCMapAgent(11, 0, 0));
				return;
			}
		}
		int num = 50 + Random.Range(-35, 35);
		int num2 = 50 + Random.Range(-35, 35);
		bool flag = checkIfChunkCanHaveBoat(num, num2);
		int num3 = 500;
		while (!flag || num3 <= 0)
		{
			num = 50 + Random.Range(-35, 35);
			num2 = 50 + Random.Range(-35, 35);
			flag = checkIfChunkCanHaveBoat(num, num2);
			num3--;
		}
		if (flag)
		{
			GameObject spawnMe = Object.Instantiate(SaveLoad.saveOrLoad.vehiclePrefabs[10], new Vector3(num * 20, 0.4f, num2 * 20), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
			NetworkMapSharer.share.spawnGameObject(spawnMe);
			NPCManager.manage.npcsOnMap.Add(new NPCMapAgent(11, 0, 0));
			NPCMapAgent mapAgentForNPC = NPCManager.manage.getMapAgentForNPC(11);
			mapAgentForNPC.setNewDayDesire();
			mapAgentForNPC.warpNpcInside();
		}
	}

	public void despawnJimmiesBoat()
	{
		for (int i = 0; i < SaveLoad.saveOrLoad.vehiclesToSave.Count; i++)
		{
			if (SaveLoad.saveOrLoad.vehiclesToSave[i].saveId == 10)
			{
				SaveLoad.saveOrLoad.vehiclesToSave[i].destroyServerSelf();
			}
		}
	}

	private bool checkIfChunkCanHaveBoat(int chunkX, int chunkY)
	{
		for (int i = 0; i < WorldManager.manageWorld.getChunkSize(); i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.getChunkSize(); j++)
			{
				if (!WorldManager.manageWorld.waterMap[chunkX * 10 + j, chunkY * 10 + i] || WorldManager.manageWorld.heightMap[chunkX * 10 + j, chunkY * 10 + i] >= 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void generateAvaliableVisitors()
	{
		randomVisitorNPCs.Clear();
		randomVisitorNPCs.Add(2);
		randomVisitorNPCs.Add(4);
		if (TownManager.manage.getCurrentHouseStage() >= 1)
		{
			randomVisitorNPCs.Add(3);
		}
		if (LicenceManager.manage.allLicences[11].hasALevelOneOrHigher())
		{
			randomVisitorNPCs.Add(0);
		}
		if (LicenceManager.manage.allLicences[9].hasALevelOneOrHigher())
		{
			randomVisitorNPCs.Add(7);
		}
		if (NPCManager.manage.getNoOfNPCsMovedIn() >= 4)
		{
			randomVisitorNPCs.Add(8);
		}
	}
}
