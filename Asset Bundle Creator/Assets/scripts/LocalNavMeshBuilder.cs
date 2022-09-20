using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-102)]
public class LocalNavMeshBuilder : MonoBehaviour
{
	public Transform m_Tracked;

	public Vector3 m_Size = new Vector3(80f, 20f, 80f);

	private NavMeshData m_NavMesh;

	private AsyncOperation m_Operation;

	private NavMeshDataInstance m_Instance;

	public static List<NavMeshBuildSource> m_Sources = new List<NavMeshBuildSource>();

	private NavMeshBuildSettings defaultBuildSettings;

	private bool buildNow;

	private Bounds bounds;

	private IEnumerator Start()
	{
		bounds = QuantizedBounds();
		defaultBuildSettings = NavMesh.GetSettingsByID(0);
		defaultBuildSettings.overrideVoxelSize = true;
		defaultBuildSettings.voxelSize = 0.333f;
		defaultBuildSettings.minRegionArea = 0f;
		while (true)
		{
			if (!buildNow)
			{
				yield return null;
				continue;
			}
			buildNow = false;
			UpdateNavMesh(true);
			while (!m_Operation.isDone)
			{
				yield return null;
			}
		}
	}

	private void OnEnable()
	{
		m_NavMesh = new NavMeshData();
		m_Instance = NavMesh.AddNavMeshData(m_NavMesh);
		if (m_Tracked == null)
		{
			m_Tracked = base.transform;
		}
		UpdateNavMesh();
	}

	private void OnDisable()
	{
		m_Instance.Remove();
	}

	private void UpdateNavMesh(bool asyncUpdate = false)
	{
		if (asyncUpdate)
		{
			m_Operation = NavMeshBuilder.UpdateNavMeshDataAsync(m_NavMesh, defaultBuildSettings, m_Sources, bounds);
		}
		else
		{
			NavMeshBuilder.UpdateNavMeshData(m_NavMesh, defaultBuildSettings, m_Sources, bounds);
		}
		m_Sources.Clear();
	}

	public void InstantRebuild()
	{
		UpdateNavMesh();
	}

	public void refreshBuildNow()
	{
		buildNow = true;
	}

	private static Vector3 Quantize(Vector3 v, Vector3 quant)
	{
		float x = quant.x * Mathf.Floor(v.x / quant.x);
		float y = quant.y * Mathf.Floor(v.y / quant.y);
		float z = quant.z * Mathf.Floor(v.z / quant.z);
		return new Vector3(x, y, z);
	}

	private Bounds QuantizedBounds()
	{
		return new Bounds(Quantize(m_Tracked ? m_Tracked.position : base.transform.position, 0.1f * m_Size), m_Size);
	}

	private void OnDrawGizmosSelected()
	{
		if ((bool)m_NavMesh)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(m_NavMesh.sourceBounds.center, m_NavMesh.sourceBounds.size);
		}
		Gizmos.color = Color.yellow;
		Bounds bounds = QuantizedBounds();
		Gizmos.DrawWireCube(bounds.center, bounds.size);
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(m_Tracked ? m_Tracked.position : base.transform.position, m_Size);
	}
}
