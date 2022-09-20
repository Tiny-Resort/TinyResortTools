using UnityEngine;

public class randomRotationOnObject : MonoBehaviour
{
	private void OnEnable()
	{
		Random.InitState(Mathf.RoundToInt(base.transform.position.x * base.transform.position.z + (base.transform.position.z - base.transform.position.x) * base.transform.position.x));
		base.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 720f), 0f);
	}
}
