using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BuriedManager : MonoBehaviour
{
	public static BuriedManager manage;

	public List<BuriedItem> allBuriedItems = new List<BuriedItem>();

	public InventoryItemLootTable randomDrops;

	public InventoryItemLootTable oreDrops;

	public InventoryItem[] oreDropItems;

	public InventoryItem[] normalBarrelDrops;

	public InventoryItemLootTable barrelDrops;

	public InventoryItemLootTable wheelieBinDrops;

	public InventoryItemLootTable shellDrops;

	public InventoryItemLootTable veryCommonDrops;

	public TileObject oldBarrel;

	public TileObject wheelieBin;

	public GameObject amberChunk;

	private void Awake()
	{
		manage = this;
	}

	public void Start()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		List<InventoryItem> list2 = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].relic)
			{
				list.Add(Inventory.inv.allItems[i]);
				if (Inventory.inv.allItems[i].relic.myseason.myRarity >= SeasonAndTime.rarity.Uncommon)
				{
					list2.Add(Inventory.inv.allItems[i]);
				}
			}
		}
		randomDrops.autoFillFromArray(list2.ToArray());
		wheelieBinDrops.autoFillFromArray(list2.ToArray());
		list.Add(normalBarrelDrops[0]);
		barrelDrops.autoFillFromArray(list.ToArray());
	}

	public BuriedItem checkIfBuriedItem(int xPos, int yPos)
	{
		for (int i = 0; i < allBuriedItems.Count; i++)
		{
			if (allBuriedItems[i].matches(xPos, yPos))
			{
				return allBuriedItems[i];
			}
		}
		return null;
	}

	public void buryNewItem(int itemId, int stack, int xPos, int yPos)
	{
		allBuriedItems.Add(new BuriedItem(itemId, stack, xPos, yPos));
	}

	private IEnumerator placeBarrelNextFrame(int xPos, int yPos, NetworkConnection con)
	{
		yield return null;
		NetworkMapSharer.share.RpcUpdateOnTileObject(oldBarrel.tileObjectId, xPos, yPos);
		NetworkMapSharer.share.TargetGiveDigUpTreasureMilestone(con, -1);
	}

	private IEnumerator placeWheelieBinNextFrame(int xPos, int yPos, NetworkConnection con)
	{
		yield return null;
		NetworkMapSharer.share.RpcUpdateOnTileObject(wheelieBin.tileObjectId, xPos, yPos);
		NetworkMapSharer.share.TargetGiveDigUpTreasureMilestone(con, -1);
	}

	private IEnumerator placeAmberNextFrame(int xPos, int yPos, NetworkConnection con)
	{
		yield return null;
		Vector3 pos = new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2);
		NetworkMapSharer.share.spawnACarryable(amberChunk, pos);
		NetworkMapSharer.share.TargetGiveDigUpTreasureMilestone(con, -1);
	}

	public BuriedItem createARandomItemWhenNotFound(int xPos, int yPos, NetworkConnection con)
	{
		NetworkMapSharer.share.RpcDigUpBuriedItemNoise(xPos, yPos);
		if (Random.Range(0, 11) < 5)
		{
			StartCoroutine(placeBarrelNextFrame(xPos, yPos, con));
			return null;
		}
		if (Random.Range(0, 65) < 6)
		{
			StartCoroutine(placeWheelieBinNextFrame(xPos, yPos, con));
			return null;
		}
		if (Random.Range(0, 50) == 2)
		{
			StartCoroutine(placeAmberNextFrame(xPos, yPos, con));
			return null;
		}
		int stack = 1;
		InventoryItem randomDropFromTable;
		if (Random.Range(0, 3) <= 1)
		{
			randomDropFromTable = randomDrops.getRandomDropFromTable();
		}
		else
		{
			randomDropFromTable = veryCommonDrops.getRandomDropFromTable();
			stack = Random.Range(1, 4);
		}
		return new BuriedItem(Inventory.inv.getInvItemId(randomDropFromTable), stack, xPos, yPos);
	}

	public bool checkIfShouldTurnIntoBuriedItem(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] > 0 && (bool)WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].dropsItemOnDeath && (bool)WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].dropsItemOnDeath.placeable && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].dropsItemOnDeath.placeable.tileObjectId == WorldManager.manageWorld.onTileMap[xPos, yPos])
		{
			return false;
		}
		if (Random.Range(0, 45) == 25)
		{
			return true;
		}
		return false;
	}
}
