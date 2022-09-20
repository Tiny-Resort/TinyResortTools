using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	[Obsolete("Per-NetworkIdentity Interest Management is being replaced by global Interest Management.\n\nWe already converted some components to the new system. For those, please remove Proximity checkers from NetworkIdentity prefabs and add one global InterestManagement component to your NetworkManager instead. If we didn't convert this one yet, then simply wait. See our Benchmark example and our Mirror/Components/InterestManagement for available implementations.\n\nIf you need to port a custom solution, move your code into a new class that inherits from InterestManagement and add one global update method instead of using NetworkBehaviour.Update.\n\nDon't panic. The whole change mostly moved code from NetworkVisibility components into one global place on NetworkManager. Allows for Spatial Hashing which is ~30x faster.\n\n(╯°□°)╯︵ ┻━┻")]
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkMatchChecker")]
	[RequireComponent(typeof(NetworkIdentity))]
	[RequireComponent(typeof(NetworkMatch))]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-match-checker")]
	public class NetworkMatchChecker : NetworkVisibility
	{
		internal static readonly Dictionary<Guid, HashSet<NetworkIdentity>> matchPlayers = new Dictionary<Guid, HashSet<NetworkIdentity>>();

		internal Guid lastMatch;

		internal Guid currentMatch
		{
			get
			{
				return GetComponent<NetworkMatch>().matchId;
			}
			set
			{
				GetComponent<NetworkMatch>().matchId = value;
			}
		}

		public override void OnStartServer()
		{
			if (!(currentMatch == Guid.Empty))
			{
				if (!matchPlayers.ContainsKey(currentMatch))
				{
					matchPlayers.Add(currentMatch, new HashSet<NetworkIdentity>());
				}
				matchPlayers[currentMatch].Add(base.netIdentity);
			}
		}

		public override void OnStopServer()
		{
			if (!(currentMatch == Guid.Empty) && matchPlayers.ContainsKey(currentMatch) && matchPlayers[currentMatch].Remove(base.netIdentity))
			{
				RebuildMatchObservers(currentMatch);
			}
		}

		private void RebuildMatchObservers(Guid specificMatch)
		{
			foreach (NetworkIdentity item in matchPlayers[specificMatch])
			{
				if ((object)item != null)
				{
					item.RebuildObservers(false);
				}
			}
		}

		public override bool OnCheckObserver(NetworkConnection conn)
		{
			if (currentMatch == Guid.Empty)
			{
				return false;
			}
			NetworkMatchChecker component = conn.identity.GetComponent<NetworkMatchChecker>();
			if (component == null)
			{
				return false;
			}
			return component.currentMatch == currentMatch;
		}

		public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
		{
			if (currentMatch == Guid.Empty)
			{
				return;
			}
			foreach (NetworkIdentity item in matchPlayers[currentMatch])
			{
				if (item != null && item.connectionToClient != null)
				{
					observers.Add(item.connectionToClient);
				}
			}
		}

		[ServerCallback]
		private void Update()
		{
			if (!NetworkServer.active || currentMatch == lastMatch)
			{
				return;
			}
			if (lastMatch != Guid.Empty)
			{
				matchPlayers[lastMatch].Remove(base.netIdentity);
				RebuildMatchObservers(lastMatch);
			}
			if (currentMatch != Guid.Empty)
			{
				if (!matchPlayers.ContainsKey(currentMatch))
				{
					matchPlayers.Add(currentMatch, new HashSet<NetworkIdentity>());
				}
				matchPlayers[currentMatch].Add(base.netIdentity);
				RebuildMatchObservers(currentMatch);
			}
			else
			{
				base.netIdentity.RebuildObservers(false);
			}
			lastMatch = currentMatch;
		}

		private void MirrorProcessed()
		{
		}
	}
}
