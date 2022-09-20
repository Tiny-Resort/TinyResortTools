using UnityEngine;

public class CharTalkUse : MonoBehaviour
{
	public LayerMask talkLayerMask;

	public int npcInRange = -1;

	public NPCAI npcTryToTalk;

	private RaycastHit ray;

	public void Update()
	{
		if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out ray, 3f, talkLayerMask))
		{
			NPCIdentity component = ray.transform.GetComponent<NPCIdentity>();
			if ((bool)component)
			{
				npcTryToTalk = ray.transform.GetComponent<NPCAI>();
				npcInRange = component.NPCNo;
				return;
			}
			npcInRange = -1;
		}
		npcInRange = -1;
	}

	public bool talkOrUse()
	{
		if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, out ray, 3f, talkLayerMask))
		{
			NPCAI component = ray.transform.GetComponent<NPCAI>();
			if ((bool)component)
			{
				component.TalkToNPC();
				return true;
			}
		}
		return false;
	}
}
