using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CursorImageChange : MonoBehaviour
{
	public Image cursorImage;

	public Sprite normalCursor;

	public Sprite dragCursor;

	public Sprite startDragCursor;

	public Vector3 dragPos;

	private void OnEnable()
	{
		StartCoroutine(checkCursor());
	}

	private IEnumerator checkCursor()
	{
		cursorImage.transform.localPosition = Vector3.zero;
		cursorImage.sprite = normalCursor;
		while (!Inventory.inv)
		{
			yield return null;
		}
		while (true)
		{
			if (Inventory.inv.dragSlot.itemNo != -1)
			{
				int inDragSlot = Inventory.inv.dragSlot.itemNo;
				yield return StartCoroutine(moveCursorToPos(dragPos, startDragCursor, dragCursor));
				while (Inventory.inv.dragSlot.itemNo == inDragSlot)
				{
					yield return null;
				}
				if (Inventory.inv.dragSlot.itemNo != -1)
				{
					yield return StartCoroutine(moveCursorToPos(dragPos / 2f, startDragCursor, dragCursor));
				}
				else
				{
					yield return StartCoroutine(moveCursorToPos(Vector3.zero, startDragCursor, normalCursor));
				}
			}
			yield return null;
		}
	}

	private IEnumerator moveCursorToPos(Vector3 desiredPos, Sprite startingSprite, Sprite endingSprite)
	{
		float timer = 0f;
		cursorImage.sprite = startingSprite;
		while (timer < 1f)
		{
			cursorImage.transform.localPosition = Vector3.Lerp(cursorImage.transform.localPosition, desiredPos, timer);
			timer += Time.deltaTime * 10f;
			yield return null;
		}
		cursorImage.sprite = endingSprite;
	}
}
