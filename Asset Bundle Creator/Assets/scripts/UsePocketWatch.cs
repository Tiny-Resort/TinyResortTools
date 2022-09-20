using UnityEngine;

public class UsePocketWatch : MonoBehaviour
{
	public ASound soundOnUse;

	public void usePocketWatch()
	{
		SoundManager.manage.playASoundAtPoint(soundOnUse, base.transform.position);
		if (NetworkMapSharer.share.isServer)
		{
			if (RealWorldTimeLight.time.getCurrentSpeed() == 2f)
			{
				RealWorldTimeLight.time.changeSpeed(0.05f);
			}
			else if (RealWorldTimeLight.time.getCurrentSpeed() == 0.05f)
			{
				RealWorldTimeLight.time.changeSpeed(2f);
			}
		}
	}
}
