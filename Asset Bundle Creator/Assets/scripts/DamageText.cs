using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
	public Text damageText;

	private Transform connectedTransform;

	private Vector3 randomDif = Vector3.zero;

	private float height;

	private void Update()
	{
		if ((bool)connectedTransform)
		{
			Vector3 position = connectedTransform.position + randomDif + Vector3.up * height;
			position = CameraController.control.mainCamera.WorldToScreenPoint(position);
			base.transform.position = Vector3.Lerp(base.transform.position, position, Time.deltaTime * 2f);
			height = Mathf.Lerp(height, 6f, Time.deltaTime * 3f);
		}
	}

	public void displayDamage(int damageAmount, Transform connectToTrans)
	{
		connectedTransform = connectToTrans;
		damageText.text = damageAmount.ToString() ?? "";
		randomDif = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 1f), Random.Range(-0.5f, 0.5f));
		base.transform.position = CameraController.control.mainCamera.WorldToScreenPoint(connectedTransform.position + randomDif + Vector3.up * height);
	}

	public void destroy()
	{
		Object.Destroy(base.gameObject);
	}
}
