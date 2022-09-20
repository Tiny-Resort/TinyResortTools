using UnityEngine;

public class CopyShopItem : MonoBehaviour
{
	public MeshRenderer ren;

	public MeshFilter mesh;

	public void changeMesh(Mesh newMesh)
	{
		mesh.mesh = newMesh;
		ren.gameObject.SetActive(true);
	}

	public void changeMaterial(Material newMat)
	{
		ren.material = newMat;
		ren.gameObject.SetActive(true);
	}

	public void disable()
	{
		ren.gameObject.SetActive(false);
	}
}
