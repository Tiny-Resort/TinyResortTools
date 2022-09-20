using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	public class SpatialHashingInterestManagement : InterestManagement
	{
		public enum CheckMethod
		{
			XZ_FOR_3D = 0,
			XY_FOR_2D = 1
		}

		[Tooltip("The maximum range that objects will be visible at.")]
		public int visRange = 30;

		[Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
		public float rebuildInterval = 1f;

		private double lastRebuildTime;

		[Tooltip("Spatial Hashing supports 3D (XZ) and 2D (XY) games.")]
		public CheckMethod checkMethod;

		public bool showSlider;

		private Grid2D<NetworkConnection> grid = new Grid2D<NetworkConnection>();

		public int resolution
		{
			get
			{
				return visRange / 3;
			}
		}

		private Vector2Int ProjectToGrid(Vector3 position)
		{
			if (checkMethod != 0)
			{
				return Vector2Int.RoundToInt(new Vector2(position.x, position.y) / resolution);
			}
			return Vector2Int.RoundToInt(new Vector2(position.x, position.z) / resolution);
		}

		public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
		{
			Vector2Int vector2Int = ProjectToGrid(identity.transform.position);
			Vector2Int vector2Int2 = ProjectToGrid(newObserver.identity.transform.position);
			return (vector2Int - vector2Int2).sqrMagnitude <= 2;
		}

		public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
		{
			Vector2Int position = ProjectToGrid(identity.transform.position);
			grid.GetWithNeighbours(position, newObservers);
		}

		internal void Update()
		{
			if (!NetworkServer.active)
			{
				return;
			}
			grid.ClearNonAlloc();
			foreach (NetworkConnectionToClient value in NetworkServer.connections.Values)
			{
				if (value.isAuthenticated && value.identity != null)
				{
					Vector2Int position = ProjectToGrid(value.identity.transform.position);
					grid.Add(position, value);
				}
			}
			if (NetworkTime.localTime >= lastRebuildTime + (double)rebuildInterval)
			{
				RebuildAll();
				lastRebuildTime = NetworkTime.localTime;
			}
		}

		private void OnGUI()
		{
			if (showSlider && NetworkServer.active)
			{
				int num = 30;
				int num2 = 250;
				GUILayout.BeginArea(new Rect(Screen.width / 2 - num2 / 2, Screen.height - num, num2, num));
				GUILayout.BeginHorizontal("Box");
				GUILayout.Label("Radius:");
				visRange = Mathf.RoundToInt(GUILayout.HorizontalSlider(visRange, 0f, 200f, GUILayout.Width(150f)));
				GUILayout.Label(visRange.ToString());
				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			}
		}
	}
}
