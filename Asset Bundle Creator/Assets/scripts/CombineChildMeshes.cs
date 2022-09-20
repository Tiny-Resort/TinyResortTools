using UnityEngine;

public class CombineChildMeshes : MonoBehaviour
{
	public MeshFilter[] childrenToCombine;

	public MeshFilter finalFilter;

	public void combineChildren()
	{
		Quaternion localRotation = finalFilter.transform.localRotation;
		Vector3 localPosition = finalFilter.transform.localPosition;
		finalFilter.transform.position = Vector3.zero;
		finalFilter.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		CombineInstance[] array = new CombineInstance[childrenToCombine.Length];
		for (int i = 0; i < childrenToCombine.Length; i++)
		{
			array[i].mesh = childrenToCombine[i].sharedMesh;
			array[i].transform = childrenToCombine[i].transform.localToWorldMatrix;
			childrenToCombine[i].gameObject.SetActive(false);
		}
		finalFilter.sharedMesh = new Mesh();
		finalFilter.sharedMesh.name = base.transform.root.name + " Combined Mesh";
		finalFilter.sharedMesh.CombineMeshes(array);
		finalFilter.transform.localRotation = localRotation;
		finalFilter.transform.localPosition = localPosition;
	}
}
