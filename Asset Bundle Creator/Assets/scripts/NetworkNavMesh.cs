using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class NetworkNavMesh : NetworkBehaviour
{
	public static NetworkNavMesh nav;

	public GameObject navChunkPrefab;

	public GameObject NavTilePrefab;

	public List<Vector2> lastCharPos = new List<Vector2>();

	public List<Transform> charsConnected = new List<Transform>();

	private List<CharMovement> charMovementConnected = new List<CharMovement>();

	public List<NetworkIdentity> charNetConn = new List<NetworkIdentity>();

	private List<Transform> sleepingChars = new List<Transform>();

	public List<NavChunk> navChunks = new List<NavChunk>();

	public LocalNavMeshBuilder builder;

	public List<NavMeshSourceTag> otherMeshes;

	private int chunkViewDistance = 3;

	private int chunkSize = 10;

	public int animalDistance;

	private bool needsRebuild;

	[SyncVar(hook = "onSleepingAmountChange")]
	private int totalCharsSleeping;

	public UnityEvent checkNavMeshEvent = new UnityEvent();

	private Coroutine navMeshRoutine;

	private int deactivatedDistance = 2;

	public int NetworktotalCharsSleeping
	{
		get
		{
			return totalCharsSleeping;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref totalCharsSleeping))
			{
				int old = totalCharsSleeping;
				SetSyncVar(value, ref totalCharsSleeping, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onSleepingAmountChange(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	private void Awake()
	{
		nav = this;
	}

	private void Start()
	{
		animalDistance = chunkSize * WorldManager.manageWorld.getTileSize() * (chunkViewDistance + 1);
	}

	public override void OnStartServer()
	{
		if (base.isServer)
		{
			chunkSize = WorldManager.manageWorld.getChunkSize();
			animalDistance = chunkSize * WorldManager.manageWorld.getTileSize() * (chunkViewDistance + 1);
			navMeshRoutine = StartCoroutine(handleNavMesh());
			NetworkMapSharer.share.onChangeMaps.AddListener(moveLevels);
			NPCManager.manage.startNPCs();
		}
	}

	public void forceRebuild()
	{
		needsRebuild = true;
	}

	private IEnumerator handleNavMesh()
	{
		while (true)
		{
			checkSleepingList();
			for (int i = 0; i < charsConnected.Count; i++)
			{
				int newCentreChunkX = (int)(Mathf.Round(charsConnected[i].position.x) / 2f) / chunkSize * chunkSize;
				int newCentreChunkY = (int)(Mathf.Round(charsConnected[i].position.z) / 2f) / chunkSize * chunkSize;
				if (newCentreChunkX == (int)lastCharPos[i].x && newCentreChunkY == (int)lastCharPos[i].y)
				{
					continue;
				}
				lastCharPos[i] = new Vector2(newCentreChunkX, newCentreChunkY);
				for (int y = -chunkViewDistance; y < chunkViewDistance; y++)
				{
					for (int x = -chunkViewDistance; x < chunkViewDistance; x++)
					{
						int num = newCentreChunkX + chunkSize * x;
						int num2 = newCentreChunkY + chunkSize * y;
						if (doesPositionNeedNavChunk(num, num2))
						{
							needsRebuild = true;
							placeAFreeNavChunk(num, num2);
							yield return null;
						}
					}
					if (hasPositionChanged(i))
					{
						break;
					}
				}
			}
			if (needsRebuild)
			{
				for (int j = 0; j < otherMeshes.Count; j++)
				{
					otherMeshes[j].refreshBuildOnly();
				}
				returnChunkNotCloseEnough();
				for (int k = 0; k < navChunks.Count; k++)
				{
					if (navChunks[k].active)
					{
						navChunks[k].collect();
					}
				}
				builder.refreshBuildNow();
				needsRebuild = false;
			}
			yield return null;
		}
	}

	public void InstantNavMeshRefresh()
	{
		StopCoroutine(navMeshRoutine);
		navMeshRoutine = StartCoroutine(handleNavMesh());
	}

	private bool hasPositionChanged(int charToCheck)
	{
		int num = (int)(Mathf.Round(charsConnected[charToCheck].position.x) / 2f) / chunkSize * chunkSize;
		int num2 = (int)(Mathf.Round(charsConnected[charToCheck].position.z) / 2f) / chunkSize * chunkSize;
		if (num != (int)lastCharPos[charToCheck].x || num2 != (int)lastCharPos[charToCheck].y)
		{
			lastCharPos[charToCheck] = new Vector2(num, num2);
			return true;
		}
		return false;
	}

	private void returnChunkNotCloseEnough()
	{
		checkNavMeshEvent.Invoke();
		for (int i = 0; i < navChunks.Count; i++)
		{
			if (!navChunks[i].active)
			{
				continue;
			}
			for (int j = 0; j < charsConnected.Count; j++)
			{
				int num = chunkViewDistance * chunkSize + chunkSize;
				if (navChunks[i].showingY < (int)lastCharPos[j].y + num && navChunks[i].showingY > (int)lastCharPos[j].y - num && navChunks[i].showingX < (int)lastCharPos[j].x + num && navChunks[i].showingX > (int)lastCharPos[j].x - num)
				{
					navChunks[i].active = true;
					break;
				}
				navChunks[i].active = false;
			}
		}
	}

	public bool isCloseEnoughToNavChunk(int xPos, int yPos)
	{
		int num = (int)((float)xPos / (float)chunkSize * (float)chunkSize) - 2;
		int num2 = (int)((float)yPos / (float)chunkSize * (float)chunkSize) - 2;
		for (int i = 0; i < charsConnected.Count; i++)
		{
			int num3 = chunkViewDistance * chunkSize;
			if (num2 < (int)lastCharPos[i].y + num3 && num2 > (int)lastCharPos[i].y - num3 && num < (int)lastCharPos[i].x + num3 && num > (int)lastCharPos[i].x - num3)
			{
				return true;
			}
		}
		return false;
	}

	public void placeAFreeNavChunk(int xPos, int yPos)
	{
		NavChunk navChunk = null;
		for (int i = 0; i < navChunks.Count; i++)
		{
			if (!navChunks[i].active)
			{
				navChunk = navChunks[i];
				navChunk.active = true;
				navChunk.placeInPos(xPos, yPos);
				StartCoroutine(placeAnimalDelay(xPos, yPos));
				return;
			}
		}
		navChunk = Object.Instantiate(navChunkPrefab, base.transform).GetComponent<NavChunk>();
		navChunks.Add(navChunk);
		navChunk.placeInPosWithDelay(xPos, yPos);
		StartCoroutine(placeAnimalDelay(xPos, yPos));
	}

	private IEnumerator placeAnimalDelay(int xPos, int yPos)
	{
		yield return null;
		yield return null;
		if (doesPositionHaveNavChunk(xPos, yPos))
		{
			AnimalManager.manage.checkChunkForAnimals(xPos, yPos);
		}
		ReSpawnFarmAnimalsOnChunk(xPos, yPos);
	}

	public void returnAChunk(NavChunk chunkToReturn)
	{
		chunkToReturn.active = false;
	}

	public void updateChunkInUse()
	{
		if (!base.isServer)
		{
			return;
		}
		foreach (NavChunk navChunk in navChunks)
		{
			if (navChunk.active)
			{
				navChunk.placeInPos(navChunk.showingX, navChunk.showingY);
			}
		}
		for (int i = 0; i < otherMeshes.Count; i++)
		{
			otherMeshes[i].refreshBuildOnly();
		}
		builder.refreshBuildNow();
	}

	public bool doesPositionHaveNavChunk(int xPos, int yPos)
	{
		int changeXPos = xPos / chunkSize * chunkSize;
		int changeYPos = yPos / chunkSize * chunkSize;
		return !doesPositionNeedNavChunk(changeXPos, changeYPos);
	}

	public bool isMyNavChunkStillActive(int xPos, int yPos)
	{
		foreach (NavChunk navChunk in navChunks)
		{
			if (navChunk.showingY == xPos && navChunk.showingX == yPos && navChunk.active)
			{
				return true;
			}
		}
		return false;
	}

	public bool npcCheckIfHasNavmesh(int xPos, int yPos)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2), out hit, 5f, -1))
		{
			return true;
		}
		return false;
	}

	public Vector3 getNpcOnNavmeshSpawnPoint(int xPos, int yPos)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2), out hit, 5f, -1))
		{
			return hit.position;
		}
		return new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2);
	}

	public bool doesPositionNeedNavChunk(int changeXPos, int changeYPos)
	{
		if (changeXPos < 0 || changeXPos >= WorldManager.manageWorld.getMapSize() || changeYPos < 0 || changeYPos >= WorldManager.manageWorld.getMapSize())
		{
			return false;
		}
		foreach (NavChunk navChunk in navChunks)
		{
			if (navChunk.active && navChunk.showingY == changeYPos && navChunk.showingX == changeXPos)
			{
				return false;
			}
		}
		return true;
	}

	public void moveLevels()
	{
		StartCoroutine(delayMoveLevels());
	}

	private IEnumerator delayMoveLevels()
	{
		yield return null;
		for (int i = 0; i < lastCharPos.Count; i++)
		{
			lastCharPos[i] = Vector2.zero;
		}
		for (int j = 0; j < navChunks.Count; j++)
		{
			if (navChunks[j].active)
			{
				navChunks[j].placeInPosForceRefresh(navChunks[j].showingX, navChunks[j].showingY);
				StartCoroutine(placeAnimalDelay(navChunks[j].showingX, navChunks[j].showingY));
			}
		}
		needsRebuild = true;
	}

	public void addAPlayer(Transform newChar)
	{
		if (!charsConnected.Contains(newChar))
		{
			charsConnected.Add(newChar);
			charNetConn.Add(newChar.gameObject.GetComponent<NetworkIdentity>());
			charMovementConnected.Add(newChar.gameObject.GetComponent<CharMovement>());
			lastCharPos.Add(Vector2.zero);
			RenderMap.map.trackOtherPlayers(newChar);
		}
		if (charsConnected.Count == 1)
		{
			deactivatedDistance = 2;
		}
		if (charsConnected.Count >= 2)
		{
			deactivatedDistance = 4;
		}
		else
		{
			deactivatedDistance = 10;
		}
	}

	public int getPlayerCount()
	{
		int num = 0;
		for (int i = 0; i < charsConnected.Count; i++)
		{
			if (charsConnected[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	private IEnumerator waitForNameToChange(Transform newChar)
	{
		EquipItemToChar thisChar = newChar.GetComponent<EquipItemToChar>();
		while (thisChar != null && !thisChar.nameHasBeenUpdated)
		{
			yield return null;
		}
		NotificationManager.manage.makeTopNotification(thisChar.playerName + " has joined");
	}

	public void removeAPlayer(Transform removeChar)
	{
		if (charsConnected.Contains(removeChar))
		{
			lastCharPos.RemoveAt(charsConnected.IndexOf(removeChar));
			charNetConn.RemoveAt(charsConnected.IndexOf(removeChar));
			charMovementConnected.RemoveAt(charsConnected.IndexOf(removeChar));
			charsConnected.Remove(removeChar);
			RenderMap.map.unTrackOtherPlayers(removeChar);
		}
		if (charsConnected.Count == 1)
		{
			deactivatedDistance = 2;
		}
		if (charsConnected.Count >= 2)
		{
			deactivatedDistance = 4;
		}
		else
		{
			deactivatedDistance = 10;
		}
	}

	public void SpawnAnAnimalOnTile(int animalSaveId, int xPos, int yPos, AnimalsSaved save = null, int setHealth = 0, uint lastCarrying = 0u)
	{
		int num = Mathf.FloorToInt((float)animalSaveId / 10f);
		int variation = animalSaveId - num * 10;
		AnimalAI animalAI = AnimalManager.manage.spawnFreeAnimal(num, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
		if (!(animalAI == null))
		{
			animalAI.setVariation(variation);
			if (setHealth != 0)
			{
				StartCoroutine(delayHealthSet(animalAI.GetComponent<Damageable>(), setHealth, lastCarrying));
			}
			if (save == null || save.sleepPos == Vector3.zero)
			{
				animalAI.setSleepPos(new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
			}
			else
			{
				animalAI.setSleepPos(save.sleepPos);
			}
			if (num == 1)
			{
				animalAI.GetComponent<FishType>().generateFishForEnviroment();
			}
			if (save != null && (bool)animalAI.saveAsAlpha)
			{
				Debug.Log("Alpha animal days set to " + save.daysRemaining);
				animalAI.saveAsAlpha.daysRemaining = save.daysRemaining;
			}
			NetworkServer.Spawn(animalAI.gameObject);
		}
	}

	private IEnumerator delayHealthSet(Damageable damage, int setHealth, uint lastCarrying)
	{
		yield return null;
		Transform component = NetworkIdentity.spawned[lastCarrying].GetComponent<Transform>();
		damage.attackAndDoDamage(setHealth, component, 0f);
	}

	public bool SpawnFarmAnimal(FarmAnimalDetails farmAnimalDetails, bool animalGrew = false)
	{
		AnimalAI animalAI = AnimalManager.manage.spawnFreeAnimal(farmAnimalDetails.animalId, new Vector3(farmAnimalDetails.currentPosition[0], farmAnimalDetails.currentPosition[1], farmAnimalDetails.currentPosition[2]));
		animalAI.GetComponent<FarmAnimal>().setUpAnimalServer(farmAnimalDetails, animalGrew);
		if (farmAnimalDetails.animalVariation != -1)
		{
			animalAI.GetComponent<AnimalVariation>().setVariation(farmAnimalDetails.animalVariation);
		}
		animalAI.GetComponent<FarmAnimalHarvest>();
		NetworkServer.Spawn(animalAI.gameObject);
		return true;
	}

	public void spawnAFishBreakFree(int fishId, Vector3 position, Transform bobber)
	{
		if (fishId == -2)
		{
			NetworkServer.Spawn(AnimalManager.manage.spawnFreeAnimal(5, position).gameObject);
			return;
		}
		AnimalAI animalAI = AnimalManager.manage.spawnFreeAnimal(1, position);
		NetworkServer.Spawn(animalAI.gameObject);
		animalAI.GetComponent<FishType>().setFishType(fishId, fishId);
		animalAI.GetComponent<FishScript>().fishStartScared(bobber);
	}

	public void spawnSpecificBug(int bugId, Vector3 position)
	{
		AnimalAI animalAI = AnimalManager.manage.spawnFreeAnimal(2, position);
		NetworkServer.Spawn(animalAI.gameObject);
		animalAI.GetComponent<BugTypes>().setUpBug(bugId, bugId);
	}

	public void SpawnAnNPCAtPosition(int id, Vector3 position)
	{
		if (!RealWorldTimeLight.time.underGround)
		{
			NPCAI nPCAI = NPCManager.manage.spawnFreeNPCAtPos(id, position);
			nPCAI.myManager = NPCManager.manage.getNPCMapAgentForNPC(id);
			NetworkServer.Spawn(nPCAI.gameObject);
		}
	}

	public void SpawnAnNPCOnTileAndWarp(int id, int xPos, int yPos, Vector3 warpPos)
	{
		if (!RealWorldTimeLight.time.underGround)
		{
			NPCAI nPCAI = NPCManager.manage.spawnFreeNPC(id, xPos, yPos);
			nPCAI.myManager = NPCManager.manage.getNPCMapAgentForNPC(id);
			nPCAI.transform.position = warpPos;
			NetworkServer.Spawn(nPCAI.gameObject);
		}
	}

	public void SpawnAnNPCFromMapToPlaceInBuilding(int id, Vector3 spawnInPosition)
	{
		NPCAI nPCAI = NPCManager.manage.spawnFreeNPC(id, (int)spawnInPosition.x / 2, (int)spawnInPosition.z / 2);
		nPCAI.moveSpawnSpot(spawnInPosition);
		nPCAI.myManager = NPCManager.manage.getNPCMapAgentForNPC(id);
		NetworkServer.Spawn(nPCAI.gameObject);
	}

	public void UnSpawnNPCDontSaveToMap(NPCAI despawnMe)
	{
		NetworkServer.Destroy(despawnMe.gameObject);
	}

	public void UnSpawnNPCOnTile(NPCAI despawnMe)
	{
		for (int i = 0; i < NPCManager.manage.npcsOnMap.Count; i++)
		{
			if (NPCManager.manage.npcsOnMap[i].npcId == despawnMe.myId.NPCNo)
			{
				NPCManager.manage.npcsOnMap[i].saveNpcToMap(despawnMe.myAgent.transform.position);
				break;
			}
		}
		NetworkServer.Destroy(despawnMe.gameObject);
	}

	public void UnSpawnAnAnimal(AnimalAI despawnMe, bool saveToMap = true)
	{
		if (saveToMap)
		{
			AnimalManager.manage.returnAnimalAndSaveToMap(despawnMe);
		}
		else
		{
			AnimalManager.manage.returnAnimalAndDoNotSaveToMap(despawnMe);
		}
		despawnMe.gameObject.SetActive(false);
		NetworkServer.UnSpawn(despawnMe.gameObject);
	}

	public void UnSpawnFarmAnimal(AnimalAI despawnMe)
	{
		despawnMe.GetComponent<FarmAnimal>().getDetails().setPosition(despawnMe.transform.position);
		despawnMe.gameObject.SetActive(false);
		NetworkServer.UnSpawn(despawnMe.gameObject);
	}

	public void ReSpawnFarmAnimalsOnChunk(int xChunk, int yChunk)
	{
		for (int i = 0; i < FarmAnimalManager.manage.farmAnimalDetails.Count; i++)
		{
			if (FarmAnimalManager.manage.farmAnimalDetails[i].isCurrentlyOnChunk(xChunk, yChunk) && FarmAnimalManager.manage.activeAnimalAgents[FarmAnimalManager.manage.farmAnimalDetails[i].agentListId] != null && !FarmAnimalManager.manage.activeAnimalAgents[FarmAnimalManager.manage.farmAnimalDetails[i].agentListId].gameObject.activeInHierarchy)
			{
				FarmAnimalManager.manage.activeAnimalAgents[FarmAnimalManager.manage.farmAnimalDetails[i].agentListId].gameObject.SetActive(true);
				NetworkServer.Spawn(FarmAnimalManager.manage.activeAnimalAgents[FarmAnimalManager.manage.farmAnimalDetails[i].agentListId].gameObject);
			}
		}
	}

	public bool farAwayFromAllNPCs(float desiredDistance = 200f)
	{
		for (int i = 0; i < nav.charsConnected.Count; i++)
		{
			for (int j = 0; j < NPCManager.manage.npcsOnMap.Count; j++)
			{
				if ((bool)NPCManager.manage.npcsOnMap[j].activeNPC && Vector3.Distance(NPCManager.manage.npcsOnMap[j].activeNPC.transform.position, charsConnected[i].position) < desiredDistance)
				{
					return false;
				}
				if (Vector3.Distance(NPCManager.manage.npcsOnMap[j].currentPosition, charsConnected[i].position) < desiredDistance)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfVectorIsCloseToPlayers(Vector3 positionToCheck)
	{
		for (int i = 0; i < nav.charsConnected.Count; i++)
		{
			if (Vector3.Distance(new Vector3(nav.charsConnected[i].position.x, 0f, nav.charsConnected[i].position.z), new Vector3(positionToCheck.x, 0f, positionToCheck.z)) < (float)(animalDistance - 15))
			{
				return true;
			}
		}
		return false;
	}

	public void checkSleepingList()
	{
		if (getPlayerCount() != 0 && sleepingChars.Count == getPlayerCount() && !RealWorldTimeLight.time.underGround && !TownManager.manage.checkIfInMovingBuildingForSleep())
		{
			WorldManager.manageWorld.nextDay();
			sleepingChars.Clear();
		}
	}

	public void addSleepingChar(Transform trans)
	{
		if (!sleepingChars.Contains(trans))
		{
			sleepingChars.Add(trans);
			NetworktotalCharsSleeping = sleepingChars.Count;
		}
	}

	public bool isCharSleeping(Transform trans)
	{
		return sleepingChars.Contains(trans);
	}

	public void removeSleepingChar(Transform trans)
	{
		if (sleepingChars.Contains(trans))
		{
			sleepingChars.Remove(trans);
			NetworktotalCharsSleeping = sleepingChars.Count;
		}
	}

	private void onSleepingAmountChange(int old, int newAmount)
	{
		NetworktotalCharsSleeping = newAmount;
		if (totalCharsSleeping == charsConnected.Count)
		{
			NotificationManager.manage.createChatNotification("<b><color=purple> Sleeping </color></b>");
			if (charsConnected.Count != 0 && totalCharsSleeping == charsConnected.Count)
			{
				WorldManager.manageWorld.nextDay();
				sleepingChars.Clear();
				NetworktotalCharsSleeping = 0;
			}
		}
		if (totalCharsSleeping != 0)
		{
			NotificationManager.manage.createChatNotification("<b><color=purple> Ready to Sleep </color></b> [" + totalCharsSleeping + "/" + getPlayerCount() + "]");
		}
	}

	public Vector3 checkIfPlaceOnNavMeshForDrop(Vector3 checkPos)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(checkPos, out hit, 2f, -1))
		{
			if (Mathf.Abs(hit.position.y - (float)WorldManager.manageWorld.heightMap[Mathf.RoundToInt((int)hit.position.x / 2), Mathf.RoundToInt(hit.position.z / 2f)]) <= 1.5f)
			{
				Vector3 position = hit.position;
				position.y = WorldManager.manageWorld.heightMap[Mathf.RoundToInt(hit.position.x / 2f), Mathf.RoundToInt(hit.position.z / 2f)];
				return position;
			}
			return hit.position;
		}
		return Vector3.zero;
	}

	public bool isPlayerInDangerNearCamera()
	{
		if ((bool)NetworkMapSharer.share.localChar && NetworkMapSharer.share.localChar.isInDanger())
		{
			return true;
		}
		if (charMovementConnected.Count > 1)
		{
			for (int i = 0; i < charMovementConnected.Count; i++)
			{
				if (charMovementConnected[i].isInDanger() && Vector3.Distance(charsConnected[i].position, CameraController.control.transform.position) < 25f)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(totalCharsSleeping);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(totalCharsSleeping);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = totalCharsSleeping;
			NetworktotalCharsSleeping = reader.ReadInt();
			if (!SyncVarEqual(num, ref totalCharsSleeping))
			{
				onSleepingAmountChange(num, totalCharsSleeping);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = totalCharsSleeping;
			NetworktotalCharsSleeping = reader.ReadInt();
			if (!SyncVarEqual(num3, ref totalCharsSleeping))
			{
				onSleepingAmountChange(num3, totalCharsSleeping);
			}
		}
	}
}
