using System.Collections;
using UnityEngine;

public class EatItem : MonoBehaviour
{
	public ASound chewsound;

	public ParticleSystem chewParticle;

	private Animator parentAnim;

	private CharMovement myCharMovement;

	private NPCHoldsItems npcHold;

	private CharacterHeadIK headIk;

	private Animator foodAnim;

	private CharPickUp myPickup;

	private bool usingChanger;

	public void Start()
	{
		npcHold = base.transform.root.GetComponent<NPCHoldsItems>();
		myCharMovement = GetComponentInParent<CharMovement>();
		myPickup = GetComponentInParent<CharPickUp>();
		if ((bool)myCharMovement)
		{
			parentAnim = myCharMovement.myAnim;
			headIk = myCharMovement.GetComponent<CharacterHeadIK>();
			foodAnim = GetComponent<Animator>();
			StartCoroutine(slowWalkOnUse());
		}
		if ((bool)npcHold)
		{
			parentAnim = npcHold.GetComponent<Animator>();
		}
		if (!parentAnim)
		{
			GetComponent<Animator>().Rebind();
			Object.Destroy(GetComponent<Animator>());
		}
	}

	private IEnumerator slowWalkOnUse()
	{
		while (true)
		{
			if (myCharMovement.localUsing)
			{
				myCharMovement.isSneaking(true);
			}
			else
			{
				myCharMovement.isSneaking(false);
			}
			if ((bool)headIk)
			{
				if (myCharMovement.myEquip.usingItem)
				{
					headIk.setIsEating(true);
				}
				else
				{
					headIk.setIsEating(false);
				}
			}
			yield return null;
			RaycastHit hitInfo;
			if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out hitInfo, 3f, myPickup.pickUpLayerMask))
			{
				if ((bool)hitInfo.transform.GetComponentInParent<ItemDepositAndChanger>())
				{
					foodAnim.SetBool("TooFull", true);
					usingChanger = true;
				}
				else
				{
					foodAnim.SetBool("TooFull", StatusManager.manage.isTooFull());
					usingChanger = false;
				}
			}
			else
			{
				foodAnim.SetBool("TooFull", StatusManager.manage.isTooFull());
				usingChanger = false;
			}
		}
	}

	public void OnDestroy()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.isSneaking(false);
		}
		if ((bool)headIk)
		{
			headIk.setIsEating(false);
		}
	}

	public void giveToFullWarning()
	{
		if (!usingChanger)
		{
			NotificationManager.manage.pocketsFull.showTooFull();
		}
	}

	public void EatObject()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && !StatusManager.manage.isTooFull())
		{
			eatObjectBenefits();
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.EatSomething);
			Inventory.inv.consumeItemInHand();
			StatusManager.manage.addToFullness();
		}
	}

	private void npcEatRemoveDelay()
	{
		base.gameObject.SetActive(false);
	}

	public void PlayEatSound()
	{
		SoundManager.manage.playASoundAtPoint(chewsound, base.transform.position);
		if ((bool)chewParticle)
		{
			chewParticle.Emit(8);
		}
	}

	public void playEatAnimation()
	{
		if ((bool)parentAnim)
		{
			parentAnim.SetTrigger("Eating");
		}
	}

	private void eatObjectBenefits()
	{
		if (Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.consumeable.givesTempPoints)
		{
			StatusManager.manage.addTempPoints(Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.consumeable.tempHealthGain, Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.consumeable.tempStaminaGain);
		}
		else
		{
			StatusManager.manage.changeStatus(Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.consumeable.healthGain, Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.consumeable.staminaGain);
		}
		Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.consumeable.giveBuffs();
	}
}
