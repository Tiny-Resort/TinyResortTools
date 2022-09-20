public class ChunkUpdateDelay
{
	private int chunkX;

	private int chunkY;

	private bool needsOnTile = true;

	private bool needsTileType = true;

	private bool needsHeight = true;

	private bool needsWater = true;

	public ChunkUpdateDelay(int newChunkX, int newChunkY)
	{
		chunkX = newChunkX;
		chunkY = newChunkY;
	}

	public void serverSetUp(bool waitForOnTile, bool waitForType, bool waitForHeight, bool waitForWater)
	{
		if (!waitForOnTile)
		{
			needsOnTile = false;
		}
		if (!waitForType)
		{
			needsTileType = false;
		}
		if (!waitForHeight)
		{
			needsHeight = false;
		}
		if (!waitForWater)
		{
			needsWater = false;
		}
		checkAllAndRefreshTileIfNeeded();
	}

	private void checkAllAndRefreshTileIfNeeded()
	{
		if (!needsOnTile && !needsTileType && !needsHeight && !needsWater)
		{
			WorldManager.manageWorld.refreshAllChunksInUse(chunkX, chunkY, true);
		}
	}

	public void heightGiven()
	{
		needsHeight = false;
		checkAllAndRefreshTileIfNeeded();
	}

	public void ontileGiven()
	{
		needsOnTile = false;
		checkAllAndRefreshTileIfNeeded();
	}

	public void typeGiven()
	{
		needsTileType = false;
		checkAllAndRefreshTileIfNeeded();
	}

	public void waterGiven()
	{
		needsWater = false;
		checkAllAndRefreshTileIfNeeded();
	}

	public bool checkIfIsChunk(int checkX, int checkY)
	{
		if (checkX == chunkX)
		{
			return checkY == chunkY;
		}
		return false;
	}
}
