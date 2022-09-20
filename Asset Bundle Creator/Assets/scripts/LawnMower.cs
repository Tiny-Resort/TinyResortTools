using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class LawnMower : NetworkBehaviour
{
	public Vehicle myVehicle;

	public TileObject[] objectsCanCut;

	public ParticleSystem backPart;

	public ParticleSystem underPart;

	public ASound mowerRunOverSound;

	public override void OnStartAuthority()
	{
		StartCoroutine("mowTheLawn");
	}

	private IEnumerator mowTheLawn()
	{
		while (base.hasAuthority)
		{
			yield return null;
			if (!myVehicle.hasDriver())
			{
				continue;
			}
			int num = Mathf.RoundToInt(base.transform.position.x / 2f);
			int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
			if (WorldManager.manageWorld.onTileMap[num, num2] > 0 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num, num2]].isGrass)
			{
				if (base.hasAuthority)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CutGrass);
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.LawnMower);
					CmdCutTheGrass(num, num2);
				}
				if (shouldCutTileType(num, num2))
				{
					StartCoroutine(playCutParticles(WorldManager.manageWorld.tileTypeMap[num, num2]));
					yield return new WaitForSeconds(0.15f);
				}
			}
			else if (WorldManager.manageWorld.onTileMap[num, num2] == -1 && shouldCutTileType(num, num2))
			{
				if (base.hasAuthority)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.LawnMower);
					CmdChangeTileType(num, num2);
				}
				StartCoroutine(playCutParticles(WorldManager.manageWorld.tileTypeMap[num, num2]));
				yield return new WaitForSeconds(0.15f);
			}
			else if (WorldManager.manageWorld.onTileMap[num, num2] == -1 && hasBeenCut(num, num2))
			{
				StartCoroutine(playAlreadyCutParticles(WorldManager.manageWorld.tileTypeMap[num, num2]));
				yield return new WaitForSeconds(0.15f);
			}
		}
	}

	[Command]
	public void CmdCutTheGrass(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(LawnMower), "CmdCutTheGrass", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeTileType(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(LawnMower), "CmdChangeTileType", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void mowTileType(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 1)
		{
			NetworkMapSharer.share.RpcUpdateTileType(23, xPos, yPos);
		}
		if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 15)
		{
			NetworkMapSharer.share.RpcUpdateTileType(24, xPos, yPos);
		}
		if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 4)
		{
			NetworkMapSharer.share.RpcUpdateTileType(25, xPos, yPos);
		}
	}

	public bool shouldCutTileType(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] != 1 && WorldManager.manageWorld.tileTypeMap[xPos, yPos] != 4)
		{
			return WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 15;
		}
		return true;
	}

	public bool hasBeenCut(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] != 23 && WorldManager.manageWorld.tileTypeMap[xPos, yPos] != 24)
		{
			return WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 25;
		}
		return true;
	}

	private IEnumerator playCutParticles(int cuttingTileTypeId)
	{
		backPart.GetComponent<ParticleSystemRenderer>().sharedMaterial = WorldManager.manageWorld.tileTypes[cuttingTileTypeId].myTileMaterial;
		underPart.GetComponent<ParticleSystemRenderer>().sharedMaterial = WorldManager.manageWorld.tileTypes[cuttingTileTypeId].myTileMaterial;
		float cutTimer = 0.55f;
		SoundManager.manage.playASoundAtPoint(mowerRunOverSound, base.transform.position);
		while (cutTimer > 0f)
		{
			yield return null;
			cutTimer -= Time.deltaTime;
			backPart.Emit(4);
			underPart.Emit(4);
		}
	}

	private IEnumerator playAlreadyCutParticles(int cuttingTileTypeId)
	{
		backPart.GetComponent<ParticleSystemRenderer>().sharedMaterial = WorldManager.manageWorld.tileTypes[cuttingTileTypeId].myTileMaterial;
		float cutTimer = 0.15f;
		while (cutTimer > 0f)
		{
			yield return null;
			cutTimer -= Time.deltaTime;
			if (Random.Range(0, 10) == 2)
			{
				backPart.Emit(1);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdCutTheGrass(int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcUpdateOnTileObject(-1, xPos, yPos);
		mowTileType(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdCutTheGrass(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCutTheGrass called on client.");
		}
		else
		{
			((LawnMower)obj).UserCode_CmdCutTheGrass(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeTileType(int xPos, int yPos)
	{
		mowTileType(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdChangeTileType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTileType called on client.");
		}
		else
		{
			((LawnMower)obj).UserCode_CmdChangeTileType(reader.ReadInt(), reader.ReadInt());
		}
	}

	static LawnMower()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(LawnMower), "CmdCutTheGrass", InvokeUserCode_CmdCutTheGrass, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(LawnMower), "CmdChangeTileType", InvokeUserCode_CmdChangeTileType, true);
	}
}
