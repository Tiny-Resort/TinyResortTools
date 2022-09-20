using UnityEngine;

public class MeshManager : MonoBehaviour
{
	public static MeshManager manage;

	private MeshFilter myFilter;

	public Mesh[] allMeshes;

	public Mesh[,] allMeshVariations;

	public Mesh allSidesTile;

	public Mesh[,] allSidesVariations = new Mesh[8, 8];

	public Mesh noEdgeTile;

	[Header("Edges --------------")]
	public Mesh frontEdgeTile;

	public Mesh backEdgeTile;

	public Mesh leftEdgeTile;

	public Mesh rightEdgeTile;

	[Header("Edges with both back corners --------------")]
	public Mesh noEdgesAllCorners;

	public Mesh rightEdgeBothBack;

	public Mesh leftEdgeBothBack;

	public Mesh backEdgeBothBack;

	public Mesh frontEdgeBothBack;

	[Header("Edges with one corner --------------")]
	public Mesh rightEdgeFront;

	public Mesh rightEdgeBack;

	public Mesh leftEdgeFront;

	public Mesh leftEdgeBack;

	public Mesh frontEdgeLeftCorner;

	public Mesh frontEdgeRightCorner;

	public Mesh backEdgeLeftCorner;

	public Mesh backEdgeRightCorner;

	[Header("Corners--------------")]
	public Mesh backLeftTile;

	public Mesh backRightTile;

	public Mesh frontLeftTile;

	public Mesh frontRightTile;

	public Mesh frontCornerTile;

	public Mesh backCornerTile;

	public Mesh leftCornerTile;

	public Mesh rightCornerTile;

	[Header("Corners with edges--------------")]
	public Mesh backLeftTileEdge;

	public Mesh backRightTileEdge;

	public Mesh frontLeftTileEdge;

	public Mesh frontRightTileEdge;

	[Header("Flat with corners --------------")]
	public Mesh flatFrontLeftCorner;

	public Mesh flatFrontRightCorner;

	public Mesh flatBackLeftCorner;

	public Mesh flatBackRightCorner;

	[Header("Flat with both corners --------------")]
	public Mesh flatLeftBothCorners;

	public Mesh flatRightBothCorners;

	public Mesh flatBackBothCorners;

	public Mesh flatFrontBothCorners;

	[Header("Others --------------")]
	public Mesh noNeighboursUp;

	public Mesh noNeighboursSide;

	private void Awake()
	{
		manage = this;
		myFilter = GetComponent<MeshFilter>();
	}

	public Mesh getUniqueMeshForVariation(int tileType)
	{
		myFilter.mesh = allMeshes[tileType];
		return myFilter.mesh;
	}

	private void Start()
	{
		allMeshVariations = new Mesh[allMeshes.Length, 17];
	}
}
