using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
	public int chunkPosX;

	public int chunkPosY;

	public Chunk myChunk;

	public Chunk myLastChunk;

	public BoxCollider myCol;

	private void Start()
	{
		chunkPosX = (int)base.transform.position.x / 2;
		chunkPosY = (int)base.transform.position.z / 2;
	}

	private void OnTriggerEnter(Collider other)
	{
		bool flag = (bool)myChunk;
	}

	private void OnTriggerExit(Collider other)
	{
		if ((bool)myChunk)
		{
			WorldManager.manageWorld.giveBackChunk(myChunk);
			myChunk = null;
		}
	}
}
