using UnityEngine;

public class Equipable : MonoBehaviour
{
	public bool cloths = true;

	public bool hat;

	public bool face;

	public bool shirt;

	public bool dress;

	public bool pants;

	public bool shoes;

	public bool idol;

	public bool flooring;

	public bool wallpaper;

	public Material material;

	public Mesh shirtMesh;

	public Mesh useAltMesh;

	public GameObject hatPrefab;

	public bool useHelmetHair;

	public bool hideHair;

	public bool useRegularHair;

	public float jumpDif;

	public float runSpeedDif;

	public float swimSpeedDif;

	public int healthSoak;

	public float staminaSoak;

	public bool useOwnSprite;

	public bool canEquipInThisSlot(int inventoryId)
	{
		if (inventoryId == -1)
		{
			return true;
		}
		InventoryItem inventoryItem = Inventory.inv.allItems[inventoryId];
		if ((bool)inventoryItem.equipable && ((face && inventoryItem.equipable.face) || (idol && inventoryItem.equipable.idol) || (hat && inventoryItem.equipable.hat == hat) || (shirt && inventoryItem.equipable.shirt == shirt) || (pants && inventoryItem.equipable.pants == pants) || (shoes && inventoryItem.equipable.shoes == shoes)))
		{
			return true;
		}
		return false;
	}
}
