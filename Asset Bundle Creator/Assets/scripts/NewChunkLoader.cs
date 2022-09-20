using System.Collections;
using UnityEngine;

public class NewChunkLoader : MonoBehaviour
{
	public static NewChunkLoader loader;

	private Transform viewingTransform;

	public bool inside;

	private int centreChunkX;

	private int centreChunkY;

	private int chunkViewDistance = 4;

	private int distanceToView;

	private int chunkSize;

	public int oceanTilesNearChar;

	public int waterTilesNearChar;

	public int riverTilesInCharChunk;

	private WaitForSeconds twoSecs = new WaitForSeconds(0.1f);

	private void Awake()
	{
		loader = this;
	}

	private void Start()
	{
		chunkSize = WorldManager.manageWorld.getChunkSize();
		viewingTransform = base.transform;
		StartCoroutine(checkChunks());
	}

	public void setChunkDistance(int newDistance = 3)
	{
		chunkViewDistance = newDistance;
		CameraController.control.updateDepthOfFieldAndFog(newDistance);
		if ((bool)viewingTransform)
		{
			forceInstantUpdateAtPos();
		}
	}

	public int getChunkDistance()
	{
		return chunkViewDistance;
	}

	public void resetChunksViewing()
	{
		centreChunkX = 0;
		centreChunkY = 0;
	}

	private IEnumerator checkChunks()
	{
		yield return new WaitForSeconds(1f);
		while (true)
		{
			if (!inside)
			{
				int num = (int)(Mathf.Round(viewingTransform.position.x) / 2f) / chunkSize * chunkSize;
				int num2 = (int)(Mathf.Round(viewingTransform.position.z) / 2f) / chunkSize * chunkSize;
				if (num != centreChunkX || num2 != centreChunkY)
				{
					centreChunkX = num;
					centreChunkY = num2;
					WorldManager.manageWorld.returnChunksNotCloseEnough(centreChunkX, centreChunkY, chunkViewDistance);
					for (int y = -chunkViewDistance; y < chunkViewDistance; y++)
					{
						for (int x = -chunkViewDistance; x < chunkViewDistance; x++)
						{
							int num3 = centreChunkX + chunkSize * x;
							int num4 = centreChunkY + chunkSize * y;
							if (WorldManager.manageWorld.doesPositionNeedsChunk(num3, num4))
							{
								WorldManager.manageWorld.getFreeChunkAndSetInPos(num3, num4);
								yield return null;
							}
						}
						num = (int)(Mathf.Round(viewingTransform.position.x) / 2f) / chunkSize * chunkSize;
						num2 = (int)(Mathf.Round(viewingTransform.position.z) / 2f) / chunkSize * chunkSize;
						if (num != centreChunkX || num2 != centreChunkY)
						{
							break;
						}
					}
					WorldManager.manageWorld.getNoOfWaterTilesClose((int)(Mathf.Round(viewingTransform.parent.position.x) / 2f) / chunkSize * chunkSize, (int)(Mathf.Round(viewingTransform.parent.position.z) / 2f) / chunkSize * chunkSize);
				}
			}
			yield return null;
		}
	}

	public void forceInstantUpdateAtPos()
	{
		int num = (int)(Mathf.Round(viewingTransform.position.x) / 2f) / chunkSize * chunkSize;
		int num2 = (int)(Mathf.Round(viewingTransform.position.z) / 2f) / chunkSize * chunkSize;
		if (num == centreChunkX && num2 == centreChunkY)
		{
			return;
		}
		centreChunkX = num;
		centreChunkY = num2;
		WorldManager.manageWorld.returnChunksNotCloseEnough(centreChunkX, centreChunkY, chunkViewDistance);
		for (int i = -chunkViewDistance; i < chunkViewDistance; i++)
		{
			for (int j = -chunkViewDistance; j < chunkViewDistance; j++)
			{
				int num3 = centreChunkX + chunkSize * j;
				int num4 = centreChunkY + chunkSize * i;
				if (WorldManager.manageWorld.doesPositionNeedsChunk(num3, num4))
				{
					WorldManager.manageWorld.getFreeChunkAndSetInPos(num3, num4);
				}
			}
		}
		WorldManager.manageWorld.getNoOfWaterTilesClose((int)(Mathf.Round(viewingTransform.parent.position.x) / 2f) / chunkSize * chunkSize, (int)(Mathf.Round(viewingTransform.parent.position.z) / 2f) / chunkSize * chunkSize);
	}

	public IEnumerator staggerRoutine()
	{
		if (!TownManager.manage.firstConnect)
		{
			int staggeredDistanceTarget = getChunkDistance();
			chunkViewDistance = 2;
			while (chunkViewDistance < staggeredDistanceTarget)
			{
				yield return new WaitForSeconds(2f);
				chunkViewDistance++;
				resetChunksViewing();
				CameraController.control.updateDepthOfFieldAndFog(chunkViewDistance);
			}
		}
	}

	public void staggerChunkDistanceOnConnect()
	{
		StartCoroutine(staggerRoutine());
	}
}
