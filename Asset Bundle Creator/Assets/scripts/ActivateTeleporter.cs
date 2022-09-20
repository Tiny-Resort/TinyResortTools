using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTeleporter : MonoBehaviour
{
	public GameObject signObject;

	public GameObject pad;

	private List<CharMovement> charsInCol = new List<CharMovement>();

	private bool activationTimerComplete;

	private void OnTriggerEnter(Collider other)
	{
		CharMovement componentInParent = other.GetComponentInParent<CharMovement>();
		if ((bool)componentInParent && !charsInCol.Contains(componentInParent))
		{
			if (componentInParent.isLocalPlayer)
			{
				RenderMap.map.canTele = true;
				if (activationTimerComplete)
				{
					if (moreThanOneTeleOn())
					{
						MenuButtonsTop.menu.switchToMap();
						StartCoroutine(centreCharWhileOnPad(componentInParent.transform));
					}
					else
					{
						Invoke("readSignDelay", 1f);
					}
				}
			}
			charsInCol.Add(componentInParent);
		}
		checkPad();
		checkSign();
	}

	private void OnTriggerExit(Collider other)
	{
		CharMovement componentInParent = other.GetComponentInParent<CharMovement>();
		if ((bool)componentInParent)
		{
			if (componentInParent.isLocalPlayer)
			{
				RenderMap.map.canTele = false;
			}
			charsInCol.Remove(componentInParent);
		}
		checkPad();
		checkSign();
	}

	private void OnEnable()
	{
		activationTimerComplete = false;
		charsInCol.Clear();
		pad.SetActive(false);
		signObject.SetActive(false);
		Invoke("activateTimer", 1f);
	}

	public void activateTimer()
	{
		activationTimerComplete = true;
	}

	public void readSignDelay()
	{
		if (charsInCol.Contains(NetworkMapSharer.share.localChar))
		{
			signObject.GetComponent<ReadableSign>().readSign();
		}
	}

	public void checkPad()
	{
		if (charsInCol.Count > 0)
		{
			pad.SetActive(true);
		}
		else
		{
			pad.SetActive(false);
		}
	}

	public void checkSign()
	{
		if (!charsInCol.Contains(NetworkMapSharer.share.localChar))
		{
			signObject.SetActive(false);
		}
	}

	private IEnumerator centreCharWhileOnPad(Transform localChar)
	{
		while (RenderMap.map.mapOpen)
		{
			yield return null;
			localChar.position = Vector3.Lerp(localChar.position, base.transform.position, Time.deltaTime * 2f);
			localChar.rotation = Quaternion.Lerp(localChar.rotation, base.transform.rotation, Time.deltaTime * 2f);
		}
	}

	public bool moreThanOneTeleOn()
	{
		int num = 0;
		if (NetworkMapSharer.share.privateTowerPos != Vector2.zero)
		{
			num++;
		}
		if (NetworkMapSharer.share.northOn)
		{
			num++;
		}
		if (NetworkMapSharer.share.eastOn)
		{
			num++;
		}
		if (NetworkMapSharer.share.southOn)
		{
			num++;
		}
		if (NetworkMapSharer.share.westOn)
		{
			num++;
		}
		return num > 1;
	}
}
