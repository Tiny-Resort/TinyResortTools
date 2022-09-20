using System.Collections;
using UnityEngine;

public class CheckEnterWater : MonoBehaviour
{
	private bool isInWater;

	private void Start()
	{
		StartCoroutine(checkWater());
	}

	private IEnumerator checkWater()
	{
		while (true)
		{
			yield return null;
			yield return null;
			yield return null;
			if (base.transform.position.y < 0.6f)
			{
				if (!WorldManager.manageWorld.isPositionOnMap((int)base.transform.position.x / 2, (int)base.transform.position.z / 2))
				{
					continue;
				}
				if (WorldManager.manageWorld.waterMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2])
				{
					if (!isInWater)
					{
						ParticleManager.manage.waterSplash(base.transform.position);
						SoundManager.manage.playASoundAtPoint(SoundManager.manage.treadWater, base.transform.position);
						isInWater = true;
					}
					ParticleManager.manage.waterWakePart(base.transform.position, 2);
				}
				else if (isInWater)
				{
					SoundManager.manage.playASoundAtPoint(SoundManager.manage.treadWater, base.transform.position);
					isInWater = false;
					ParticleManager.manage.waterWakePart(base.transform.position, 2);
				}
			}
			else if (isInWater)
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.treadWater, base.transform.position);
				isInWater = false;
				ParticleManager.manage.waterWakePart(base.transform.position, 2);
			}
		}
	}
}
