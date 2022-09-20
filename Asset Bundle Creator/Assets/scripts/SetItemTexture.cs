using UnityEngine;

public class SetItemTexture : MonoBehaviour
{
	public MeshRenderer itemRenderer;

	public SkinnedMeshRenderer itemSkinRen;

	public Transform changeSize;

	public bool useAltMesh = true;

	public Transform spawnObjectHere;

	public int matNo = -1;

	public void setTexture(InventoryItem item)
	{
		if (matNo != -1)
		{
			Material[] sharedMaterials = itemRenderer.sharedMaterials;
			sharedMaterials[matNo] = item.equipable.material;
			itemRenderer.sharedMaterials = sharedMaterials;
			return;
		}
		if ((bool)itemRenderer)
		{
			itemRenderer.material = item.equipable.material;
			if (useAltMesh && (bool)item.equipable.useAltMesh)
			{
				itemRenderer.GetComponent<MeshFilter>().mesh = item.equipable.useAltMesh;
			}
		}
		if ((bool)itemSkinRen)
		{
			itemSkinRen.material = item.equipable.material;
			if (useAltMesh && (bool)item.equipable.useAltMesh)
			{
				itemSkinRen.sharedMesh = item.equipable.useAltMesh;
			}
		}
		if ((bool)item.bug)
		{
			GameObject obj = Object.Instantiate(item.bug.insectType, spawnObjectHere);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.GetComponent<BugAppearance>().setUpBug(item);
			obj.GetComponent<Animator>().SetTrigger("Captured");
			Object.Destroy(obj.GetComponentInChildren<BoxCollider>());
		}
		if ((bool)item.underwaterCreature)
		{
			GameObject gameObject = Object.Instantiate(item.underwaterCreature.creatureModel, spawnObjectHere);
			gameObject.transform.localPosition = Vector3.zero;
			if (item.underwaterCreature.turn90DegreesInHand)
			{
				gameObject.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			}
			else
			{
				gameObject.transform.localRotation = Quaternion.identity;
			}
			gameObject.GetComponent<BugAppearance>().setUpBug(item);
			if ((bool)gameObject.GetComponent<Animator>())
			{
				gameObject.GetComponent<Animator>().SetTrigger("Captured");
			}
			Object.Destroy(gameObject.GetComponentInChildren<BoxCollider>());
		}
	}

	public void changeSizeOfTrans(Vector3 scale)
	{
		changeSize.localScale = scale;
	}
}
