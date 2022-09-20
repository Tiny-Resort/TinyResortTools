using UnityEngine;

public class CameraShake : MonoBehaviour
{
	private float xRotMax = 5f;

	private float yRotMax = 5f;

	private float zRotMax = 5f;

	private float shake;

	private float trauma;

	private void Start()
	{
	}

	private void Update()
	{
		shake = Mathf.Clamp01(trauma * trauma);
		if (shake != 0f)
		{
			float num = xRotMax * shake * Random.Range(-1f, 1f);
			float y = yRotMax * shake * Random.Range(-1f, 1f);
			float z = zRotMax * shake * Random.Range(-1f, 1f);
			Vector3 localEulerAngles = new Vector3(50f + num, y, z);
			base.transform.localEulerAngles = localEulerAngles;
			trauma = Mathf.Clamp01(trauma - Time.deltaTime / 2f);
		}
	}

	public void addToTrauma(float zeroToOne)
	{
		trauma = Mathf.Clamp01(trauma + zeroToOne);
	}

	public void addToTraumaMax(float zeroToOne, float max)
	{
		if (trauma < max)
		{
			trauma = Mathf.Clamp01(trauma + zeroToOne);
		}
	}
}
