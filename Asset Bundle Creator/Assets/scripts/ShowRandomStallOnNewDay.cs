using UnityEngine;

public class ShowRandomStallOnNewDay : MonoBehaviour
{
	public GameObject[] toShowFromList;

	private void OnEnable()
	{
		if (!NetworkMapSharer.share)
		{
			return;
		}
		Random.InitState(NetworkMapSharer.share.mineSeed);
		int num = Random.Range(0, toShowFromList.Length);
		for (int i = 0; i < toShowFromList.Length; i++)
		{
			if (i == num)
			{
				toShowFromList[i].SetActive(true);
			}
			else
			{
				toShowFromList[i].SetActive(false);
			}
		}
	}
}
