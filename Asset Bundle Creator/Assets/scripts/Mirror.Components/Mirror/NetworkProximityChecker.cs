using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	[Obsolete("Per-NetworkIdentity Interest Management is being replaced by global Interest Management.\n\nWe already converted some components to the new system. For those, please remove Proximity checkers from NetworkIdentity prefabs and add one global InterestManagement component to your NetworkManager instead. If we didn't convert this one yet, then simply wait. See our Benchmark example and our Mirror/Components/InterestManagement for available implementations.\n\nIf you need to port a custom solution, move your code into a new class that inherits from InterestManagement and add one global update method instead of using NetworkBehaviour.Update.\n\nDon't panic. The whole change mostly moved code from NetworkVisibility components into one global place on NetworkManager. Allows for Spatial Hashing which is ~30x faster.\n\n(╯°□°)╯︵ ┻━┻")]
	[AddComponentMenu("Network/NetworkProximityChecker")]
	[RequireComponent(typeof(NetworkIdentity))]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-proximity-checker")]
	public class NetworkProximityChecker : NetworkVisibility
	{
		[Tooltip("The maximum range that objects will be visible at.")]
		public int visRange = 10;

		[Tooltip("How often (in seconds) that this object should update the list of observers that can see it.")]
		public float visUpdateInterval = 1f;

		[Obsolete("Use NetworkIdentity.visible mode instead of forceHidden!")]
		public bool forceHidden
		{
			get
			{
				return base.netIdentity.visible == Visibility.ForceHidden;
			}
			set
			{
				base.netIdentity.visible = (value ? Visibility.ForceHidden : Visibility.Default);
			}
		}

		public override void OnStartServer()
		{
			InvokeRepeating("RebuildObservers", 0f, visUpdateInterval);
		}

		public override void OnStopServer()
		{
			CancelInvoke("RebuildObservers");
		}

		private void RebuildObservers()
		{
			base.netIdentity.RebuildObservers(false);
		}

		public override bool OnCheckObserver(NetworkConnection conn)
		{
			if (forceHidden)
			{
				return false;
			}
			return Vector3.Distance(conn.identity.transform.position, base.transform.position) < (float)visRange;
		}

		public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
		{
			if (forceHidden)
			{
				return;
			}
			Vector3 position = base.transform.position;
			foreach (NetworkConnectionToClient value in NetworkServer.connections.Values)
			{
				if (value != null && value.identity != null && Vector3.Distance(value.identity.transform.position, position) < (float)visRange)
				{
					observers.Add(value);
				}
			}
		}

		private void MirrorProcessed()
		{
		}
	}
}
