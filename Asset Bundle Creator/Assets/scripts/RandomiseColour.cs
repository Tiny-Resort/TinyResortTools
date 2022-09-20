using UnityEngine;

public class RandomiseColour : MonoBehaviour
{
	public MeshFilter colours;

	public MeshFilter lowColours;

	public Mesh[] meshes;

	public Mesh[] lowMeshes;

	private void OnEnable()
	{
		Random.InitState((int)(base.transform.position.x * base.transform.position.z + base.transform.position.y));
		int num = Random.Range(0, meshes.Length);
		colours.sharedMesh = meshes[num];
		lowColours.sharedMesh = lowMeshes[num];
	}
}
