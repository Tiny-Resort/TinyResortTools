using UnityEngine;

public class DealFireDamage : MonoBehaviour
{
	public int additionalDamage;

	private void OnTriggerEnter(Collider other)
	{
		Damageable componentInParent = other.GetComponentInParent<Damageable>();
		if ((bool)componentInParent)
		{
			if (NetworkMapSharer.share.isServer)
			{
				componentInParent.setOnFire();
			}
			else if ((bool)NetworkMapSharer.share.localChar)
			{
				NetworkMapSharer.share.localChar.CmdSetOnFire(componentInParent.netId);
			}
		}
	}
}
