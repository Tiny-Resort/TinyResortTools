using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	[DisallowMultipleComponent]
	public abstract class InterestManagement : MonoBehaviour
	{
		private void Awake()
		{
			if (NetworkServer.aoi == null)
			{
				NetworkServer.aoi = this;
			}
			else
			{
				Debug.LogError(string.Format("Only one InterestManagement component allowed. {0} has been set up already.", NetworkServer.aoi.GetType()));
			}
			if (NetworkClient.aoi == null)
			{
				NetworkClient.aoi = this;
			}
			else
			{
				Debug.LogError(string.Format("Only one InterestManagement component allowed. {0} has been set up already.", NetworkClient.aoi.GetType()));
			}
		}

		public abstract bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver);

		public abstract void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize);

		protected void RebuildAll()
		{
			foreach (NetworkIdentity value in NetworkIdentity.spawned.Values)
			{
				NetworkServer.RebuildObservers(value, false);
			}
		}

		public virtual void SetHostVisibility(NetworkIdentity identity, bool visible)
		{
			Renderer[] componentsInChildren = identity.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = visible;
			}
		}

		public virtual void OnSpawned(NetworkIdentity identity)
		{
		}

		public virtual void OnDestroyed(NetworkIdentity identity)
		{
		}
	}
}
