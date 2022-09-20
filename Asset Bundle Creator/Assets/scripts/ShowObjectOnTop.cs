using UnityEngine;

public class ShowObjectOnTop : MonoBehaviour
{
	public TileObject[] showingTileObjectOnTop = new TileObject[0];

	private Transform[] onTopPositions = new Transform[0];

	public void setUp(Transform[] myPositions)
	{
		if (showingTileObjectOnTop.Length != myPositions.Length)
		{
			clearObjectsOnTopOfMe();
		}
		showingTileObjectOnTop = new TileObject[myPositions.Length];
		onTopPositions = myPositions;
	}

	public void updateItemsOnTopOfMe(ItemOnTop[] itemsOnTopToSpawn)
	{
		for (int i = 0; i < onTopPositions.Length; i++)
		{
			bool flag = false;
			for (int j = 0; j < itemsOnTopToSpawn.Length; j++)
			{
				if (itemsOnTopToSpawn[j].sittingOnTopPosition() == i)
				{
					flag = true;
					getPrefabForPos(itemsOnTopToSpawn[j]);
				}
			}
			if (!flag && showingTileObjectOnTop[i] != null)
			{
				Object.Destroy(showingTileObjectOnTop[i].gameObject);
				showingTileObjectOnTop[i] = null;
			}
		}
	}

	public void getPrefabForPos(ItemOnTop itemOnTopToSpawn)
	{
		if (showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()] != null && showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].tileObjectId != itemOnTopToSpawn.getTileObjectId())
		{
			MonoBehaviour.print(showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].tileObjectId + " " + itemOnTopToSpawn.getTileObjectId());
			Object.Destroy(showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].gameObject);
			showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()] = null;
		}
		if (showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()] == null)
		{
			showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()] = WorldManager.manageWorld.getTileObjectForOnTop(itemOnTopToSpawn.getTileObjectId(), onTopPositions[itemOnTopToSpawn.sittingOnTopPosition()].transform.position);
			showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].placeDown();
		}
		showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].transform.position = onTopPositions[itemOnTopToSpawn.sittingOnTopPosition()].transform.position;
		showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].setRotatiomNumber(itemOnTopToSpawn.getRotation());
		if ((bool)showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].showObjectOnStatusChange)
		{
			showingTileObjectOnTop[itemOnTopToSpawn.sittingOnTopPosition()].showObjectOnStatusChange.showGameObject(itemOnTopToSpawn.getStatus());
		}
	}

	public void clearObjectsOnTopOfMe()
	{
		for (int i = 0; i < showingTileObjectOnTop.Length; i++)
		{
			if (showingTileObjectOnTop[i] != null)
			{
				Object.Destroy(showingTileObjectOnTop[i].gameObject);
				showingTileObjectOnTop[i] = null;
			}
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < showingTileObjectOnTop.Length; i++)
		{
			if (showingTileObjectOnTop[i] != null)
			{
				showingTileObjectOnTop[i].gameObject.SetActive(false);
			}
		}
	}

	private void OnEnable()
	{
		for (int i = 0; i < showingTileObjectOnTop.Length; i++)
		{
			if (showingTileObjectOnTop[i] != null)
			{
				showingTileObjectOnTop[i].gameObject.SetActive(true);
			}
		}
	}
}
