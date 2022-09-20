using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	public GameObject tilePrefab;

	public Tile[,] chunksTiles;

	public int showingChunkX;

	public int showingChunkY;

	public int waterTilesOnChunk;

	public int oceanTilesOnChunk;

	public Transform _transform;

	public MeshFilter finalFilter;

	public MeshRenderer finalRen;

	public MeshCollider finalCollider;

	public MeshFilter waterFilt;

	public MeshCollider waterCollider;

	public MeshCollider swimmingCollider;

	public static Mesh[,] waterMeshes = new Mesh[10, 10];

	private Coroutine combiningRoutine;

	private void Start()
	{
		finalFilter.sharedMesh = new Mesh();
	}

	private void Awake()
	{
		setUpTiles();
	}

	public void OnEnable()
	{
		combiningRoutine = null;
	}

	public void setUpTiles()
	{
		chunksTiles = new Tile[WorldManager.manageWorld.chunkSize, WorldManager.manageWorld.chunkSize];
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				chunksTiles[j, i] = Object.Instantiate(tilePrefab, base.transform.position + new Vector3(j * WorldManager.manageWorld.getTileSize(), 0f, i * WorldManager.manageWorld.getTileSize()), Quaternion.identity, base.transform).GetComponent<Tile>();
			}
		}
	}

	public void refreshChunk(bool fullRefresh = true)
	{
		if (fullRefresh)
		{
			for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
			{
				for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
				{
					chunksTiles[j, i].refreshTile(showingChunkX + j, showingChunkY + i);
				}
			}
			if (combiningRoutine != null)
			{
				StopCoroutine(combiningRoutine);
				combiningRoutine = null;
			}
			combiningRoutine = StartCoroutine(combineChildren());
			return;
		}
		for (int k = 0; k < WorldManager.manageWorld.chunkSize; k++)
		{
			for (int l = 0; l < WorldManager.manageWorld.chunkSize; l++)
			{
				if (l == 0 || k == 0 || l == WorldManager.manageWorld.chunkSize - 1 || k == WorldManager.manageWorld.chunkSize - 1)
				{
					chunksTiles[l, k].refreshForNeighbours();
				}
			}
		}
		if (combiningRoutine != null)
		{
			StopCoroutine(combiningRoutine);
			combiningRoutine = null;
		}
		combiningRoutine = StartCoroutine(combineChildren());
	}

	public void returnAllTileObjects()
	{
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				chunksTiles[j, i].returnTileObjects();
			}
		}
	}

	public void refreshChunksOnTileObjects(bool neighbourCheck = false)
	{
		if (!neighbourCheck)
		{
			for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
			{
				for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
				{
					chunksTiles[j, i].refreshTileObjects();
				}
			}
		}
		else
		{
			for (int k = 0; k < WorldManager.manageWorld.chunkSize; k++)
			{
				for (int l = 0; l < WorldManager.manageWorld.chunkSize; l++)
				{
					if (l == 0 || k == 0 || l == WorldManager.manageWorld.chunkSize - 1 || k == WorldManager.manageWorld.chunkSize - 1)
					{
						chunksTiles[l, k].refreshTileObjects();
					}
				}
			}
		}
		countWaterTiles();
	}

	public void refreshChunkToEmptySeaChunk()
	{
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				chunksTiles[j, i].setTileToOceanFloor();
			}
		}
		oceanTilesOnChunk = 100;
		waterTilesOnChunk = 0;
		List<CombineInstance> list = new List<CombineInstance>();
		List<CombineInstance> list2 = new List<CombineInstance>();
		for (int k = 0; k < WorldManager.manageWorld.chunkSize; k++)
		{
			for (int l = 0; l < WorldManager.manageWorld.chunkSize; l++)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = chunksTiles[l, k].filt.sharedMesh;
				item.transform = chunksTiles[l, k].filt.transform.localToWorldMatrix;
				list2.Add(item);
				CombineInstance item2 = default(CombineInstance);
				item2.mesh = chunksTiles[l, k].waterFilter.sharedMesh;
				item2.transform = chunksTiles[l, k].waterFilter.transform.localToWorldMatrix;
				list.Add(item2);
			}
		}
		Object.Destroy(waterFilt.sharedMesh);
		waterFilt.sharedMesh = new Mesh();
		waterFilt.sharedMesh.CombineMeshes(list.ToArray());
		waterCollider.sharedMesh = waterFilt.sharedMesh;
		swimmingCollider.sharedMesh = waterFilt.sharedMesh;
		Object.Destroy(finalFilter.sharedMesh);
		finalFilter.sharedMesh = new Mesh();
		finalFilter.sharedMesh.CombineMeshes(list2.ToArray());
		finalRen.materials = new Material[1] { WorldManager.manageWorld.tileTypes[3].myTileMaterial };
		base.transform.position = new Vector3(showingChunkX * 2, 0f, showingChunkY * 2);
	}

	public void refreshToNewChunk()
	{
		if ((bool)NetworkMapSharer.share && (bool)NetworkMapSharer.share.localChar && !NetworkMapSharer.share.isServer && !WorldManager.manageWorld.clientRequestedMap[showingChunkX / WorldManager.manageWorld.getChunkSize(), showingChunkY / WorldManager.manageWorld.getChunkSize()])
		{
			WorldManager.manageWorld.clientRequestedMap[showingChunkX / WorldManager.manageWorld.chunkSize, showingChunkY / WorldManager.manageWorld.getChunkSize()] = true;
			NetworkMapSharer.share.addChunkRequestedDelay(showingChunkX, showingChunkY);
			NetworkMapSharer.share.localChar.CmdRequestMapChunk(showingChunkX, showingChunkY);
		}
		refreshNewChunk();
	}

	public void setChunkAndRefresh(int newshowingX, int newshowingY, bool fullRefresh = false)
	{
		showingChunkX = newshowingX;
		showingChunkY = newshowingY;
		base.gameObject.SetActive(true);
		if (fullRefresh)
		{
			refreshToNewChunk();
		}
		else if (showingChunkX < 0 || showingChunkX >= WorldManager.manageWorld.getMapSize() || showingChunkY < 0 || showingChunkY >= WorldManager.manageWorld.getMapSize())
		{
			refreshChunkToEmptySeaChunk();
		}
		else
		{
			refreshToNewChunk();
		}
	}

	public void returnOnTileObjects()
	{
		if (showingChunkX < 0 || showingChunkX >= WorldManager.manageWorld.getMapSize() || showingChunkY < 0 || showingChunkY >= WorldManager.manageWorld.getMapSize())
		{
			refreshChunkToEmptySeaChunk();
			return;
		}
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				chunksTiles[j, i].returnTileObjects();
			}
		}
	}

	private void refreshNewChunk()
	{
		if (showingChunkX < 0 || showingChunkX >= WorldManager.manageWorld.getMapSize() || showingChunkY < 0 || showingChunkY >= WorldManager.manageWorld.getMapSize())
		{
			refreshChunkToEmptySeaChunk();
			return;
		}
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				chunksTiles[j, i].refreshTile(showingChunkX + j, showingChunkY + i);
			}
		}
		countWaterTiles();
		if (combiningRoutine != null)
		{
			StopCoroutine(combiningRoutine);
			combiningRoutine = null;
		}
		combiningRoutine = StartCoroutine(combineChildren());
	}

	public void countWaterTiles()
	{
		oceanTilesOnChunk = 0;
		waterTilesOnChunk = 0;
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				if (WorldManager.manageWorld.waterMap[showingChunkX + j, showingChunkY + i])
				{
					if (WorldManager.manageWorld.tileTypeMap[showingChunkX + j, showingChunkY + i] == 3)
					{
						oceanTilesOnChunk++;
					}
					else
					{
						waterTilesOnChunk++;
					}
				}
			}
		}
	}

	public IEnumerator combineChildren()
	{
		yield return null;
		finalRen.enabled = false;
		base.transform.position = Vector3.zero;
		TileIdInMat[] array = new TileIdInMat[WorldManager.manageWorld.tileTypes.Length];
		TileIdInMat tileIdInMat = new TileIdInMat();
		List<CombineInstance> list = new List<CombineInstance>();
		List<Material> list2 = new List<Material>();
		int num = 0;
		for (int i = 0; i < WorldManager.manageWorld.chunkSize; i++)
		{
			for (int j = 0; j < WorldManager.manageWorld.chunkSize; j++)
			{
				int num2 = WorldManager.manageWorld.tileTypeMap[showingChunkX + j, showingChunkY + i];
				CombineInstance item = default(CombineInstance);
				item.mesh = chunksTiles[j, i].filt.sharedMesh;
				item.transform = chunksTiles[j, i].filt.transform.localToWorldMatrix;
				if (array[num2] == null)
				{
					array[num2] = new TileIdInMat();
				}
				array[num2].idsWithMyMat.Add(item);
				if (chunksTiles[j, i].filt.sharedMesh.subMeshCount > 1)
				{
					CombineInstance item2 = default(CombineInstance);
					item2.mesh = chunksTiles[j, i].filt.sharedMesh;
					item2.transform = chunksTiles[j, i].filt.transform.localToWorldMatrix;
					item2.subMeshIndex = 1;
					if (WorldManager.manageWorld.tileTypes[num2].sideOfTileSame)
					{
						array[num2].idsWithMyMat.Add(item2);
					}
					else
					{
						tileIdInMat.idsWithMyMat.Add(item2);
					}
				}
				if (WorldManager.manageWorld.waterMap[showingChunkX + j, showingChunkY + i])
				{
					CombineInstance item3 = default(CombineInstance);
					item3.mesh = chunksTiles[j, i].waterFilter.mesh;
					item3.transform = chunksTiles[j, i].waterFilter.transform.localToWorldMatrix;
					list.Add(item3);
				}
				num++;
			}
		}
		List<CombineInstance> list3 = new List<CombineInstance>();
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] != null && array[k].idsWithMyMat.Count > 0)
			{
				finalFilter.sharedMesh = new Mesh();
				finalFilter.sharedMesh.CombineMeshes(array[k].idsWithMyMat.ToArray());
				CombineInstance item4 = default(CombineInstance);
				item4.mesh = finalFilter.sharedMesh;
				item4.transform = finalFilter.transform.localToWorldMatrix;
				list3.Add(item4);
				list2.Add(WorldManager.manageWorld.tileTypes[k].myTileMaterial);
			}
		}
		if (tileIdInMat.idsWithMyMat.Count > 0)
		{
			finalFilter.sharedMesh = new Mesh();
			finalFilter.sharedMesh.CombineMeshes(tileIdInMat.idsWithMyMat.ToArray());
			CombineInstance item5 = default(CombineInstance);
			item5.mesh = finalFilter.sharedMesh;
			item5.transform = finalFilter.transform.localToWorldMatrix;
			list3.Add(item5);
			list2.Add(WorldManager.manageWorld.stoneSide);
		}
		Object.Destroy(waterFilt.sharedMesh);
		if (list.Count > 0)
		{
			waterFilt.sharedMesh = new Mesh();
			waterFilt.sharedMesh.CombineMeshes(list.ToArray());
			waterCollider.sharedMesh = waterFilt.sharedMesh;
			swimmingCollider.sharedMesh = waterFilt.sharedMesh;
		}
		Object.Destroy(finalFilter.sharedMesh);
		finalFilter.sharedMesh = new Mesh();
		finalFilter.sharedMesh.CombineMeshes(list3.ToArray(), false);
		for (int l = 0; l < list3.Count; l++)
		{
			Object.Destroy(list3[l].mesh);
		}
		Object.Destroy(finalCollider.sharedMesh);
		finalCollider.sharedMesh = finalFilter.sharedMesh;
		finalRen.materials = list2.ToArray();
		finalFilter.transform.localPosition = new Vector3(showingChunkX * 2, 0f, showingChunkY * 2);
		finalRen.enabled = true;
		combiningRoutine = null;
	}

	public void WeldVertices(Mesh aMesh, float aMaxDelta = 0.001f)
	{
		Vector3[] vertices = aMesh.vertices;
		Vector3[] normals = aMesh.normals;
		Vector2[] uv = aMesh.uv;
		List<int> list = new List<int>();
		int[] array = new int[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = vertices[i];
			Vector3 to = normals[i];
			Vector2 vector2 = uv[i];
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				int num = list[j];
				if ((vertices[num] - vector).sqrMagnitude <= aMaxDelta && Vector3.Angle(normals[num], to) <= aMaxDelta && (uv[num] - vector2).sqrMagnitude <= aMaxDelta)
				{
					array[i] = j;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				array[i] = list.Count;
				list.Add(i);
			}
		}
		Vector3[] array2 = new Vector3[list.Count];
		Vector3[] array3 = new Vector3[list.Count];
		Vector2[] array4 = new Vector2[list.Count];
		for (int k = 0; k < list.Count; k++)
		{
			int num2 = list[k];
			array2[k] = vertices[num2];
			array3[k] = normals[num2];
			array4[k] = uv[num2];
		}
		int[] triangles = aMesh.triangles;
		for (int l = 0; l < triangles.Length; l++)
		{
			triangles[l] = array[triangles[l]];
		}
		aMesh.triangles = triangles;
		aMesh.vertices = array2;
		aMesh.normals = array3;
		aMesh.uv = array4;
	}

	public Mesh moveMeshUV(float x, float y, Mesh waterMesh)
	{
		Vector2[] array = new Vector2[waterMesh.vertices.Length];
		array = waterMesh.uv;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vector2(array[i].x + x, array[i].y + y);
		}
		waterMesh.uv = array;
		return waterMesh;
	}
}
