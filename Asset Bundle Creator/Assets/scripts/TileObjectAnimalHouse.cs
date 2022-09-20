using UnityEngine;

public class TileObjectAnimalHouse : MonoBehaviour
{
	public int[] canHouseAnimals;

	public string houseTypeName = "Coop";

	public GameObject houseNavTileFloor;

	private TileObject myTileObject;

	private void Start()
	{
		myTileObject = GetComponent<TileObject>();
		base.gameObject.AddComponent<InteractableObject>().isAnimalHouse = this;
	}

	public void showHouseDetails()
	{
	}

	public int myXPos()
	{
		return myTileObject.xPos;
	}

	public int myYPos()
	{
		return myTileObject.yPos;
	}
}
