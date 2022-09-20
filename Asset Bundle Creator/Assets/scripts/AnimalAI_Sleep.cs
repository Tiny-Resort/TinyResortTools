using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class AnimalAI_Sleep : NetworkBehaviour
{
	private AnimalAI myAi;

	private FarmAnimal isFarmAnimal;

	private Damageable hasDamagable;

	private AnimalAI_Attack doesAttack;

	private FarmAnimalHouseFloor mySleepSpot;

	private Vector3 wildAnimalDesiredSleepPos;

	[SyncVar(hook = "onSleepChange")]
	private bool isSleeping;

	private WaitForSeconds sleepCheck = new WaitForSeconds(1f);

	public Transform headSleepPartPos;

	public int sleepFrom;

	public int sleepTo;

	public GameObject sleepingCollider;

	private AnimalAI_Pet isPet;

	public bool NetworkisSleeping
	{
		get
		{
			return isSleeping;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref isSleeping))
			{
				bool old = isSleeping;
				SetSyncVar(value, ref isSleeping, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onSleepChange(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	private void Start()
	{
		isPet = (isPet = GetComponent<AnimalAI_Pet>());
		myAi = GetComponent<AnimalAI>();
		isFarmAnimal = GetComponent<FarmAnimal>();
		hasDamagable = GetComponent<Damageable>();
		doesAttack = GetComponent<AnimalAI_Attack>();
	}

	public override void OnStartServer()
	{
		sleepCheck = new WaitForSeconds(Random.Range(0.85f, 1.25f));
		NetworkisSleeping = false;
	}

	public override void OnStartClient()
	{
		onSleepChange(isSleeping, isSleeping);
	}

	public bool tryingToSleep()
	{
		if (isSleeping)
		{
			return true;
		}
		return sleepDesiresMet();
	}

	private bool sleepDesiresMet()
	{
		if ((bool)isPet && hasDamagable.health <= 1)
		{
			return true;
		}
		if ((bool)isFarmAnimal && RealWorldTimeLight.time.isNightTime)
		{
			return true;
		}
		if (!isFarmAnimal && !RealWorldTimeLight.time.underGround && RealWorldTimeLight.time.currentHour >= sleepFrom && RealWorldTimeLight.time.currentHour < sleepTo)
		{
			return true;
		}
		return false;
	}

	public IEnumerator checkIfNeedsSleep()
	{
		if ((bool)isFarmAnimal && sleepDesiresMet())
		{
			if (!isSleeping && (!isPet || isPet.followingId == 0))
			{
				if ((bool)mySleepSpot)
				{
					if (myAi.myAgent.isActiveAndEnabled)
					{
						if (myAi.checkDestination(mySleepSpot.sleepingSpot.position) != base.transform.position)
						{
							myAi.myAgent.SetDestination(mySleepSpot.sleepingSpot.position);
						}
						else
						{
							Vector3 normalized = (mySleepSpot.sleepingSpot.position - base.transform.position).normalized;
							normalized = myAi.myAgent.transform.position + normalized * 4f;
							normalized = new Vector3(normalized.x, WorldManager.manageWorld.heightMap[(int)normalized.x / 2, (int)normalized.z / 2], normalized.z);
							myAi.myAgent.SetDestination(normalized);
						}
						if (Vector3.Distance(base.transform.position, mySleepSpot.sleepingSpot.position) < myAi.myAgent.stoppingDistance + 0.1f)
						{
							NetworkisSleeping = true;
						}
					}
					else
					{
						Vector3 normalized2 = (mySleepSpot.sleepingSpot.position - base.transform.position).normalized;
						normalized2 = myAi.myAgent.transform.position + normalized2;
						normalized2 = new Vector3(normalized2.x, WorldManager.manageWorld.heightMap[(int)normalized2.x / 2, (int)normalized2.z / 2], normalized2.z);
						myAi.myAgent.transform.position = normalized2;
						yield return new WaitForSeconds(0.5f);
						if (Vector3.Distance(base.transform.position, mySleepSpot.sleepingSpot.position) < myAi.myAgent.stoppingDistance + 1.5f)
						{
							NetworkisSleeping = true;
						}
					}
				}
				else
				{
					mySleepSpot = FarmAnimalManager.manage.findHouseForAnimal(isFarmAnimal.getDetails(), base.transform.position);
					if (!mySleepSpot && !WorldManager.manageWorld.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] && Random.Range(0, 15) == 8)
					{
						myAi.myAgent.SetDestination(base.transform.position);
						NetworkisSleeping = true;
					}
				}
			}
			yield return myAi.checkTimer;
		}
		else if (!isSleeping && sleepDesiresMet() && myAi.myAgent.isActiveAndEnabled)
		{
			if (myAi.checkDestination(wildAnimalDesiredSleepPos) != base.transform.position)
			{
				myAi.myAgent.SetDestination(wildAnimalDesiredSleepPos);
			}
			else
			{
				Vector3 normalized3 = (wildAnimalDesiredSleepPos - base.transform.position).normalized;
				normalized3 = myAi.myAgent.transform.position + normalized3 * 4f;
				normalized3 = new Vector3(normalized3.x, WorldManager.manageWorld.heightMap[(int)normalized3.x / 2, (int)normalized3.z / 2], normalized3.z);
				myAi.myAgent.SetDestination(normalized3);
			}
			if (Vector3.Distance(base.transform.position, wildAnimalDesiredSleepPos) < myAi.myAgent.stoppingDistance + 0.1f)
			{
				NetworkisSleeping = true;
				myAi.myAgent.isStopped = true;
				myAi.myAgent.updateRotation = false;
			}
		}
		while (isSleeping)
		{
			yield return null;
			if ((bool)isPet && isPet.followingId != 0)
			{
				wakeUpNow();
			}
			else if ((bool)isFarmAnimal)
			{
				if ((bool)mySleepSpot)
				{
					while (Quaternion.Angle(myAi.myAgent.transform.rotation, mySleepSpot.sleepingSpot.rotation) > 5f)
					{
						myAi.myAgent.transform.rotation = Quaternion.Lerp(myAi.myAgent.transform.rotation, mySleepSpot.sleepingSpot.rotation, Time.deltaTime);
						myAi.myAgent.transform.position = Vector3.Lerp(myAi.myAgent.transform.position, mySleepSpot.sleepingSpot.position, Time.deltaTime * 5f);
						yield return null;
					}
				}
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sleepingPart, headSleepPartPos.position, 1);
				if (!RealWorldTimeLight.time.isNightTime && Random.Range(0, 6) == 3)
				{
					wakeUpNow();
					if (myAi.myAgent.isActiveAndEnabled && (bool)mySleepSpot)
					{
						myAi.myAgent.SetDestination(base.transform.position + mySleepSpot.sleepingSpot.forward * 2.5f);
					}
				}
				if (!mySleepSpot)
				{
					mySleepSpot = FarmAnimalManager.manage.findHouseForAnimal(isFarmAnimal.getDetails(), base.transform.position);
					if ((bool)mySleepSpot)
					{
						wakeUpNow();
					}
				}
			}
			else
			{
				while (Vector3.Distance(myAi.myAgent.transform.position, wildAnimalDesiredSleepPos) > 0.5f)
				{
					myAi.myAgent.transform.position = Vector3.Lerp(myAi.myAgent.transform.position, wildAnimalDesiredSleepPos, Time.deltaTime * 2f);
					yield return null;
				}
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sleepingPart, headSleepPartPos.position, 1);
				if (RealWorldTimeLight.time.underGround)
				{
					wakeUpNow();
				}
				else if (RealWorldTimeLight.time.currentHour >= sleepFrom && RealWorldTimeLight.time.currentHour <= sleepTo)
				{
					if (myAi.isSkiddish)
					{
						Transform transform = myAi.returnClosestEnemy(0.25f);
						if ((bool)transform)
						{
							myAi.setRunningFrom(transform);
							wakeUpNow();
						}
					}
					if ((bool)doesAttack && (bool)doesAttack.returnClosestPrey(0.45f))
					{
						doesAttack.lookForClosetPreyAndChase();
						wakeUpNow();
					}
				}
				else if (Random.Range(0, 6) == 3)
				{
					wakeUpNow();
				}
			}
			yield return sleepCheck;
		}
	}

	private void onSleepChange(bool old, bool newSleeping)
	{
		NetworkisSleeping = newSleeping;
		GetComponent<Animator>().SetBool("Sleeping", isSleeping);
		bool isSleeping2 = isSleeping;
		if ((bool)sleepingCollider)
		{
			sleepingCollider.SetActive(isSleeping);
		}
	}

	public bool checkIfSleeping()
	{
		return isSleeping;
	}

	public void setDesiredSleepPos(Vector3 sleepPos)
	{
		wildAnimalDesiredSleepPos = sleepPos;
	}

	public Vector3 getSleepPos()
	{
		return wildAnimalDesiredSleepPos;
	}

	public void wakeUpNow()
	{
		if (isSleeping)
		{
			myAi.myAgent.isStopped = false;
			myAi.myAgent.updateRotation = true;
		}
		NetworkisSleeping = false;
	}

	public void setAnimalSleepSpot(FarmAnimalHouseFloor newSleepSpot)
	{
		wakeUpNow();
		mySleepSpot = newSleepSpot;
	}

	public void sendAnimalToSleep()
	{
		if (!isFarmAnimal)
		{
			return;
		}
		if ((bool)isPet)
		{
			isPet.setNewFollowTo(0u);
		}
		if ((bool)mySleepSpot)
		{
			if (myAi.checkIfWarpIsPossible(mySleepSpot.sleepingSpot.position))
			{
				myAi.myAgent.Warp(mySleepSpot.sleepingSpot.position);
			}
			else
			{
				myAi.myAgent.enabled = false;
			}
			myAi.myAgent.transform.rotation = mySleepSpot.sleepingSpot.rotation;
			myAi.myAgent.transform.position = mySleepSpot.sleepingSpot.position;
			base.transform.rotation = mySleepSpot.sleepingSpot.rotation;
			base.transform.position = mySleepSpot.sleepingSpot.position;
			myAi.checkDistanceToPlayerAndReturn();
			NetworkisSleeping = true;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isSleeping);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isSleeping);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = isSleeping;
			NetworkisSleeping = reader.ReadBool();
			if (!SyncVarEqual(flag, ref isSleeping))
			{
				onSleepChange(flag, isSleeping);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = isSleeping;
			NetworkisSleeping = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref isSleeping))
			{
				onSleepChange(flag2, isSleeping);
			}
		}
	}
}
