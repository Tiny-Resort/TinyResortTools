using System.Collections;
using UnityEngine;

public class LightTurnsOnAtNight : MonoBehaviour
{
	public Light[] lightsToChange;

	public float[] originalInt;

	public StopLightStack[] lightStacks;

	public MeshRenderer[] materialToChange;

	public Material nightTimeMat;

	public Material dayTimeMat;

	public bool useMaterialNo;

	public int materialNo;

	public bool interiorOposite;

	private void OnEnable()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			RealWorldTimeLight.time.onDayNightChange.AddListener(turnOnLight);
			if (lightStacks.Length < lightsToChange.Length)
			{
				originalInt = new float[lightsToChange.Length];
				lightStacks = new StopLightStack[lightsToChange.Length];
				for (int i = 0; i < lightsToChange.Length; i++)
				{
					originalInt[i] = lightsToChange[i].intensity;
					lightStacks[i] = lightsToChange[i].GetComponentInParent<StopLightStack>();
				}
			}
			turnOnLightsNoDelay();
		}
		if (interiorOposite)
		{
			for (int j = 0; j < materialToChange.Length; j++)
			{
				materialToChange[j].enabled = true;
			}
			StartCoroutine(hideWhenCamClose());
		}
	}

	private void OnDisable()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			RealWorldTimeLight.time.onDayNightChange.RemoveListener(turnOnLight);
		}
	}

	private void Start()
	{
	}

	private void turnOnLight()
	{
		if (interiorOposite)
		{
			turnOnLightsNoDelay();
		}
		else if (RealWorldTimeLight.time.underGround || ((bool)RealWorldTimeLight.time && RealWorldTimeLight.time.currentHour >= 18))
		{
			turnOnLightsNoDelay();
		}
		else
		{
			Invoke("turnOnLightsNoDelay", Random.Range(0f, 1.8f));
		}
	}

	private void turnOnLightsNoDelay()
	{
		bool flag = RealWorldTimeLight.time.isNightTime;
		if (RealWorldTimeLight.time.underGround)
		{
			flag = true;
		}
		MeshRenderer[] array = materialToChange;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!(meshRenderer != null) || !meshRenderer.gameObject.activeInHierarchy)
			{
				continue;
			}
			Material[] sharedMaterials = meshRenderer.sharedMaterials;
			if (useMaterialNo)
			{
				if ((flag && !interiorOposite) || (!flag && interiorOposite))
				{
					sharedMaterials[materialNo] = nightTimeMat;
				}
				else
				{
					sharedMaterials[materialNo] = dayTimeMat;
				}
			}
			else if (sharedMaterials.Length == 1)
			{
				if ((flag && !interiorOposite) || (!flag && interiorOposite))
				{
					sharedMaterials[0] = nightTimeMat;
				}
				else
				{
					sharedMaterials[0] = dayTimeMat;
				}
			}
			else if ((flag && !interiorOposite) || (!flag && interiorOposite))
			{
				sharedMaterials[1] = nightTimeMat;
			}
			else
			{
				sharedMaterials[1] = dayTimeMat;
			}
			meshRenderer.materials = sharedMaterials;
		}
		if (interiorOposite)
		{
			Light[] array2 = lightsToChange;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = false;
			}
			return;
		}
		for (int j = 0; j < lightsToChange.Length; j++)
		{
			lightsToChange[j].enabled = flag;
			if (flag || RealWorldTimeLight.time.underGround)
			{
				StartCoroutine(slowFadeIn(j));
			}
		}
	}

	public IEnumerator slowFadeIn(int lightId)
	{
		lightsToChange[lightId].intensity = 0f;
		while (!RealWorldTimeLight.time.underGround && RealWorldTimeLight.time.currentHour == 17)
		{
			yield return null;
			lightsToChange[lightId].intensity = Mathf.Lerp(0f, getDesiredInt(lightId), (float)RealWorldTimeLight.time.currentMinute / 60f);
		}
		lightsToChange[lightId].intensity = getDesiredInt(lightId);
	}

	public float getDesiredInt(int lightId)
	{
		if ((bool)lightStacks[lightId])
		{
			return lightStacks[lightId].desiredIntensity;
		}
		return originalInt[lightId];
	}

	private IEnumerator changeLights()
	{
		bool isNightTime = RealWorldTimeLight.time.isNightTime;
		MeshRenderer[] array = materialToChange;
		foreach (MeshRenderer obj in array)
		{
			Material[] materials = obj.materials;
			if (useMaterialNo)
			{
				if (isNightTime)
				{
					materials[materialNo] = nightTimeMat;
				}
				else
				{
					materials[materialNo] = dayTimeMat;
				}
			}
			else if (materials.Length == 1)
			{
				if (isNightTime)
				{
					materials[0] = nightTimeMat;
				}
				else
				{
					materials[0] = dayTimeMat;
				}
			}
			else if (isNightTime)
			{
				materials[1] = nightTimeMat;
			}
			else
			{
				materials[1] = dayTimeMat;
			}
			obj.materials = materials;
			yield return new WaitForSeconds(Random.Range(0.3f, 0.9f));
		}
		Light[] array2 = lightsToChange;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].enabled = isNightTime;
			yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
		}
	}

	private IEnumerator hideWhenCamClose()
	{
		while (true)
		{
			yield return null;
			float num = Vector3.Dot((CameraController.control.cameraTrans.position - base.transform.position).normalized, base.transform.forward);
			if (num > 0f)
			{
				for (int i = 0; i < materialToChange.Length; i++)
				{
					materialToChange[i].enabled = false;
				}
				while (num > 0f)
				{
					yield return null;
					num = Vector3.Dot((CameraController.control.cameraTrans.position - base.transform.position).normalized, base.transform.forward);
				}
				for (int j = 0; j < materialToChange.Length; j++)
				{
					materialToChange[j].enabled = true;
				}
			}
		}
	}
}
