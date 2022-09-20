using System.Collections;
using UnityEngine;

public class SnapSelectionForWindow : MonoBehaviour
{
	public RectTransform[] snapToSelectionOrder;

	public bool lockCursorSnap = true;

	private RectTransform selectedBefore;

	public bool moveBackToPreviouslySelected = true;

	public RectTransform moveToFirst;

	public Transform chooseFirstActiveChildFromTransform;

	private void OnEnable()
	{
		if (moveBackToPreviouslySelected)
		{
			selectedBefore = Inventory.inv.currentlySelected;
		}
		if (lockCursorSnap)
		{
			Inventory.inv.lockCursorSnap = true;
		}
		reselectSnap();
	}

	private void OnDisable()
	{
		if (lockCursorSnap)
		{
			Inventory.inv.lockCursorSnap = false;
		}
		if ((bool)selectedBefore)
		{
			Inventory.inv.setCurrentlySelectedAndMoveCursor(selectedBefore);
		}
	}

	private IEnumerator moveToTransforms()
	{
		while (true)
		{
			for (int i = 0; i < snapToSelectionOrder.Length; i++)
			{
				if (snapToSelectionOrder[i].gameObject.activeSelf)
				{
					if (Inventory.inv.currentlySelected != snapToSelectionOrder[i])
					{
						Inventory.inv.setCurrentlySelectedAndMoveCursor(snapToSelectionOrder[i]);
					}
					break;
				}
			}
			yield return null;
		}
	}

	public void reselectDelay()
	{
		StartCoroutine(selectDelay());
	}

	private IEnumerator selectDelay()
	{
		yield return null;
		reselectSnap();
	}

	public void reselectSnap()
	{
		if ((bool)chooseFirstActiveChildFromTransform)
		{
			for (int i = 0; i < chooseFirstActiveChildFromTransform.childCount; i++)
			{
				if (chooseFirstActiveChildFromTransform.GetChild(i).gameObject.activeInHierarchy)
				{
					Inventory.inv.setCurrentlySelectedAndMoveCursor(chooseFirstActiveChildFromTransform.GetChild(i).GetComponent<RectTransform>());
					break;
				}
			}
		}
		if ((bool)moveToFirst)
		{
			Inventory.inv.setCurrentlySelectedAndMoveCursor(moveToFirst);
		}
		if (snapToSelectionOrder.Length != 0)
		{
			StartCoroutine(moveToTransforms());
		}
	}
}
