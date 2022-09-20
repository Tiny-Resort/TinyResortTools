using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprinklerTile : MonoBehaviour
{
	public int horizontalSize;

	public int verticlSize;

	public bool isTank;

	public bool isSilo;

	public Animator anim;

	public TileObject myTileObject;

	public TileObject[] toFill;

	private bool routineRunning;

	private void OnEnable()
	{
		WorldManager.manageWorld.changeDayEvent.AddListener(startSprinkler);
		startSprinkler();
	}

	private void OnDisable()
	{
		WorldManager.manageWorld.changeDayEvent.RemoveListener(startSprinkler);
		routineRunning = false;
	}

	public void waterTiles(int xPos, int yPos, List<int[]> waterTanks)
	{
		if (isSilo)
		{
			if (!NetworkMapSharer.share.isServer)
			{
				return;
			}
			for (int i = -horizontalSize; i < horizontalSize + 1; i++)
			{
				for (int j = -verticlSize; j < verticlSize + 1; j++)
				{
					if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] < 1 || WorldManager.manageWorld.onTileMap[xPos + i, yPos + j] <= -1)
					{
						continue;
					}
					for (int k = 0; k < toFill.Length; k++)
					{
						if (WorldManager.manageWorld.onTileMap[xPos + i, yPos + j] == toFill[k].tileObjectId && WorldManager.manageWorld.onTileStatusMap[xPos + i, yPos + j] == 0)
						{
							NetworkMapSharer.share.RpcGiveOnTileStatus(1, xPos + i, yPos + j);
							WorldManager.manageWorld.onTileStatusMap[xPos, yPos]--;
							MonoBehaviour.print("taking one from silo");
							break;
						}
					}
				}
			}
			NetworkMapSharer.share.RpcGiveOnTileStatus(WorldManager.manageWorld.onTileStatusMap[xPos, yPos], xPos, yPos);
			return;
		}
		bool flag = false;
		for (int l = 0; l < waterTanks.Count; l++)
		{
			int num = 0;
			int num2 = 0;
			if (xPos > waterTanks[l][0])
			{
				num = 1;
			}
			if (yPos > waterTanks[l][1])
			{
				num2 = 1;
			}
			if (Mathf.Abs(waterTanks[l][0] - xPos) <= WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[waterTanks[l][0], waterTanks[l][1]]].sprinklerTile.horizontalSize + num && Mathf.Abs(waterTanks[l][1] - yPos) <= WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[waterTanks[l][0], waterTanks[l][1]]].sprinklerTile.verticlSize + num2)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = 1;
			for (int m = -horizontalSize; m < horizontalSize + 1; m++)
			{
				for (int n = -verticlSize; n < verticlSize + 1; n++)
				{
					if (WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos + m, yPos + n]].wetVersion != -1)
					{
						WorldManager.manageWorld.tileTypeMap[xPos + m, yPos + n] = WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos + m, yPos + n]].wetVersion;
						WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
					}
					if (WorldManager.manageWorld.onTileMap[xPos + m, yPos + n] == -1)
					{
						if (WorldManager.manageWorld.tileTypeMap[xPos + m, yPos + n] == 1)
						{
							WorldManager.manageWorld.onTileMap[xPos + m, yPos + n] = GenerateMap.generate.bushLandGrowBack.objectsInBiom[0].tileObjectId;
							WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
						}
						if (WorldManager.manageWorld.tileTypeMap[xPos + m, yPos + n] == 4)
						{
							WorldManager.manageWorld.onTileMap[xPos + m, yPos + n] = GenerateMap.generate.tropicalGrowBack.objectsInBiom[0].tileObjectId;
							WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
						}
						if (WorldManager.manageWorld.tileTypeMap[xPos + m, yPos + n] == 15)
						{
							WorldManager.manageWorld.onTileMap[xPos + m, yPos + n] = GenerateMap.generate.coldLandGrowBack.objectsInBiom[0].tileObjectId;
							WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((xPos + m) / 10), Mathf.RoundToInt((yPos + n) / 10)] = true;
						}
					}
				}
			}
			if (NetworkMapSharer.share.isServer)
			{
				WorldManager.manageWorld.sprinkerContinuesToWater(xPos, yPos);
			}
		}
		else
		{
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = 0;
		}
	}

	public void startSprinkler()
	{
		if (!isTank && !isSilo && !routineRunning)
		{
			routineRunning = true;
			StartCoroutine(sprinklerAnim());
		}
	}

	private IEnumerator sprinklerAnim()
	{
		while (myTileObject.xPos == 0 && myTileObject.xPos == 0)
		{
			yield return null;
		}
		if (RealWorldTimeLight.time.currentHour >= 1 && RealWorldTimeLight.time.currentHour < 9 && WorldManager.manageWorld.onTileStatusMap[myTileObject.xPos, myTileObject.yPos] == 1)
		{
			anim.SetFloat("Offset", Random.Range(0f, 1f));
			anim.SetBool("SprinklerOn", true);
			while (RealWorldTimeLight.time.currentHour >= 1 && RealWorldTimeLight.time.currentHour < 9)
			{
				yield return null;
			}
			yield return new WaitForSeconds(Random.Range(0f, 1.5f));
			anim.SetBool("SprinklerOn", false);
		}
		routineRunning = false;
		WorldManager.manageWorld.onTileStatusMap[myTileObject.xPos, myTileObject.yPos] = 0;
	}
}
