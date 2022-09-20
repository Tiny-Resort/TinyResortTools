using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class AnimalAILookForFood : NetworkBehaviour
{
	public bool animalEatsCrops;

	public TileObject[] eatsTiles;

	public InventoryItem[] eatsDrops;

	private AnimalAI myAi;

	private FarmAnimal isFarmAnimal;

	public bool isHungry = true;

	private bool isEatingADrop;

	private Damageable myDamageable;

	private AnimalAI_Attack hunts;

	[Header("Check Distances")]
	public float dropCheckDistance = 5f;

	private int loopsSinceLastCheck;

	private void Start()
	{
		myAi = GetComponent<AnimalAI>();
		isFarmAnimal = GetComponent<FarmAnimal>();
		hunts = GetComponent<AnimalAI_Attack>();
		myDamageable = GetComponent<Damageable>();
	}

	public override void OnStartServer()
	{
		if (!isFarmAnimal && (bool)hunts && hunts.huntsWhenHungry)
		{
			isHungry = Random.Range(0, 10) == 1;
		}
		else
		{
			isHungry = true;
		}
	}

	public IEnumerator searchForFoodNearby()
	{
		loopsSinceLastCheck++;
		if (loopsSinceLastCheck < Random.Range(8, 12))
		{
			loopsSinceLastCheck = 0;
			if ((bool)isFarmAnimal && RealWorldTimeLight.time.currentHour < 8)
			{
				yield break;
			}
			if (myDamageable.health < myDamageable.maxHealth)
			{
				isHungry = true;
			}
			if (isHungry && Random.Range(0, 15) == 7)
			{
				Vector3 currentlyTryingToEatPos = Vector3.zero;
				if (eatsDrops.Length != 0)
				{
					currentlyTryingToEatPos = WorldManager.manageWorld.findDroppedObjectAround(base.transform.position, eatsDrops, dropCheckDistance);
					if (currentlyTryingToEatPos != Vector3.zero)
					{
						isEatingADrop = true;
					}
					else
					{
						isEatingADrop = false;
					}
				}
				if (eatsTiles.Length != 0 && !isEatingADrop)
				{
					currentlyTryingToEatPos = WorldManager.manageWorld.findTileObjectAround(base.transform.position, eatsTiles, 5, true);
				}
				if (animalEatsCrops && !isEatingADrop && currentlyTryingToEatPos != Vector3.zero)
				{
					currentlyTryingToEatPos = WorldManager.manageWorld.findTileObjectAround(base.transform.position, TownManager.manage.allCropsTypes, 5, true);
				}
				if (!(currentlyTryingToEatPos != Vector3.zero))
				{
					yield break;
				}
				int objectEatingId = WorldManager.manageWorld.onTileMap[(int)currentlyTryingToEatPos.x / 2, (int)currentlyTryingToEatPos.z / 2];
				myAi.myAgent.SetDestination(currentlyTryingToEatPos);
				while (isHungry && currentlyTryingToEatPos != Vector3.zero && !myAi.returnClosestEnemy() && myAi.currentlyAttacking() == null)
				{
					if (isWhatIWantStillThere(currentlyTryingToEatPos, objectEatingId))
					{
						if (myAi.checkIfHasArrivedAtDestination() && Vector3.Distance(base.transform.position, currentlyTryingToEatPos) <= 3f)
						{
							yield return StartCoroutine(faceFood(currentlyTryingToEatPos, objectEatingId));
						}
					}
					else
					{
						if (eatsDrops.Length == 0)
						{
							currentlyTryingToEatPos = ((!animalEatsCrops) ? WorldManager.manageWorld.findTileObjectAround(base.transform.position, eatsTiles, 5, true) : WorldManager.manageWorld.findTileObjectAround(base.transform.position, TownManager.manage.allCropsTypes, 5, true));
						}
						else
						{
							currentlyTryingToEatPos = WorldManager.manageWorld.findDroppedObjectAround(base.transform.position, eatsDrops, dropCheckDistance);
							if (currentlyTryingToEatPos != Vector3.zero)
							{
								isEatingADrop = true;
							}
							else
							{
								isEatingADrop = false;
							}
						}
						if (currentlyTryingToEatPos != Vector3.zero)
						{
							objectEatingId = WorldManager.manageWorld.onTileMap[(int)currentlyTryingToEatPos.x / 2, (int)currentlyTryingToEatPos.z / 2];
							myAi.myAgent.SetDestination(currentlyTryingToEatPos);
						}
					}
					if (myAi.checkIfShouldContinue())
					{
						yield return null;
						continue;
					}
					break;
				}
			}
			else
			{
				yield return null;
			}
		}
		else
		{
			yield return null;
		}
	}

	private IEnumerator faceFood(Vector3 currentlyTryingToEatPos, int objectEatingId)
	{
		myAi.myAgent.updateRotation = false;
		myAi.myAgent.SetDestination(base.transform.position);
		Vector3 toPosition = new Vector3(currentlyTryingToEatPos.x, base.transform.position.y, currentlyTryingToEatPos.z) - base.transform.position;
		float step = 200f * Time.deltaTime;
		while (myAi.myAgent.isActiveAndEnabled && Vector3.Angle(base.transform.forward, toPosition) > 5f && isWhatIWantStillThere(currentlyTryingToEatPos, objectEatingId))
		{
			if (myAi.myAgent.isActiveAndEnabled)
			{
				myAi.myAgent.SetDestination(base.transform.position);
			}
			toPosition = new Vector3(currentlyTryingToEatPos.x, base.transform.position.y, currentlyTryingToEatPos.z) - base.transform.position;
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(toPosition), step);
			base.transform.rotation = myAi.myAgent.transform.rotation;
			if (!myAi.checkIfShouldContinue())
			{
				break;
			}
			yield return null;
		}
		if (myAi.checkIfShouldContinue() && isWhatIWantStillThere(currentlyTryingToEatPos, objectEatingId))
		{
			yield return StartCoroutine(EatFood(currentlyTryingToEatPos, objectEatingId));
		}
		else
		{
			currentlyTryingToEatPos = Vector3.zero;
		}
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator EatFood(Vector3 currentlyTryingToEatPos, int objectEatingId)
	{
		yield return new WaitForSeconds(0.25f);
		int eatingHealth = 0;
		if (!isEatingADrop)
		{
			eatingHealth = (int)WorldManager.manageWorld.allObjectSettings[objectEatingId].fullHealth;
		}
		while (myAi.checkIfShouldContinue() && isWhatIWantStillThere(currentlyTryingToEatPos, objectEatingId) && currentlyTryingToEatPos != Vector3.zero && !myAi.returnClosestEnemy())
		{
			if (isEatingADrop)
			{
				List<DroppedItem> allDropsOnTile = WorldManager.manageWorld.getAllDropsOnTile((int)currentlyTryingToEatPos.x / 2, (int)currentlyTryingToEatPos.z / 2);
				bool flag = false;
				for (int i = 0; i < allDropsOnTile.Count; i++)
				{
					for (int j = 0; j < eatsDrops.Length; j++)
					{
						if (allDropsOnTile[i].myItemId == Inventory.inv.getInvItemId(eatsDrops[j]))
						{
							if (allDropsOnTile[i].stackAmount > 1)
							{
								DroppedItem droppedItem = allDropsOnTile[i];
								droppedItem.NetworkstackAmount = droppedItem.stackAmount - 1;
							}
							else
							{
								allDropsOnTile[i].bury();
							}
							RpcEatADrop();
							flag = true;
							currentlyTryingToEatPos = Vector3.zero;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				checkIfStillHungry();
			}
			else
			{
				eatingHealth -= 5;
				RpcEat((int)currentlyTryingToEatPos.x / 2, (int)currentlyTryingToEatPos.z / 2);
				if (eatingHealth <= 0)
				{
					if ((bool)WorldManager.manageWorld.allObjects[objectEatingId].tileOnOff)
					{
						NetworkMapSharer.share.RpcOpenCloseTile((int)currentlyTryingToEatPos.x / 2, (int)currentlyTryingToEatPos.z / 2, 0);
					}
					else
					{
						NetworkMapSharer.share.RpcUpdateOnTileObject(-1, (int)currentlyTryingToEatPos.x / 2, (int)currentlyTryingToEatPos.z / 2);
					}
					currentlyTryingToEatPos = Vector3.zero;
					checkIfStillHungry();
				}
			}
			if (!myAi.checkIfShouldContinue())
			{
				yield return null;
				break;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private bool isWhatIWantStillThere(Vector3 wantToEatPos, int objectEatingId)
	{
		if (isEatingADrop)
		{
			List<DroppedItem> allDropsOnTile = WorldManager.manageWorld.getAllDropsOnTile((int)wantToEatPos.x / 2, (int)wantToEatPos.z / 2);
			for (int i = 0; i < allDropsOnTile.Count; i++)
			{
				for (int j = 0; j < eatsDrops.Length; j++)
				{
					if (allDropsOnTile[i].myItemId == Inventory.inv.getInvItemId(eatsDrops[j]))
					{
						return true;
					}
				}
			}
			return false;
		}
		if ((WorldManager.manageWorld.onTileMap[(int)wantToEatPos.x / 2, (int)wantToEatPos.z / 2] > -1 && !WorldManager.manageWorld.allObjects[objectEatingId].tileOnOff) || ((bool)WorldManager.manageWorld.allObjects[objectEatingId].tileOnOff && WorldManager.manageWorld.onTileStatusMap[(int)wantToEatPos.x / 2, (int)wantToEatPos.z / 2] == 1))
		{
			return true;
		}
		return false;
	}

	private void checkIfStillHungry()
	{
		if (myDamageable.health < myDamageable.maxHealth)
		{
			myDamageable.Networkhealth = Mathf.Clamp(myDamageable.health + 20, 0, myDamageable.maxHealth);
		}
		if (myDamageable.health >= myDamageable.maxHealth)
		{
			isHungry = false;
		}
		if ((bool)isFarmAnimal)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.FeedAnimals);
			isFarmAnimal.setEaten(true);
		}
	}

	[ClientRpc]
	private void RpcEat(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(AnimalAILookForFood), "RpcEat", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcEatADrop()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAILookForFood), "RpcEatADrop", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator eatRpcDelay(int xPos, int yPos)
	{
		GetComponent<Animator>().SetTrigger("Eat");
		yield return new WaitForSeconds(0.5f);
		TileObject tileObject = WorldManager.manageWorld.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.damage();
		}
	}

	private IEnumerator eatADropRpc()
	{
		GetComponent<Animator>().SetTrigger("Eat");
		yield return new WaitForSeconds(0.25f);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcEat(int xPos, int yPos)
	{
		StartCoroutine(eatRpcDelay(xPos, yPos));
	}

	protected static void InvokeUserCode_RpcEat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEat called on server.");
		}
		else
		{
			((AnimalAILookForFood)obj).UserCode_RpcEat(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcEatADrop()
	{
		StartCoroutine(eatADropRpc());
	}

	protected static void InvokeUserCode_RpcEatADrop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEatADrop called on server.");
		}
		else
		{
			((AnimalAILookForFood)obj).UserCode_RpcEatADrop();
		}
	}

	static AnimalAILookForFood()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAILookForFood), "RpcEat", InvokeUserCode_RpcEat);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAILookForFood), "RpcEatADrop", InvokeUserCode_RpcEatADrop);
	}
}
