using System.Collections.Generic;
using UnityEngine;

public static class MergeMeshes
{
	public static Mesh CombineMeshes(this GameObject aGo)
	{
		MeshRenderer[] componentsInChildren = aGo.GetComponentsInChildren<MeshRenderer>(false);
		int num = 0;
		int num2 = 0;
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			MeshRenderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				MeshFilter component = array[i].gameObject.GetComponent<MeshFilter>();
				if (component != null && component.sharedMesh != null)
				{
					num += component.sharedMesh.vertexCount;
					num2++;
				}
			}
		}
		switch (num2)
		{
		case 0:
			Debug.Log("No meshes found in children. There's nothing to combine.");
			return null;
		case 1:
			Debug.Log("Only 1 mesh found in children. There's nothing to combine.");
			return null;
		default:
		{
			if (num > 65535)
			{
				Debug.Log("There are too many vertices to combine into 1 mesh (" + num + "). The max. limit is 65535");
				return null;
			}
			Mesh mesh = new Mesh();
			Matrix4x4 worldToLocalMatrix = aGo.transform.worldToLocalMatrix;
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector2> list3 = new List<Vector2>();
			List<Vector2> list4 = new List<Vector2>();
			Dictionary<Material, List<int>> dictionary = new Dictionary<Material, List<int>>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				MeshRenderer[] array = componentsInChildren;
				foreach (MeshRenderer meshRenderer in array)
				{
					MeshFilter component2 = meshRenderer.gameObject.GetComponent<MeshFilter>();
					if (component2 != null && component2.sharedMesh != null)
					{
						MergeMeshInto(component2.sharedMesh, meshRenderer.sharedMaterials, worldToLocalMatrix * component2.transform.localToWorldMatrix, list, list2, list3, list4, dictionary);
						if (component2.gameObject != aGo)
						{
							component2.gameObject.SetActive(false);
						}
					}
				}
			}
			mesh.vertices = list.ToArray();
			if (list2.Count > 0)
			{
				mesh.normals = list2.ToArray();
			}
			if (list3.Count > 0)
			{
				mesh.uv = list3.ToArray();
			}
			if (list4.Count > 0)
			{
				mesh.uv2 = list4.ToArray();
			}
			mesh.subMeshCount = dictionary.Keys.Count;
			Material[] array2 = new Material[dictionary.Keys.Count];
			int num3 = 0;
			using (Dictionary<Material, List<int>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					mesh.SetTriangles(dictionary[array2[num3] = enumerator.Current].ToArray(), num3++);
				}
			}
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				MeshRenderer meshRenderer2 = aGo.GetComponent<MeshRenderer>();
				if (meshRenderer2 == null)
				{
					meshRenderer2 = aGo.AddComponent<MeshRenderer>();
				}
				meshRenderer2.sharedMaterials = array2;
				MeshFilter meshFilter = aGo.GetComponent<MeshFilter>();
				if (meshFilter == null)
				{
					meshFilter = aGo.AddComponent<MeshFilter>();
				}
				meshFilter.sharedMesh = mesh;
				aGo.GetComponent<MeshCollider>().sharedMesh = mesh;
			}
			return mesh;
		}
		}
	}

	private static void MergeMeshInto(Mesh meshToMerge, Material[] ms, Matrix4x4 transformMatrix, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s, Dictionary<Material, List<int>> subMeshes)
	{
		if (meshToMerge == null)
		{
			return;
		}
		int count = vertices.Count;
		Vector3[] vertices2 = meshToMerge.vertices;
		for (int i = 0; i < vertices2.Length; i++)
		{
			vertices2[i] = transformMatrix.MultiplyPoint3x4(vertices2[i]);
		}
		vertices.AddRange(vertices2);
		Quaternion quaternion = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
		Vector3[] normals2 = meshToMerge.normals;
		if (normals2 != null && normals2.Length != 0)
		{
			for (int j = 0; j < normals2.Length; j++)
			{
				normals2[j] = quaternion * normals2[j];
			}
			normals.AddRange(normals2);
		}
		Vector2[] uv = meshToMerge.uv;
		if (uv != null && uv.Length != 0)
		{
			uv1s.AddRange(uv);
		}
		uv = meshToMerge.uv2;
		if (uv != null && uv.Length != 0)
		{
			uv2s.AddRange(uv);
		}
		for (int k = 0; k < ms.Length; k++)
		{
			if (k >= meshToMerge.subMeshCount)
			{
				continue;
			}
			int[] triangles = meshToMerge.GetTriangles(k);
			if (triangles.Length != 0)
			{
				if (ms[k] != null && !subMeshes.ContainsKey(ms[k]))
				{
					subMeshes.Add(ms[k], new List<int>());
				}
				List<int> list = subMeshes[ms[k]];
				for (int l = 0; l < triangles.Length; l++)
				{
					triangles[l] += count;
				}
				list.AddRange(triangles);
			}
		}
	}
}
