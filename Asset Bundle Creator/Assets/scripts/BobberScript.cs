using UnityEngine;

public class BobberScript : MonoBehaviour
{
	public bool catchSharks;

	public FishingRodCastAndReel connectedToRod;

	public ASound waterBloop;

	public void fishIsBiting()
	{
	}

	public void fishStopBiting()
	{
	}

	public void bobberLandInWater()
	{
		ParticleManager.manage.waterSplash(base.transform.position);
		SoundManager.manage.playASoundAtPoint(waterBloop, base.transform.position);
	}

	public void bobberSmallBloop()
	{
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.fishBite, base.transform.position);
		Vector3 position = base.transform.position;
		position.y = 0.61f;
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.fishSplash, position, 10);
		Invoke("splashDelay", 0.02f);
	}

	public void bobberFakeBloop()
	{
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.fishFakeBite, base.transform.position);
	}

	public void splashDelay()
	{
		Vector3 position = base.transform.position;
		position.y = 0.61f;
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.fishSplash, position, 10);
	}
}
