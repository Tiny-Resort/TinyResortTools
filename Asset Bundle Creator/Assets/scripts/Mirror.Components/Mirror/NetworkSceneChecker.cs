using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror
{
	[Obsolete("Per-NetworkIdentity Interest Management is being replaced by global Interest Management.\n\nWe already converted some components to the new system. For those, please remove Proximity checkers from NetworkIdentity prefabs and add one global InterestManagement component to your NetworkManager instead. If we didn't convert this one yet, then simply wait. See our Benchmark example and our Mirror/Components/InterestManagement for available implementations.\n\nIf you need to port a custom solution, move your code into a new class that inherits from InterestManagement and add one global update method instead of using NetworkBehaviour.Update.\n\nDon't panic. The whole change mostly moved code from NetworkVisibility components into one global place on NetworkManager. Allows for Spatial Hashing which is ~30x faster.\n\n(╯°□°)╯︵ ┻━┻")]
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkSceneChecker")]
	[RequireComponent(typeof(NetworkIdentity))]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-scene-checker")]
	public class NetworkSceneChecker : NetworkVisibility
	{
		[Tooltip("Enable to force this object to be hidden from all observers.")]
		public bool forceHidden;

		private static readonly Dictionary<Scene, HashSet<NetworkIdentity>> sceneCheckerObjects = new Dictionary<Scene, HashSet<NetworkIdentity>>();

		private Scene currentScene;

		[ServerCallback]
		private void Awake()
		{
			if (NetworkServer.active)
			{
				currentScene = base.gameObject.scene;
			}
		}

		public override void OnStartServer()
		{
			if (!sceneCheckerObjects.ContainsKey(currentScene))
			{
				sceneCheckerObjects.Add(currentScene, new HashSet<NetworkIdentity>());
			}
			sceneCheckerObjects[currentScene].Add(base.netIdentity);
		}

		public override void OnStopServer()
		{
			if (sceneCheckerObjects.ContainsKey(currentScene) && sceneCheckerObjects[currentScene].Remove(base.netIdentity))
			{
				RebuildSceneObservers();
			}
		}

		[ServerCallback]
		private void Update()
		{
			if (NetworkServer.active && !(currentScene == base.gameObject.scene))
			{
				sceneCheckerObjects[currentScene].Remove(base.netIdentity);
				RebuildSceneObservers();
				currentScene = base.gameObject.scene;
				if (!sceneCheckerObjects.ContainsKey(currentScene))
				{
					sceneCheckerObjects.Add(currentScene, new HashSet<NetworkIdentity>());
				}
				sceneCheckerObjects[currentScene].Add(base.netIdentity);
				RebuildSceneObservers();
			}
		}

		private void RebuildSceneObservers()
		{
			foreach (NetworkIdentity item in sceneCheckerObjects[currentScene])
			{
				if (item != null)
				{
					item.RebuildObservers(false);
				}
			}
		}

		public override bool OnCheckObserver(NetworkConnection conn)
		{
			if (forceHidden)
			{
				return false;
			}
			return conn.identity.gameObject.scene == base.gameObject.scene;
		}

		public override void OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
		{
			if (forceHidden)
			{
				return;
			}
			foreach (NetworkIdentity item in sceneCheckerObjects[currentScene])
			{
				if (item != null && item.connectionToClient != null)
				{
					observers.Add(item.connectionToClient);
				}
			}
		}

		private void MirrorProcessed()
		{
		}
	}
}
