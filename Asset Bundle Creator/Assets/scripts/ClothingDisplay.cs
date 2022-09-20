using UnityEngine;

public class ClothingDisplay : MonoBehaviour
{
	public MeshRenderer ren;

	public Transform hatPos;

	private GameObject headObject;

	public SetItemTexture fishTexture;

	public Transform bugPos;

	private int showingId = -1;

	private GameObject myBugModel;

	public void updateStatus(int clothingId)
	{
		if (clothingId <= -1)
		{
			return;
		}
		if ((bool)Inventory.inv.allItems[clothingId].bug)
		{
			if (showingId != clothingId)
			{
				if (myBugModel != null)
				{
					Object.Destroy(myBugModel.gameObject);
				}
				myBugModel = Object.Instantiate(Inventory.inv.allItems[clothingId].bug.insectType, bugPos);
				myBugModel.transform.localPosition = Vector3.zero;
				myBugModel.transform.localRotation = Quaternion.identity;
				myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.inv.allItems[clothingId]);
				myBugModel.GetComponent<Animator>().SetTrigger("Captured");
				showingId = clothingId;
			}
			else
			{
				myBugModel.GetComponent<Animator>().SetTrigger("Captured");
			}
		}
		else if ((bool)Inventory.inv.allItems[clothingId].fish)
		{
			if (showingId != clothingId)
			{
				fishTexture.setTexture(Inventory.inv.allItems[clothingId]);
				fishTexture.changeSizeOfTrans(Inventory.inv.allItems[clothingId].transform.localScale);
				fishTexture.gameObject.SetActive(true);
				fishTexture.GetComponentInChildren<Animator>().SetFloat("Offset", Random.Range(0f, 1f));
				showingId = clothingId;
			}
		}
		else if (((bool)Inventory.inv.allItems[clothingId].equipable && Inventory.inv.allItems[clothingId].equipable.hat) || ((bool)Inventory.inv.allItems[clothingId].equipable && Inventory.inv.allItems[clothingId].equipable.face))
		{
			if (headObject != null)
			{
				Object.Destroy(headObject);
			}
			headObject = Object.Instantiate(Inventory.inv.allItems[clothingId].equipable.hatPrefab, hatPos);
			headObject.transform.localPosition = Vector3.zero;
			headObject.transform.localRotation = Quaternion.identity;
			if ((bool)headObject.GetComponent<SetItemTexture>())
			{
				headObject.GetComponent<SetItemTexture>().setTexture(Inventory.inv.allItems[clothingId]);
			}
		}
		else if (((bool)Inventory.inv.allItems[clothingId].equipable && Inventory.inv.allItems[clothingId].equipable.shirt) || ((bool)Inventory.inv.allItems[clothingId].equipable && Inventory.inv.allItems[clothingId].equipable.pants) || ((bool)Inventory.inv.allItems[clothingId].equipable && Inventory.inv.allItems[clothingId].equipable.shoes))
		{
			if ((bool)Inventory.inv.allItems[clothingId].equipable.shirtMesh)
			{
				ren.GetComponent<MeshFilter>().mesh = Inventory.inv.allItems[clothingId].equipable.shirtMesh;
			}
			else if ((bool)Inventory.inv.allItems[clothingId].equipable.useAltMesh)
			{
				ren.GetComponent<MeshFilter>().mesh = Inventory.inv.allItems[clothingId].equipable.useAltMesh;
			}
			else if (Inventory.inv.allItems[clothingId].equipable.shirt)
			{
				ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultShirtMesh;
			}
			else if (Inventory.inv.allItems[clothingId].equipable.pants)
			{
				ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultPants;
			}
			else if (Inventory.inv.allItems[clothingId].equipable.shoes)
			{
				ren.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defualtShoeMesh;
			}
			ren.material = Inventory.inv.allItems[clothingId].equipable.material;
		}
	}
}
