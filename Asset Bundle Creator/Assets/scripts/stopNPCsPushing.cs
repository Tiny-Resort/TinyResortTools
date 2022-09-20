using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class stopNPCsPushing : MonoBehaviour
{
	public NavMeshAgent myAgent;

	public NPCAI myAi;

	public GameObject myObsticles;

	public LayerMask npcLayer;

	public bool isStopped;

	public bool stoppedSelf;

	private static WaitForSeconds checkWait = new WaitForSeconds(1f);

	private int checkCount;

	private stopNPCsPushing stoppingFor;

	private void OnEnable()
	{
		if (NetworkMapSharer.share.isServer)
		{
			StartCoroutine(stopClumping());
		}
	}

	private IEnumerator stopClumping()
	{
		while (true)
		{
			if (!stoppedSelf)
			{
				RaycastHit hitInfo;
				if (isStopped)
				{
					checkCount--;
					if (checkCount == 0)
					{
						startNPC();
					}
					else if (myAi.doesTask.isScared)
					{
						startNPC();
					}
					else if (!stoppingFor)
					{
						startNPC();
					}
					else if (stoppingFor.isStopped)
					{
						startNPC();
					}
					else if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, 2.5f, npcLayer))
					{
						if (hitInfo.transform.root != stoppingFor.transform)
						{
							startNPC();
						}
					}
					else
					{
						startNPC();
					}
				}
				else if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out hitInfo, 2.5f, npcLayer))
				{
					stopNPCsPushing component = hitInfo.transform.GetComponent<stopNPCsPushing>();
					if ((bool)component && !component.isStopped)
					{
						if ((bool)myAi.doesTask && myAi.doesTask.hasTask && (bool)component.myAi.doesTask && !component.myAi.doesTask.hasTask)
						{
							component.stopNPC(this);
							recalculatePath();
						}
						else if (!myAi.doesTask.hasTask && component.myAi.doesTask.hasTask)
						{
							stopNPC(component);
							component.recalculatePath();
						}
						else if (myAgent.hasPath && !component.myAgent.hasPath)
						{
							component.stopNPC(this);
							recalculatePath();
						}
						else if (!myAgent.hasPath && component.myAgent.hasPath)
						{
							stopNPC(component);
							component.recalculatePath();
						}
						else if (Random.Range(0, 1) == 1)
						{
							component.stopNPC(this);
							recalculatePath();
						}
						else
						{
							stopNPC(component);
							component.recalculatePath();
						}
					}
				}
			}
			yield return checkWait;
		}
	}

	public void stopNPC(stopNPCsPushing stoppedfor)
	{
		checkCount = Random.Range(7, 10);
		stoppingFor = stoppedfor;
		isStopped = true;
		if (myAgent.isOnNavMesh)
		{
			myAgent.isStopped = true;
		}
		myObsticles.SetActive(true);
	}

	public void startNPC()
	{
		stoppingFor = null;
		isStopped = false;
		if (myAgent.isOnNavMesh && myAgent.isStopped)
		{
			myAgent.isStopped = false;
		}
		myObsticles.SetActive(false);
	}

	public bool checkIfIHavePriority(NavMeshAgent toCompare)
	{
		if (myAgent.avoidancePriority < toCompare.avoidancePriority)
		{
			return true;
		}
		return false;
	}

	public void recalculatePath()
	{
		if (myAgent.isActiveAndEnabled && myAgent.isStopped && myAgent.isOnNavMesh)
		{
			Vector3 destination = myAgent.destination;
			myAgent.ResetPath();
			myAgent.SetDestination(destination);
		}
	}

	public void stopSelf()
	{
		stoppedSelf = true;
		stoppingFor = null;
		isStopped = true;
		if (myAgent.isOnNavMesh)
		{
			myAgent.isStopped = true;
		}
		myObsticles.SetActive(true);
	}

	public void startSelf()
	{
		stoppedSelf = false;
		stoppingFor = null;
		isStopped = false;
		if (myAgent.isStopped && myAgent.isOnNavMesh)
		{
			myAgent.isStopped = false;
		}
		myObsticles.SetActive(false);
	}
}
