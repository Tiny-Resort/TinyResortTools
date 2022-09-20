using System.Collections;
using UnityEngine;

public class NavChunk : MonoBehaviour
{
	private NavTile[,] navTiles = new NavTile[10, 10];

	public int showingX;

	public int showingY;

	public bool active = true;

	private void Awake()
	{
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				navTiles[j, i] = Object.Instantiate(NetworkNavMesh.nav.NavTilePrefab, base.transform.position + new Vector3(j * 2, 0f, i * 2), Quaternion.identity, base.transform).GetComponent<NavTile>();
				navTiles[j, i].myChunk = this;
			}
		}
	}

	public void collect()
	{
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				navTiles[j, i].collect();
			}
		}
	}

	public void placeInPos(int xPos, int yPos)
	{
		if (showingX != xPos || showingY != yPos)
		{
			showingX = xPos;
			showingY = yPos;
			base.transform.localPosition = new Vector3(showingX * 2, 0f, showingY * 2);
		}
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				navTiles[j, i].setTile(showingX + j, showingY + i);
			}
		}
	}

	public void placeInPosForceRefresh(int xPos, int yPos)
	{
		showingX = xPos;
		showingY = yPos;
		base.transform.localPosition = new Vector3(showingX * 2, 0f, showingY * 2);
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				navTiles[j, i].setTile(showingX + j, showingY + i, true);
			}
		}
	}

	public void placeInPosWithDelay(int xPos, int yPos)
	{
		StartCoroutine(placeInPosDelay(xPos, yPos));
	}

	private IEnumerator placeInPosDelay(int xPos, int yPos)
	{
		yield return null;
		placeInPos(xPos, yPos);
	}
}
