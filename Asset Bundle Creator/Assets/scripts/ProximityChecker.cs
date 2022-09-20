using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ProximityChecker : NetworkVisibility
{
	public List<NetworkConnection> playersObserving = new List<NetworkConnection>();

	private NetworkIdentity networkIdentity;

	private bool needsUpdating;

	private WaitForSeconds waitTime = new WaitForSeconds(1f);

	private void Awake()
	{
		networkIdentity = GetComponent<NetworkIdentity>();
	}

	public override void OnStartServer()
	{
		waitTime = new WaitForSeconds(Random.Range(0.75f, 1.05f));
		StartCoroutine(checkProximity());
	}

	public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
	{
		foreach (NetworkConnection item in playersObserving)
		{
			observers.Add(item);
		}
	}

	public override bool OnCheckObserver(NetworkConnection newObserver)
	{
		return false;
	}

	public void updateProximityToPlayers()
	{
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			if (Vector3.Distance(new Vector3(NetworkNavMesh.nav.charsConnected[i].position.x, 0f, NetworkNavMesh.nav.charsConnected[i].position.z), new Vector3(base.transform.position.x, 0f, base.transform.position.z)) < (float)NetworkNavMesh.nav.animalDistance)
			{
				if (!playersObserving.Contains(NetworkNavMesh.nav.charNetConn[i].connectionToClient))
				{
					playersObserving.Add(NetworkNavMesh.nav.charNetConn[i].connectionToClient);
					needsUpdating = true;
				}
			}
			else if (playersObserving.Contains(NetworkNavMesh.nav.charNetConn[i].connectionToClient))
			{
				playersObserving.Remove(NetworkNavMesh.nav.charNetConn[i].connectionToClient);
				needsUpdating = true;
			}
		}
	}

	private IEnumerator checkProximity()
	{
		while (true)
		{
			updateProximityToPlayers();
			if (needsUpdating)
			{
				networkIdentity.RebuildObservers(false);
				needsUpdating = false;
			}
			yield return waitTime;
		}
	}

	private void MirrorProcessed()
	{
	}
}
