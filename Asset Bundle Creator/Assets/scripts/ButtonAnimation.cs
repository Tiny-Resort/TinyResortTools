using System.Collections;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
	private bool hovering;

	private bool playingPressAnimation;

	private void Start()
	{
	}

	private void OnDisable()
	{
		base.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public void hoverOver()
	{
		if (!hovering && !playingPressAnimation)
		{
			hovering = true;
			clearRoutines();
			if (base.isActiveAndEnabled)
			{
				StartCoroutine("HoverAnimation");
			}
		}
	}

	private void clearRoutines()
	{
		if (base.isActiveAndEnabled)
		{
			StopCoroutine("PressAnimation");
			StopCoroutine("RolloutAnimation");
			StopCoroutine("HoverAnimation");
		}
	}

	public void rollOut()
	{
		if (hovering && !playingPressAnimation)
		{
			clearRoutines();
			if (base.isActiveAndEnabled)
			{
				StartCoroutine("RolloutAnimation");
			}
			hovering = false;
		}
	}

	public void press()
	{
		clearRoutines();
		if (base.isActiveAndEnabled)
		{
			playingPressAnimation = true;
			StartCoroutine("PressAnimation");
		}
	}

	private IEnumerator HoverAnimation()
	{
		float journey = 0f;
		float duration = 0.35f;
		float y = base.transform.localScale.y;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.buttonHoverCurve.Evaluate(time);
			float num = Mathf.LerpUnclamped(1f, 1.1f, t);
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
	}

	private IEnumerator PressAnimation()
	{
		float journey = 0f;
		float duration = 0.25f;
		float y = base.transform.localScale.y;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.buttonPressCurve.Evaluate(time);
			float num = Mathf.LerpUnclamped(0.85f, 1f, t);
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
		playingPressAnimation = false;
	}

	private IEnumerator RolloutAnimation()
	{
		float journey = 0f;
		float duration = 0.35f;
		float y = base.transform.localScale.y;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.buttonRollOutCurve.Evaluate(time);
			float num = Mathf.LerpUnclamped(1.1f, 1f, t);
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
	}
}
