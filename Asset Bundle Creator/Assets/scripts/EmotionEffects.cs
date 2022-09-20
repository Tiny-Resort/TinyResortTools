using UnityEngine;

public class EmotionEffects : MonoBehaviour
{
	public ASound angrySoundEffect;

	public ASound cryingSoundEffect;

	public ASound clapingSound;

	private NPCIdentity myIdentity;

	private void Start()
	{
		myIdentity = base.transform.root.GetComponent<NPCIdentity>();
	}

	public void LaughEffect()
	{
		if ((bool)myIdentity)
		{
			SoundManager.manage.playASoundAtPointWithPitch(NPCManager.manage.NPCDetails[myIdentity.NPCNo].NPCLaugh, base.transform.position, NPCManager.manage.NPCDetails[myIdentity.NPCNo].NPCVoice.getPitch() + Random.Range(0f, 0.2f));
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.playerLaugh, base.transform.position);
		}
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.laughingPart, base.transform.position, 1);
	}

	public void angryEffect()
	{
		SoundManager.manage.playASoundAtPoint(angrySoundEffect, base.transform.position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.angryPart, base.transform.position, 1);
	}

	public void cryingEffect()
	{
		ParticleManager.manage.cryingPart.transform.rotation = Quaternion.Euler(-90f, base.transform.rotation.eulerAngles.y, 0f);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.cryingPart, base.transform.position, 1);
		ParticleManager.manage.cryingPart2.Emit(1);
		SoundManager.manage.playASoundAtPoint(cryingSoundEffect, base.transform.position);
	}

	public void thinkingEffect()
	{
		if ((bool)myIdentity)
		{
			SoundManager.manage.play2DSound(SoundManager.manage.thinkingSound);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.thinkingPart, base.transform.position - base.transform.root.right / 1.5f + Vector3.up / 1.5f, 1);
		}
	}

	public void sighEffect()
	{
	}

	public void playClappingSound()
	{
		SoundManager.manage.playASoundAtPoint(clapingSound, base.transform.position);
	}
}
