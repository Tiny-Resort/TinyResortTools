using System.Collections;
using UnityEngine;

public class ShowObjectOnStatusChange : MonoBehaviour
{
	[Header("Default (show item from tile status)--------")]
	public GameObject[] objectsToShow;

	[Header("Chest stuff--------")]
	public ChestPlaceable isChest;

	[Header("Sign stuff--------")]
	public ItemSign isSign;

	[Header("Clothing Stuff--------")]
	public ClothingDisplay isClothing;

	[Header("Scale stuff--------")]
	public Transform toScale;

	public int fullSizeAtNumber = 1;

	public ASound playSoundOnSizeChange;

	[Header("Other Stuff-------")]
	public bool animatePopUpOnChange;

	private int lastShowing = -2;

	public bool canBePickedUpByHand;

	private int insideLastShowing = -2;

	public void showGameObject(int status)
	{
		if ((bool)isClothing)
		{
			isClothing.updateStatus(status);
		}
		if ((bool)isSign)
		{
			isSign.updateStatus(status);
		}
	}

	public void showGameObject(int xPos, int yPos, HouseDetails inside = null)
	{
		if ((bool)toScale)
		{
			float y = (float)WorldManager.manageWorld.onTileStatusMap[xPos, yPos] / ((float)fullSizeAtNumber * 1f);
			toScale.localScale = new Vector3(1f, y, 1f);
			SoundManager.manage.playASoundAtPoint(playSoundOnSizeChange, base.transform.position);
			return;
		}
		if ((bool)isClothing)
		{
			if (inside == null)
			{
				isClothing.updateStatus(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]);
			}
			else
			{
				isClothing.updateStatus(inside.houseMapOnTileStatus[xPos, yPos]);
			}
		}
		if ((bool)isSign)
		{
			if (inside == null)
			{
				isSign.updateStatus(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]);
			}
			else
			{
				isSign.updateStatus(inside.houseMapOnTileStatus[xPos, yPos]);
			}
			return;
		}
		if ((bool)isChest)
		{
			if (inside != null)
			{
				if (insideLastShowing != -2 && insideLastShowing != inside.houseMapOnTileStatus[xPos, yPos])
				{
					if (inside.houseMapOnTileStatus[xPos, yPos] == 0)
					{
						SoundManager.manage.playASoundAtPoint(isChest.closeSound, base.transform.position);
					}
					else if (inside.houseMapOnTileStatus[xPos, yPos] == 1)
					{
						SoundManager.manage.playASoundAtPoint(isChest.openSound, base.transform.position);
					}
				}
				insideLastShowing = inside.houseMapOnTileStatus[xPos, yPos];
				if (insideLastShowing == 0)
				{
					isChest.chestAnim.SetBool("Open", false);
				}
				else
				{
					isChest.chestAnim.SetBool("Open", true);
				}
			}
			else
			{
				if (lastShowing != -2 && lastShowing != WorldManager.manageWorld.onTileStatusMap[xPos, yPos])
				{
					if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 0)
					{
						SoundManager.manage.playASoundAtPoint(isChest.closeSound, base.transform.position);
					}
					else if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 1)
					{
						SoundManager.manage.playASoundAtPoint(isChest.openSound, base.transform.position);
					}
				}
				lastShowing = WorldManager.manageWorld.onTileStatusMap[xPos, yPos];
				if (lastShowing == 0)
				{
					isChest.chestAnim.SetBool("Open", false);
				}
				else
				{
					isChest.chestAnim.SetBool("Open", true);
				}
			}
		}
		if ((bool)isChest || inside != null)
		{
			return;
		}
		for (int i = 0; i < objectsToShow.Length; i++)
		{
			if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == i || (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == -1 && i == 0))
			{
				if (animatePopUpOnChange && lastShowing != WorldManager.manageWorld.onTileStatusMap[xPos, yPos])
				{
					StartCoroutine(AnimateAppear(objectsToShow[i]));
				}
				objectsToShow[i].SetActive(true);
			}
			else
			{
				objectsToShow[i].SetActive(false);
			}
		}
		lastShowing = WorldManager.manageWorld.onTileStatusMap[xPos, yPos];
	}

	private IEnumerator AnimateAppear(GameObject toPopUp)
	{
		float journey = 0f;
		while (journey <= 0.35f)
		{
			journey += Time.deltaTime;
			float t = UIAnimationManager.manage.windowsOpenCurve.Evaluate(Mathf.Clamp01(journey / 0.35f));
			toPopUp.transform.localScale = new Vector3(Mathf.LerpUnclamped(0.25f, 1f, t), Mathf.LerpUnclamped(0.1f, 1f, t), Mathf.LerpUnclamped(0.25f, 1f, t));
			yield return null;
		}
	}
}
