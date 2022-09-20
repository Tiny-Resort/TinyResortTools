using System;
using System.Collections.Generic;

namespace Mirror
{
	public class MatchInterestManagement : InterestManagement
	{
		private readonly Dictionary<Guid, HashSet<NetworkIdentity>> matchObjects = new Dictionary<Guid, HashSet<NetworkIdentity>>();

		private readonly Dictionary<NetworkIdentity, Guid> lastObjectMatch = new Dictionary<NetworkIdentity, Guid>();

		private HashSet<Guid> dirtyMatches = new HashSet<Guid>();

		public override void OnSpawned(NetworkIdentity identity)
		{
			Guid matchId = identity.GetComponent<NetworkMatch>().matchId;
			lastObjectMatch[identity] = matchId;
			if (!(matchId == Guid.Empty))
			{
				HashSet<NetworkIdentity> value;
				if (!matchObjects.TryGetValue(matchId, out value))
				{
					value = new HashSet<NetworkIdentity>();
					matchObjects.Add(matchId, value);
				}
				value.Add(identity);
			}
		}

		public override void OnDestroyed(NetworkIdentity identity)
		{
			Guid guid = lastObjectMatch[identity];
			lastObjectMatch.Remove(identity);
			HashSet<NetworkIdentity> value;
			if (guid != Guid.Empty && matchObjects.TryGetValue(guid, out value) && value.Remove(identity))
			{
				RebuildMatchObservers(guid);
			}
		}

		private void Update()
		{
			if (!NetworkServer.active)
			{
				return;
			}
			foreach (NetworkIdentity value in NetworkIdentity.spawned.Values)
			{
				Guid guid = lastObjectMatch[value];
				Guid matchId = value.GetComponent<NetworkMatch>().matchId;
				if (matchId == guid)
				{
					continue;
				}
				if (guid != Guid.Empty)
				{
					dirtyMatches.Add(guid);
				}
				dirtyMatches.Add(matchId);
				if (guid != Guid.Empty)
				{
					matchObjects[guid].Remove(value);
				}
				lastObjectMatch[value] = matchId;
				if (!(matchId == Guid.Empty))
				{
					if (!matchObjects.ContainsKey(matchId))
					{
						matchObjects.Add(matchId, new HashSet<NetworkIdentity>());
					}
					matchObjects[matchId].Add(value);
				}
			}
			foreach (Guid dirtyMatch in dirtyMatches)
			{
				RebuildMatchObservers(dirtyMatch);
			}
			dirtyMatches.Clear();
		}

		private void RebuildMatchObservers(Guid matchId)
		{
			foreach (NetworkIdentity item in matchObjects[matchId])
			{
				if (item != null)
				{
					NetworkServer.RebuildObservers(item, false);
				}
			}
		}

		public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
		{
			return identity.GetComponent<NetworkMatch>().matchId == newObserver.identity.GetComponent<NetworkMatch>().matchId;
		}

		public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
		{
			Guid matchId = identity.GetComponent<NetworkMatch>().matchId;
			HashSet<NetworkIdentity> value;
			if (matchId == Guid.Empty || !matchObjects.TryGetValue(matchId, out value))
			{
				return;
			}
			foreach (NetworkIdentity item in value)
			{
				if (item != null && item.connectionToClient != null)
				{
					newObservers.Add(item.connectionToClient);
				}
			}
		}
	}
}
