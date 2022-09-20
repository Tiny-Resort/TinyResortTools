using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InvSlotAnimator : MonoBehaviour
{
	public Image itemIcon;

	public Transform itemAmountText;

	private Vector3 originalTransformScale = new Vector3(1f, 1f, 1f);

	private Vector3 originalIconScale = new Vector3(1f, 1f, 1f);

	private bool currentlySelected;

	public void Start()
	{
	}

	private void OnDisable()
	{
		base.transform.localScale = originalTransformScale;
		itemIcon.transform.localScale = originalIconScale;
	}

	public void UpdateSlotContents()
	{
		if (base.isActiveAndEnabled)
		{
			StopCoroutine("animateUpdate");
			StartCoroutine("animateUpdate");
		}
	}

	public void SelectSlot()
	{
		if (base.isActiveAndEnabled && !currentlySelected)
		{
			currentlySelected = true;
			StopCoroutine("animateDeselect");
			StopCoroutine("animateSelect");
			StartCoroutine("animateSelect");
		}
	}

	public void DeSelectSlot()
	{
		if (base.isActiveAndEnabled && currentlySelected)
		{
			currentlySelected = false;
			StopCoroutine("animateSelect");
			StopCoroutine("animateDeselect");
			StartCoroutine("animateDeselect");
		}
	}

	private IEnumerator animateUpdate()
	{
		float journey = 0f;
		float duration = 0.35f;
		float startingScale = 0f;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.invSlotUpdateCurve.Evaluate(time);
			float num = Mathf.LerpUnclamped(startingScale, 1f, t);
			itemIcon.transform.localScale = new Vector3(num, num, num);
			if ((bool)itemAmountText)
			{
				itemAmountText.localScale = new Vector3(itemAmountText.localScale.x, num, itemAmountText.localScale.z);
			}
			yield return null;
		}
	}

	private IEnumerator animateSelect()
	{
		float journey = 0f;
		float duration = 0.5f;
		float startingScale = base.transform.localScale.y;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.invSlotSelectedCurve.Evaluate(time);
			float num = Mathf.LerpUnclamped(startingScale, 1.2f, t);
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
	}

	private IEnumerator animateDeselect()
	{
		float journey = 0f;
		float duration = 0.5f;
		float startingScale = base.transform.localScale.y;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.invSlotDeselectCurve.Evaluate(time);
			float num = Mathf.LerpUnclamped(startingScale, 1f, t);
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
	}
}
