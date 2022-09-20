using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Wheelbarrow : NetworkBehaviour
{
	[SyncVar(hook = "changeTopDirt")]
	public int topDirtId;

	[SyncVar(hook = "changeDirtTotal")]
	public int totalDirt;

	public int[] layerIds = new int[10];

	public GameObject dirtFillUp;

	public MeshRenderer dirtFillUpRen;

	public InventoryItem emptyShovel;

	public InventoryItem[] shovelsToUse;

	public int NetworktopDirtId
	{
		get
		{
			return topDirtId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref topDirtId))
			{
				int oldTop = topDirtId;
				SetSyncVar(value, ref topDirtId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					changeTopDirt(oldTop, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public int NetworktotalDirt
	{
		get
		{
			return totalDirt;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref totalDirt))
			{
				int oldTotal = totalDirt;
				SetSyncVar(value, ref totalDirt, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					changeDirtTotal(oldTotal, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public override void OnStartClient()
	{
		updateContents();
	}

	public void insertDirt(int layerId)
	{
		NetworktopDirtId = layerId;
		layerIds[totalDirt] = layerId;
		NetworktotalDirt = totalDirt + 1;
	}

	public void removeDirt()
	{
		NetworktotalDirt = totalDirt - 1;
		NetworktopDirtId = layerIds[Mathf.Clamp(totalDirt - 10, 0, 10)];
	}

	private void changeDirtTotal(int oldTotal, int newTotal)
	{
		NetworktotalDirt = newTotal;
		if (topDirtId > -1 && oldTotal < newTotal)
		{
			SoundManager.manage.playASoundAtPoint(WorldManager.manageWorld.tileTypes[topDirtId].onHeightUp, base.transform.position);
		}
		else if (topDirtId > -1)
		{
			SoundManager.manage.playASoundAtPoint(WorldManager.manageWorld.tileTypes[topDirtId].onHeightDown, base.transform.position);
		}
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[4], dirtFillUp.transform.position, 15);
		updateContents();
	}

	private void changeTopDirt(int oldTop, int newTop)
	{
		NetworktopDirtId = newTop;
		updateTopLayer();
	}

	private void updateTopLayer()
	{
		if (topDirtId > -1)
		{
			dirtFillUpRen.sharedMaterial = WorldManager.manageWorld.tileTypes[topDirtId].myTileMaterial;
		}
	}

	public void updateContents()
	{
		if (totalDirt == 0)
		{
			dirtFillUp.gameObject.SetActive(false);
			return;
		}
		dirtFillUp.gameObject.SetActive(true);
		if ((float)totalDirt / 10f >= 0.5f)
		{
			dirtFillUp.transform.localScale = new Vector3(1f, (float)totalDirt / 10f, 1f);
		}
		else
		{
			dirtFillUp.transform.localScale = new Vector3(0.7f, (float)totalDirt / 10f, 0.7f);
		}
	}

	public bool isHoldingAShovel(InventoryItem itemToCheck)
	{
		for (int i = 0; i < shovelsToUse.Length; i++)
		{
			if (itemToCheck == shovelsToUse[i])
			{
				return true;
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
			writer.WriteInt(topDirtId);
			writer.WriteInt(totalDirt);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(topDirtId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(totalDirt);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = topDirtId;
			NetworktopDirtId = reader.ReadInt();
			if (!SyncVarEqual(num, ref topDirtId))
			{
				changeTopDirt(num, topDirtId);
			}
			int num2 = totalDirt;
			NetworktotalDirt = reader.ReadInt();
			if (!SyncVarEqual(num2, ref totalDirt))
			{
				changeDirtTotal(num2, totalDirt);
			}
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			int num4 = topDirtId;
			NetworktopDirtId = reader.ReadInt();
			if (!SyncVarEqual(num4, ref topDirtId))
			{
				changeTopDirt(num4, topDirtId);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			int num5 = totalDirt;
			NetworktotalDirt = reader.ReadInt();
			if (!SyncVarEqual(num5, ref totalDirt))
			{
				changeDirtTotal(num5, totalDirt);
			}
		}
	}
}
