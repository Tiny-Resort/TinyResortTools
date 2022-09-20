using UnityEngine;

public class WobbleWhenTouched : MonoBehaviour
{
	public TileObject myTileObject;

	public ASound wobbleSound;

	public bool damageParticlesOn = true;

	public bool playSound = true;

	private void OnTriggerEnter(Collider other)
	{
		if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) < 50f)
		{
			if ((bool)wobbleSound)
			{
				myTileObject.damage(false, damageParticlesOn);
				SoundManager.manage.playASoundAtPoint(wobbleSound, base.transform.position);
			}
			else
			{
				myTileObject.damage(playSound, damageParticlesOn);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (Vector3.Distance(base.transform.position, CameraController.control.transform.position) < 50f)
		{
			myTileObject.damage(false, damageParticlesOn);
		}
	}
}
