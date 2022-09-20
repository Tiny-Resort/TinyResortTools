using UnityEngine;

public class SynchronizeLights : MonoBehaviour
{
	public Light light0;

	public Light light1;

	private void LateUpdate()
	{
		if ((bool)light0)
		{
			Vector3 vector = light0.transform.rotation * new Vector3(0f, 0f, -1f);
			GetComponent<Renderer>().material.SetVector("_LightDirection0", new Vector4(vector.x, vector.y, vector.z, 0f));
			GetComponent<Renderer>().material.SetColor("_MyLightColor0", light0.color);
		}
		if ((bool)light1)
		{
			Vector3 vector2 = light1.transform.rotation * new Vector3(0f, 0f, -1f);
			GetComponent<Renderer>().material.SetVector("_LightDirection1", new Vector4(vector2.x, vector2.y, vector2.z, 0f));
			GetComponent<Renderer>().material.SetColor("_MyLightColor1", light1.color);
		}
	}
}
