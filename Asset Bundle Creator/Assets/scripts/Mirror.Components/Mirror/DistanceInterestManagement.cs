using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	public class DistanceInterestManagement : InterestManagement
	{
		[Tooltip("The maximum range that objects will be visible at. Add DistanceInterestManagementCustomRange onto NetworkIdentities for custom ranges.")]
		public int visRange = 10;

		[Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
		public float rebuildInterval = 1f;

		private double lastRebuildTime;

		private int GetVisRange(NetworkIdentity identity)
		{
			DistanceInterestManagementCustomRange component = identity.GetComponent<DistanceInterestManagementCustomRange>();
			if (!(component != null))
			{
				return visRange;
			}
			return component.visRange;
		}

		public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
		{
			int num = GetVisRange(identity);
			return Vector3.Distance(identity.transform.position, newObserver.identity.transform.position) < (float)num;
		}

		public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
		{
			int num = GetVisRange(identity);
			Vector3 position = identity.transform.position;
			foreach (NetworkConnectionToClient value in NetworkServer.connections.Values)
			{
				if (value != null && value.isAuthenticated && value.identity != null && Vector3.Distance(value.identity.transform.position, position) < (float)num)
				{
					newObservers.Add(value);
				}
			}
		}

		private void Update()
		{
			if (NetworkServer.active && NetworkTime.localTime >= lastRebuildTime + (double)rebuildInterval)
			{
				RebuildAll();
				lastRebuildTime = NetworkTime.localTime;
			}
		}
	}
}
