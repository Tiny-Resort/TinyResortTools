using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Mirror
{
	public class SceneInterestManagement : InterestManagement
	{
		private readonly Dictionary<Scene, HashSet<NetworkIdentity>> sceneObjects = new Dictionary<Scene, HashSet<NetworkIdentity>>();

		private readonly Dictionary<NetworkIdentity, Scene> lastObjectScene = new Dictionary<NetworkIdentity, Scene>();

		private HashSet<Scene> dirtyScenes = new HashSet<Scene>();

		public override void OnSpawned(NetworkIdentity identity)
		{
			Scene scene = identity.gameObject.scene;
			lastObjectScene[identity] = scene;
			HashSet<NetworkIdentity> value;
			if (!sceneObjects.TryGetValue(scene, out value))
			{
				value = new HashSet<NetworkIdentity>();
				sceneObjects.Add(scene, value);
			}
			value.Add(identity);
		}

		public override void OnDestroyed(NetworkIdentity identity)
		{
			Scene scene = lastObjectScene[identity];
			lastObjectScene.Remove(identity);
			HashSet<NetworkIdentity> value;
			if (sceneObjects.TryGetValue(scene, out value) && value.Remove(identity))
			{
				RebuildSceneObservers(scene);
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
				Scene scene = lastObjectScene[value];
				Scene scene2 = value.gameObject.scene;
				if (!(scene2 == scene))
				{
					dirtyScenes.Add(scene);
					dirtyScenes.Add(scene2);
					sceneObjects[scene].Remove(value);
					lastObjectScene[value] = scene2;
					if (!sceneObjects.ContainsKey(scene2))
					{
						sceneObjects.Add(scene2, new HashSet<NetworkIdentity>());
					}
					sceneObjects[scene2].Add(value);
				}
			}
			foreach (Scene dirtyScene in dirtyScenes)
			{
				RebuildSceneObservers(dirtyScene);
			}
			dirtyScenes.Clear();
		}

		private void RebuildSceneObservers(Scene scene)
		{
			foreach (NetworkIdentity item in sceneObjects[scene])
			{
				if (item != null)
				{
					NetworkServer.RebuildObservers(item, false);
				}
			}
		}

		public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
		{
			return identity.gameObject.scene == newObserver.identity.gameObject.scene;
		}

		public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
		{
			HashSet<NetworkIdentity> value;
			if (!sceneObjects.TryGetValue(identity.gameObject.scene, out value))
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
