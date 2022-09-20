using UnityEngine;

public class VehiclePreview : MonoBehaviour
{
	public LayerMask checkForCollision;

	public MeshRenderer ren;

	public Material yesMat;

	public Material noMat;

	public bool canBePlaced;

	private void Update()
	{
		canBePlaced = checkIfCanBePlaced();
	}

	public bool checkIfCanBePlaced()
	{
		if (Physics.CheckBox(base.transform.position + Vector3.up * 1.5f, new Vector3(1.5f, 1f, 2f), base.transform.rotation, checkForCollision))
		{
			ren.sharedMaterial = noMat;
			return false;
		}
		ren.sharedMaterial = yesMat;
		return true;
	}
}
