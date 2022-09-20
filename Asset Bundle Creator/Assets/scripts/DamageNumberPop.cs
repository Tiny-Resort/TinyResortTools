using TMPro;
using UnityEngine;

public class DamageNumberPop : MonoBehaviour
{
	public TextMeshPro damageText;

	private float leftOrRight;

	private void Start()
	{
		Invoke("destroySelf", 3.25f);
	}

	public void setDamageText(int number)
	{
		damageText.text = number.ToString() ?? "";
		leftOrRight = Random.Range(-1f, 1f);
	}

	private void destroySelf()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		base.transform.LookAt(CameraController.control.cameraTrans);
		base.transform.position += new Vector3(leftOrRight / 10f, 0f, (0f - leftOrRight) / 10f);
		leftOrRight /= 10f;
	}
}
