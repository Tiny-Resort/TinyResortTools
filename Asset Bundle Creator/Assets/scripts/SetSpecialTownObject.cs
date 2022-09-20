using System.Collections;
using UnityEngine;

public class SetSpecialTownObject : MonoBehaviour
{
	public ChestPlaceable recyclingBox;

	public bool privateTeleTower;

	private void OnEnable()
	{
		if ((bool)recyclingBox)
		{
			StartCoroutine(positionDelayRecylcing());
		}
		if (NetworkMapSharer.share.isServer && privateTeleTower)
		{
			StartCoroutine(positionDelayPrivateTeleTower());
		}
	}

	public IEnumerator positionDelayRecylcing()
	{
		while (recyclingBox.myXPos() == 0 && recyclingBox.myYPos() == 0)
		{
			yield return null;
		}
		TownManager.manage.recyclingBoxPos = new int[2]
		{
			recyclingBox.myXPos(),
			recyclingBox.myYPos()
		};
	}

	public IEnumerator positionDelayPrivateTeleTower()
	{
		TileObject myObject = GetComponent<TileObject>();
		while (myObject.xPos == 0 && myObject.yPos == 0)
		{
			yield return null;
		}
		NetworkMapSharer.share.NetworkprivateTowerPos = new Vector2(myObject.xPos, myObject.yPos);
	}
}
