using UnityEngine;

public class TractorLight : MonoBehaviour
{
	public MeshFilter myFilt;

	public Mesh onMesh;

	public Mesh offMesh;

	public Tractor myTractor;

	public int myId;

	public void updateLight(int newId)
	{
		if (newId == myId)
		{
			myFilt.sharedMesh = onMesh;
		}
		else
		{
			myFilt.sharedMesh = offMesh;
		}
	}
}
