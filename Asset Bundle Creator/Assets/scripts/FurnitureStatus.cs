using UnityEngine;

public class FurnitureStatus : MonoBehaviour
{
	public GameObject seatPosition1;

	public GameObject seatPosition2;

	public GameObject cover;

	public bool isSeat = true;

	public int showingX;

	public int showingY;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isFurniture = this;
	}

	public void updateOnTileStatus(int newX, int newY, HouseDetails inside = null)
	{
		showingX = newX;
		showingY = newY;
		int num = WorldManager.manageWorld.onTileStatusMap[showingX, showingY];
		if (inside != null)
		{
			num = inside.houseMapOnTileStatus[showingX, showingY];
		}
		if ((bool)cover)
		{
			cover.SetActive(num != 0);
		}
		switch (num)
		{
		case 0:
			enableSeat(seatPosition1);
			enableSeat(seatPosition2);
			break;
		case 1:
			disableSeat(seatPosition1);
			enableSeat(seatPosition2);
			break;
		case 2:
			enableSeat(seatPosition1);
			disableSeat(seatPosition2);
			break;
		case 3:
			disableSeat(seatPosition1);
			disableSeat(seatPosition2);
			break;
		}
		if (inside == null && WorldManager.manageWorld.waterMap[showingX, showingY] && WorldManager.manageWorld.heightMap[showingX, showingY] <= -1)
		{
			disableSeat(seatPosition1);
			disableSeat(seatPosition2);
		}
	}

	public void disableSeat(GameObject disable)
	{
		if ((bool)disable)
		{
			disable.SetActive(false);
		}
	}

	public void enableSeat(GameObject enable)
	{
		if ((bool)enable)
		{
			enable.SetActive(true);
		}
	}
}
