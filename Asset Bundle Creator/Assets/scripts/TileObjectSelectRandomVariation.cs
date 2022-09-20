using UnityEngine;

public class TileObjectSelectRandomVariation : MonoBehaviour
{
	public GameObject[] objects;

	private void OnEnable()
	{
		Random.InitState((int)base.transform.position.x * (int)base.transform.position.z + (int)base.transform.position.y + (int)base.transform.position.z - (int)base.transform.position.x);
		int num = Random.Range(0, objects.Length);
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(false);
		}
		objects[num].SetActive(true);
	}
}
