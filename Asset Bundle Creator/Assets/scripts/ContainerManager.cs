using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ContainerManager : NetworkBehaviour
{
	public List<Chest> activeChests = new List<Chest>();

	public List<Chest> undergroundChests = new List<Chest>();

	public List<Chest> privateStashes = new List<Chest>();

	public static ContainerManager manage;

	public InventoryItemLootTable undergroundCrateTable;

	public InventoryItemLootTable paintTable;

	private void Awake()
	{
		manage = this;
	}

	public void closeChestFromServer(int xPos, int yPos, int[] itemIds, int[] stacks, HouseDetails inside)
	{
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.itemIds = itemIds;
				activeChest.itemStacks = stacks;
				break;
			}
		}
	}

	public void openChestFromServer(NetworkConnection con, int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround)
		{
			foreach (Chest undergroundChest in undergroundChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, undergroundChest) && undergroundChest.xPos == xPos && undergroundChest.yPos == yPos)
				{
					playerOpenedChest(xPos, yPos, inside);
					TargetOpenChest(con, xPos, yPos, undergroundChest.itemIds, undergroundChest.itemStacks);
					return;
				}
			}
			Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
			undergroundChests.Add(chestSaveOrCreateNewOne);
			playerOpenedChest(xPos, yPos, inside);
			TargetOpenChest(con, xPos, yPos, chestSaveOrCreateNewOne.itemIds, chestSaveOrCreateNewOne.itemStacks);
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				playerOpenedChest(xPos, yPos, inside);
				TargetOpenChest(con, xPos, yPos, activeChest.itemIds, activeChest.itemStacks);
				return;
			}
		}
		Chest chestSaveOrCreateNewOne2 = getChestSaveOrCreateNewOne(xPos, yPos, inside);
		activeChests.Add(chestSaveOrCreateNewOne2);
		playerOpenedChest(xPos, yPos, inside);
		TargetOpenChest(con, xPos, yPos, chestSaveOrCreateNewOne2.itemIds, chestSaveOrCreateNewOne2.itemStacks);
	}

	public void openStash(int stashId)
	{
		if (privateStashes.Count == 0)
		{
			loadStashes();
		}
		ChestWindow.chests.openStashInWindow(stashId);
	}

	public void playerCloseChest(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround)
		{
			foreach (Chest undergroundChest in undergroundChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, undergroundChest) && undergroundChest.xPos == xPos && undergroundChest.yPos == yPos)
				{
					undergroundChest.playingLookingInside--;
					if (undergroundChest.playingLookingInside <= 0)
					{
						undergroundChest.playingLookingInside = 0;
						if (inside == null)
						{
							NetworkMapSharer.share.RpcGiveOnTileStatus(0, xPos, yPos);
						}
						else
						{
							NetworkMapSharer.share.RpcGiveOnTileStatusInside(0, xPos, yPos, inside.xPos, inside.yPos);
						}
					}
					break;
				}
			}
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (!checkIfChestIsInsideAndInThisHouse(inside, activeChest) || activeChest.xPos != xPos || activeChest.yPos != yPos)
			{
				continue;
			}
			activeChest.playingLookingInside--;
			if (activeChest.playingLookingInside <= 0)
			{
				activeChest.playingLookingInside = 0;
				if (inside == null)
				{
					NetworkMapSharer.share.RpcGiveOnTileStatus(0, xPos, yPos);
				}
				else
				{
					NetworkMapSharer.share.RpcGiveOnTileStatusInside(0, xPos, yPos, inside.xPos, inside.yPos);
				}
			}
			break;
		}
	}

	public void playerOpenedChest(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround)
		{
			foreach (Chest undergroundChest in undergroundChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, undergroundChest) && undergroundChest.xPos == xPos && undergroundChest.yPos == yPos)
				{
					undergroundChest.playingLookingInside++;
					if (inside == null)
					{
						NetworkMapSharer.share.RpcGiveOnTileStatus(1, xPos, yPos);
					}
					else
					{
						NetworkMapSharer.share.RpcGiveOnTileStatusInside(1, xPos, yPos, inside.xPos, inside.yPos);
					}
					break;
				}
			}
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.playingLookingInside++;
				if (inside == null)
				{
					NetworkMapSharer.share.RpcGiveOnTileStatus(1, xPos, yPos);
				}
				else
				{
					NetworkMapSharer.share.RpcGiveOnTileStatusInside(1, xPos, yPos, inside.xPos, inside.yPos);
				}
				break;
			}
		}
	}

	public bool checkIfEmpty(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround)
		{
			foreach (Chest undergroundChest in undergroundChests)
			{
				if (!checkIfChestIsInsideAndInThisHouse(inside, undergroundChest) || undergroundChest.xPos != xPos || undergroundChest.yPos != yPos)
				{
					continue;
				}
				for (int i = 0; i < 24; i++)
				{
					if (undergroundChest.itemIds[i] != -1)
					{
						return false;
					}
				}
				return true;
			}
		}
		else
		{
			foreach (Chest activeChest in activeChests)
			{
				if (!checkIfChestIsInsideAndInThisHouse(inside, activeChest) || activeChest.xPos != xPos || activeChest.yPos != yPos)
				{
					continue;
				}
				for (int j = 0; j < 24; j++)
				{
					if (activeChest.itemIds[j] != -1)
					{
						return false;
					}
				}
				return true;
			}
			Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
			activeChests.Add(chestSaveOrCreateNewOne);
			for (int k = 0; k < 24; k++)
			{
				if (chestSaveOrCreateNewOne.itemIds[k] != -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public Chest getChestForWindow(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround)
		{
			foreach (Chest undergroundChest in undergroundChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, undergroundChest) && undergroundChest.xPos == xPos && undergroundChest.yPos == yPos)
				{
					return undergroundChest;
				}
			}
		}
		else
		{
			foreach (Chest activeChest in activeChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
				{
					return activeChest;
				}
			}
		}
		return null;
	}

	public Chest getChestForRecycling(int xPos, int yPos, HouseDetails inside)
	{
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				return activeChest;
			}
		}
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
		activeChests.Add(chestSaveOrCreateNewOne);
		return chestSaveOrCreateNewOne;
	}

	public void changeSlotInChest(int xPos, int yPos, int slotNo, int newItemId, int newItemStack, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround)
		{
			foreach (Chest undergroundChest in undergroundChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, undergroundChest) && undergroundChest.xPos == xPos && undergroundChest.yPos == yPos)
				{
					undergroundChest.itemIds[slotNo] = newItemId;
					undergroundChest.itemStacks[slotNo] = newItemStack;
					break;
				}
			}
		}
		else
		{
			foreach (Chest activeChest in activeChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
				{
					activeChest.itemIds[slotNo] = newItemId;
					activeChest.itemStacks[slotNo] = newItemStack;
					break;
				}
			}
		}
		if (inside != null)
		{
			RpcRefreshOpenedChest(xPos, yPos, slotNo, newItemId, newItemStack, inside.xPos, inside.yPos);
		}
		else
		{
			RpcRefreshOpenedChest(xPos, yPos, slotNo, newItemId, newItemStack, -1, -1);
		}
	}

	public void clearWholeContainer(int xPos, int yPos, HouseDetails inside)
	{
		if (inside != null)
		{
			RpcClearChest(xPos, yPos, inside.xPos, inside.yPos);
		}
		else
		{
			RpcClearChest(xPos, yPos, -1, -1);
		}
	}

	[ClientRpc]
	public void RpcClearChest(int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(ContainerManager), "RpcClearChest", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRefreshOpenedChest(int xPos, int yPos, int slotNo, int newItemId, int newItemStack, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(slotNo);
		writer.WriteInt(newItemId);
		writer.WriteInt(newItemStack);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(ContainerManager), "RpcRefreshOpenedChest", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetOpenChest(NetworkConnection con, int xPos, int yPos, int[] itemIds, int[] itemStack)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemIds);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemStack);
		SendTargetRPCInternal(con, typeof(ContainerManager), "TargetOpenChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private Chest getChestSaveOrCreateNewOne(int xPos, int yPos, HouseDetails inside)
	{
		Chest chest = new Chest(xPos, yPos);
		string text = "/Chests/chest";
		if (inside != null)
		{
			chest.inside = true;
			text = text + "h" + inside.xPos + "+" + inside.yPos + "_";
			chest.insideX = inside.xPos;
			chest.insideY = inside.yPos;
		}
		if (!RealWorldTimeLight.time.underGround && File.Exists(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat", FileMode.Open);
				ChestSave chestSave = (ChestSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < 24; i++)
				{
					chest.itemIds[i] = chestSave.itemId[i];
					chest.itemStacks[i] = chestSave.itemStack[i];
				}
				fileStream.Close();
				SaveLoad.saveOrLoad.makeABackUp(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat", SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".bak");
				return chest;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading chest");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				ChestSave chestSave2 = checkForBackUpChest(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
				if (chestSave2 != null)
				{
					for (int j = 0; j < 24; j++)
					{
						chest.itemIds[j] = chestSave2.itemId[j];
						chest.itemStacks[j] = chestSave2.itemStack[j];
					}
					return chest;
				}
				for (int k = 0; k < 24; k++)
				{
					chest.itemIds[k] = -1;
					chest.itemStacks[k] = 0;
				}
				return chest;
			}
		}
		if (!RealWorldTimeLight.time.underGround)
		{
			ChestSave chestSave3 = checkForBackUpChest(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
			if (chestSave3 != null)
			{
				for (int l = 0; l < 24; l++)
				{
					chest.itemIds[l] = chestSave3.itemId[l];
					chest.itemStacks[l] = chestSave3.itemStack[l];
				}
			}
			else
			{
				if (!RealWorldTimeLight.time.underGround && EasyLoadChestExists(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat"))
				{
					return EasyLoadChests(chest, SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
				}
				for (int m = 0; m < 24; m++)
				{
					chest.itemIds[m] = -1;
					chest.itemStacks[m] = 0;
				}
			}
		}
		else
		{
			for (int n = 0; n < 24; n++)
			{
				chest.itemIds[n] = -1;
				chest.itemStacks[n] = 0;
			}
		}
		return chest;
	}

	private ChestSave checkForBackUpChest(string path)
	{
		FileStream fileStream = null;
		ChestSave result = null;
		path = path.Replace(".dat", ".bak");
		MonoBehaviour.print("Looking for backup chest at: " + path);
		if (!RealWorldTimeLight.time.underGround && File.Exists(path))
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(path, FileMode.Open);
				result = (ChestSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				Debug.LogWarning("Chest backup found");
				return result;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading chest backup");
				if (fileStream == null)
				{
					return result;
				}
				fileStream.Close();
				return result;
			}
		}
		return result;
	}

	public void EasySaveChests(Chest chestToSave, string savePath)
	{
		savePath = savePath.Replace(".dat", ".es3");
		ChestSave chestSave = new ChestSave();
		for (int i = 0; i < 24; i++)
		{
			chestSave.itemId[i] = chestToSave.itemIds[i];
			chestSave.itemStack[i] = chestToSave.itemStacks[i];
		}
		try
		{
			ES3.Save("chestInfo", chestSave, savePath);
		}
		catch
		{
			ES3.DeleteFile(savePath);
			ES3.Save("chestInfo", chestSave, savePath);
		}
	}

	public bool EasyLoadChestExists(string savePath)
	{
		savePath = savePath.Replace(".dat", ".es3");
		if (ES3.KeyExists("chestInfo", savePath))
		{
			return true;
		}
		return false;
	}

	public Chest EasyLoadChests(Chest returnChest, string savePath)
	{
		try
		{
			savePath = savePath.Replace(".dat", ".es3");
			if (ES3.KeyExists("chestInfo", savePath))
			{
				ChestSave chestSave = new ChestSave();
				ES3.LoadInto("chestInfo", savePath, chestSave);
				for (int i = 0; i < 24; i++)
				{
					returnChest.itemIds[i] = chestSave.itemId[i];
					returnChest.itemStacks[i] = chestSave.itemStack[i];
				}
				return returnChest;
			}
			return returnChest;
		}
		catch
		{
			return returnChest;
		}
	}

	public void saveChest(Chest chestToSave)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(SaveLoad.saveOrLoad.saveSlot() + "/Chests");
		if (!directoryInfo.Exists)
		{
			Debug.Log("Creating Chest Folder");
			directoryInfo.Create();
		}
		FileStream fileStream = null;
		try
		{
			string text = "/Chests/chest";
			if (chestToSave.inside)
			{
				text = text + "h" + chestToSave.insideX + "+" + chestToSave.insideY + "_";
			}
			EasySaveChests(chestToSave, SaveLoad.saveOrLoad.saveSlot() + text + chestToSave.xPos + "+" + chestToSave.yPos + ".dat");
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(SaveLoad.saveOrLoad.saveSlot() + text + chestToSave.xPos + "+" + chestToSave.yPos + ".dat");
			ChestSave chestSave = new ChestSave();
			for (int i = 0; i < 24; i++)
			{
				chestSave.itemId[i] = chestToSave.itemIds[i];
				chestSave.itemStack[i] = chestToSave.itemStacks[i];
			}
			binaryFormatter.Serialize(fileStream, chestSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving chest");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void saveStashes()
	{
		for (int i = 0; i < privateStashes.Count; i++)
		{
			Chest chest = privateStashes[i];
			DirectoryInfo directoryInfo = new DirectoryInfo(SaveLoad.saveOrLoad.saveSlot() + "/Chests");
			if (!directoryInfo.Exists)
			{
				Debug.Log("Creating Chest Folder");
				directoryInfo.Create();
			}
			string text = "/Chests/chest";
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Create(SaveLoad.saveOrLoad.saveSlot() + text + -(i + 1) + "+" + -(i + 1) + ".dat");
				ChestSave chestSave = new ChestSave();
				for (int j = 0; j < 24; j++)
				{
					chestSave.itemId[j] = chest.itemIds[j];
					chestSave.itemStack[j] = chest.itemStacks[j];
				}
				binaryFormatter.Serialize(fileStream, chestSave);
				fileStream.Close();
			}
			catch (Exception)
			{
				Debug.LogWarning("Error saving stash");
				if (fileStream != null)
				{
					fileStream.Close();
				}
			}
		}
	}

	public void loadStashes()
	{
		for (int i = 0; i < 2; i++)
		{
			privateStashes.Add(getChestSaveOrCreateNewOne(-(i + 1), -(i + 1), null));
		}
	}

	public bool checkIfChestIsInsideAndInThisHouse(HouseDetails house, Chest checkChest)
	{
		if (checkChest.inside && house != null)
		{
			if (house.xPos == checkChest.insideX && house.yPos == checkChest.insideY)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void moveHousePosForChest(int xPos, int yPos, HouseDetails details, int newHouseX, int newHouseY)
	{
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(details, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.insideX = newHouseX;
				activeChest.insideY = newHouseY;
				deleteOldChestSaveIfFound(details, xPos, yPos);
				return;
			}
		}
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, details);
		chestSaveOrCreateNewOne.insideX = newHouseX;
		chestSaveOrCreateNewOne.insideY = newHouseY;
		activeChests.Add(chestSaveOrCreateNewOne);
		deleteOldChestSaveIfFound(details, xPos, yPos);
	}

	public void moveChestInsideHousePositon(HouseDetails details, int xPos, int yPos, int newX, int newY)
	{
		if (!base.isServer)
		{
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(details, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.xPos = newX;
				activeChest.insideY = newY;
				deleteOldChestSaveIfFound(details, xPos, yPos);
				return;
			}
		}
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, details);
		chestSaveOrCreateNewOne.xPos = newX;
		chestSaveOrCreateNewOne.insideY = newY;
		activeChests.Add(chestSaveOrCreateNewOne);
		deleteOldChestSaveIfFound(details, xPos, yPos);
	}

	public void deleteOldChestSaveIfFound(HouseDetails house, int xPos, int yPos)
	{
		string text = "/Chests/chest";
		if (house != null)
		{
			text = text + "h" + house.xPos + "+" + house.yPos + "_";
		}
		if (File.Exists(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat"))
		{
			File.Delete(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
		}
	}

	public void generateUndergroundChest(int xPos, int yPos)
	{
		Chest chest = new Chest(xPos, yPos);
		for (int i = 0; i < 24; i++)
		{
			chest.itemIds[i] = -1;
			chest.itemStacks[i] = 0;
		}
		int num = UnityEngine.Random.Range(4, 6);
		while (num > 0)
		{
			int num2 = UnityEngine.Random.Range(0, 24);
			if (chest.itemIds[num2] == -1)
			{
				chest.itemIds[num2] = undergroundCrateTable.getRandomDropFromTable().getItemId();
				if (Inventory.inv.allItems[chest.itemIds[num2]].hasFuel)
				{
					chest.itemStacks[num2] = UnityEngine.Random.Range(10, (int)((float)Inventory.inv.allItems[chest.itemIds[num2]].fuelMax / 1.5f));
				}
				else
				{
					chest.itemStacks[num2] = 1;
				}
				num--;
			}
		}
		bool flag = false;
		while (!flag)
		{
			int num3 = UnityEngine.Random.Range(0, 24);
			if (chest.itemIds[num3] == -1)
			{
				chest.itemIds[num3] = paintTable.getRandomDropFromTable().getItemId();
				chest.itemStacks[num3] = 1;
				flag = true;
			}
		}
		undergroundChests.Add(chest);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcClearChest(int xPos, int yPos, int houseX, int houseY)
	{
		HouseDetails house = null;
		if (houseX != -1 && houseY != -1)
		{
			house = HouseManager.manage.getHouseInfo(houseX, houseY);
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(house, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				for (int i = 0; i < activeChest.itemIds.Length; i++)
				{
					activeChest.itemIds[i] = -1;
					activeChest.itemStacks[i] = -1;
				}
				break;
			}
		}
	}

	protected static void InvokeUserCode_RpcClearChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearChest called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_RpcClearChest(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRefreshOpenedChest(int xPos, int yPos, int slotNo, int newItemId, int newItemStack, int houseX, int houseY)
	{
		HouseDetails houseDetails = null;
		if (houseX != -1 && houseY != -1)
		{
			houseDetails = HouseManager.manage.getHouseInfo(houseX, houseY);
		}
		if (!base.isServer)
		{
			if (RealWorldTimeLight.time.underGround)
			{
				foreach (Chest undergroundChest in undergroundChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(houseDetails, undergroundChest) && undergroundChest.xPos == xPos && undergroundChest.yPos == yPos)
					{
						undergroundChest.itemIds[slotNo] = newItemId;
						undergroundChest.itemStacks[slotNo] = newItemStack;
						break;
					}
				}
			}
			else
			{
				foreach (Chest activeChest in activeChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(houseDetails, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
					{
						activeChest.itemIds[slotNo] = newItemId;
						activeChest.itemStacks[slotNo] = newItemStack;
						break;
					}
				}
			}
		}
		ChestWindow.chests.refreshOpenWindow(xPos, yPos, houseDetails);
	}

	protected static void InvokeUserCode_RpcRefreshOpenedChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRefreshOpenedChest called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_RpcRefreshOpenedChest(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetOpenChest(NetworkConnection con, int xPos, int yPos, int[] itemIds, int[] itemStack)
	{
		HouseDetails insideHouseDetails = NetworkMapSharer.share.localChar.myInteract.insideHouseDetails;
		if (!base.isServer)
		{
			if (RealWorldTimeLight.time.underGround)
			{
				foreach (Chest undergroundChest in undergroundChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, undergroundChest) && undergroundChest.xPos == xPos && undergroundChest.yPos == yPos)
					{
						undergroundChest.itemIds = itemIds;
						undergroundChest.itemStacks = itemStack;
						ChestWindow.chests.openChestInWindow(xPos, yPos);
						return;
					}
				}
				Chest chest = new Chest(xPos, yPos);
				chest.itemIds = itemIds;
				chest.itemStacks = itemStack;
				if (insideHouseDetails != null)
				{
					chest.inside = true;
					chest.insideX = insideHouseDetails.xPos;
					chest.insideY = insideHouseDetails.yPos;
				}
				undergroundChests.Add(chest);
			}
			else
			{
				foreach (Chest activeChest in activeChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
					{
						activeChest.itemIds = itemIds;
						activeChest.itemStacks = itemStack;
						ChestWindow.chests.openChestInWindow(xPos, yPos);
						return;
					}
				}
				Chest chest2 = new Chest(xPos, yPos);
				chest2.itemIds = itemIds;
				chest2.itemStacks = itemStack;
				if (insideHouseDetails != null)
				{
					chest2.inside = true;
					chest2.insideX = insideHouseDetails.xPos;
					chest2.insideY = insideHouseDetails.yPos;
				}
				activeChests.Add(chest2);
			}
		}
		ChestWindow.chests.openChestInWindow(xPos, yPos);
	}

	protected static void InvokeUserCode_TargetOpenChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOpenChest called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_TargetOpenChest(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	static ContainerManager()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "RpcClearChest", InvokeUserCode_RpcClearChest);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "RpcRefreshOpenedChest", InvokeUserCode_RpcRefreshOpenedChest);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "TargetOpenChest", InvokeUserCode_TargetOpenChest);
	}
}
