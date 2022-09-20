using UnityEngine;

public class OnOffTile : MonoBehaviour
{
	public Animator onOffAnim;

	public bool isGate;

	public bool isOpen;

	private bool firstRefresh = true;

	public ASound openSound;

	public ASound closeSound;

	public InventoryItem requiredToOpen;

	public DailyTaskGenerator.genericTaskType taskWhenUnlocked;

	public void setOnOff(int xPos, int yPos, bool changedByPlayer = false)
	{
		if (firstRefresh)
		{
			if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 0)
			{
				onOffAnim.SetTrigger("StartClosed");
				isOpen = false;
			}
			else
			{
				onOffAnim.SetTrigger("StartOpen");
				isOpen = true;
			}
			firstRefresh = false;
		}
		if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 0)
		{
			if (isOpen)
			{
				isOpen = false;
				onOffAnim.SetTrigger("Close");
				if (changedByPlayer && (bool)openSound)
				{
					SoundManager.manage.playASoundAtPoint(openSound, base.transform.position);
				}
			}
		}
		else if (!isOpen)
		{
			isOpen = true;
			onOffAnim.SetTrigger("Open");
			if (changedByPlayer && (bool)closeSound)
			{
				SoundManager.manage.playASoundAtPoint(closeSound, base.transform.position);
			}
		}
		firstRefresh = false;
	}

	public void fakeOpen()
	{
		onOffAnim.SetTrigger("StartOpen");
	}

	public void fakeClose()
	{
		onOffAnim.SetTrigger("StartClosed");
	}

	public bool getIfOpen(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 0)
		{
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		firstRefresh = true;
	}
}
