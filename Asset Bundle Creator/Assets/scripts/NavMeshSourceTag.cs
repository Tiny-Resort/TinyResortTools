using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-200)]
public class NavMeshSourceTag : MonoBehaviour
{
	public static List<MeshFilter> m_Meshes = new List<MeshFilter>();

	public static List<MeshFilter> w_Meshes = new List<MeshFilter>();

	public MeshFilter m;

	public NavMeshBuildSource mySource;

	private bool isOn = true;

	private void Awake()
	{
		getSource();
	}

	public void forceStartForBuildingPlacement(int areaId)
	{
		mySource.shape = NavMeshBuildSourceShape.Mesh;
		mySource.sourceObject = m.sharedMesh;
		mySource.transform = m.transform.localToWorldMatrix;
		mySource.area = areaId;
		LocalNavMeshBuilder.m_Sources.Add(mySource);
	}

	public void forceStartForAnimalFloor(int areaId)
	{
		mySource.shape = NavMeshBuildSourceShape.Mesh;
		mySource.sourceObject = m.sharedMesh;
		mySource.transform = m.transform.localToWorldMatrix;
		mySource.area = areaId;
		LocalNavMeshBuilder.m_Sources.Add(mySource);
	}

	public void refreshPositonAndBuild()
	{
		if (isOn)
		{
			mySource.transform = m.transform.localToWorldMatrix;
			LocalNavMeshBuilder.m_Sources.Add(mySource);
		}
	}

	public void refreshBuildOnly()
	{
		if (isOn)
		{
			LocalNavMeshBuilder.m_Sources.Add(mySource);
		}
	}

	public void setPath()
	{
		mySource.area = 4;
	}

	public void enableGate()
	{
		isOn = true;
		mySource.area = 5;
	}

	public void enableIt(bool isWater)
	{
		isOn = true;
		if (isWater)
		{
			mySource.area = 3;
		}
		else
		{
			mySource.area = 0;
		}
	}

	public void disableIt()
	{
		isOn = false;
	}

	public void changeWater(bool currentlyWater)
	{
		if (currentlyWater)
		{
			mySource.area = 3;
		}
		else
		{
			mySource.area = 0;
		}
	}

	public void getSource()
	{
		if ((bool)m.sharedMesh)
		{
			mySource.shape = NavMeshBuildSourceShape.Mesh;
			mySource.sourceObject = m.sharedMesh;
			mySource.transform = m.transform.localToWorldMatrix;
			mySource.area = 0;
		}
	}

	public void updateSourceMesh()
	{
		mySource.sourceObject = m.sharedMesh;
	}

	public static void Collect(ref List<NavMeshBuildSource> sources)
	{
	}
}
