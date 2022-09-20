using UnityEngine;

public class ButtonTabs : MonoBehaviour
{
	public InvButton[] tabButtons;

	private int currentlySelected;

	public GameObject[] dummyButtons;

	public bool selectFirstButtonOnEnable;

	public bool leftAndRightOnly;

	private void OnEnable()
	{
		if (selectFirstButtonOnEnable)
		{
			selectPressedButton(0);
		}
	}

	public void Update()
	{
		if (InputMaster.input.RB())
		{
			if (leftAndRightOnly)
			{
				if (tabButtons[1].gameObject.activeInHierarchy)
				{
					tabButtons[1].PressButton();
				}
				else
				{
					SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
				}
				return;
			}
			currentlySelected++;
			makeSelection();
		}
		if (!InputMaster.input.LB())
		{
			return;
		}
		if (leftAndRightOnly)
		{
			if (tabButtons[0].gameObject.activeInHierarchy)
			{
				tabButtons[0].PressButton();
			}
			else
			{
				SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
			}
		}
		else
		{
			currentlySelected--;
			makeSelection();
		}
	}

	public void selectPressedButton(int newSelected)
	{
		currentlySelected = newSelected;
		if (dummyButtons.Length == tabButtons.Length)
		{
			for (int i = 0; i < tabButtons.Length; i++)
			{
				dummyButtons[i].gameObject.SetActive(i == currentlySelected);
			}
		}
	}

	public void makeSelection()
	{
		if (currentlySelected < 0)
		{
			currentlySelected = tabButtons.Length - 1;
		}
		if (currentlySelected >= tabButtons.Length)
		{
			currentlySelected = 0;
		}
		tabButtons[currentlySelected].PressButton();
	}
}
