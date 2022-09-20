using UnityEngine;

public class RandomEyeDif : MonoBehaviour
{
	public Animator anim;

	private void Start()
	{
		anim.SetFloat("Offset", Random.Range(0.01f, 2f));
		anim.SetFloat("Speed", Random.Range(0.95f, 1.1f));
	}
}
