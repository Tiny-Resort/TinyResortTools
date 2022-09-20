using UnityEngine;

public class ChestPlaceable : MonoBehaviour
{
	public ASound openSound;

	public ASound closeSound;

	public Animator chestAnim;

	private TileObject myTileObject;

	public bool isStash;

	private void Awake()
	{
		myTileObject = GetComponent<TileObject>();
		base.gameObject.AddComponent<InteractableObject>().isChest = this;
	}

	public bool checkIfEmpty(int xPos, int yPos, HouseDetails inside)
	{
		return ContainerManager.manage.checkIfEmpty(xPos, yPos, inside);
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
