using UnityEngine;
using UnityEngine.EventSystems;

public class OnSelectedMoveToCursor : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	private RectTransform thisButton;

	private void Start()
	{
		thisButton = GetComponent<RectTransform>();
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (Inventory.inv.snapCursorOn)
		{
			Inventory.inv.currentlySelected = thisButton;
			Inventory.inv.cursor.position = Inventory.inv.currentlySelected.transform.position + new Vector3(Inventory.inv.currentlySelected.sizeDelta.x / 2f - 2f, 0f, 0f);
		}
	}
}
