using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	[Obsolete("Per-NetworkIdentity Interest Management is being replaced by global Interest Management.\n\nWe already converted some components to the new system. For those, please remove Proximity checkers from NetworkIdentity prefabs and add one global InterestManagement component to your NetworkManager instead. If we didn't convert this one yet, then simply wait. See our Benchmark example and our Mirror/Components/InterestManagement for available implementations.\n\nIf you need to port a custom solution, move your code into a new class that inherits from InterestManagement and add one global update method instead of using NetworkBehaviour.Update.\n\nDon't panic. The whole change mostly moved code from NetworkVisibility components into one global place on NetworkManager. Allows for Spatial Hashing which is ~30x faster.\n\n(╯°□°)╯︵ ┻━┻")]
	[DisallowMultipleComponent]
	public abstract class NetworkVisibility : NetworkBehaviour
	{
		public abstract bool OnCheckObserver(NetworkConnection conn);

		public abstract void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize);

		public virtual void OnSetHostVisibility(bool visible)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = visible;
			}
		}
	}
}
