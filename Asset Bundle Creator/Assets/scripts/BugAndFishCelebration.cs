using System.Collections;
using TMPro;
using UnityEngine;

public class BugAndFishCelebration : MonoBehaviour
{
	public static BugAndFishCelebration bugAndFishCel;

	public GameObject celebrationWindow;

	public TextMeshProUGUI celebrationText;

	public bool celebrationWindowOpen;

	public ASound celebrationSound;

	public Conversation invFullConvo;

	private WindowAnimator myAnim;

	private void Awake()
	{
		bugAndFishCel = this;
		myAnim = celebrationWindow.GetComponent<WindowAnimator>();
	}

	public void openWindow(int invItem)
	{
		celebrationWindowOpen = true;
		Inventory.inv.quickSlotBar.gameObject.SetActive(false);
		Inventory.inv.checkQuickSlotDesc();
		celebrationText.text = "I caught a " + Inventory.inv.allItems[invItem].getInvItemName() + "!";
		SoundManager.manage.play2DSound(celebrationSound);
		MenuButtonsTop.menu.closed = false;
		StartCoroutine(whileOpen(invItem));
	}

	private IEnumerator whileOpen(int itemId)
	{
		StartCoroutine(CameraController.control.zoomInFishOrBug());
		yield return new WaitForSeconds(0.25f);
		celebrationWindow.SetActive(true);
		yield return new WaitForSeconds(0.15f);
		StartCoroutine(lettersAppear());
		while (celebrationWindowOpen)
		{
			yield return null;
			if (InputMaster.input.Interact() || InputMaster.input.Use() || InputMaster.input.UICancel())
			{
				SoundManager.manage.play2DSound(ConversationManager.manage.nextTextSound);
				yield return StartCoroutine(myAnim.closeWithMask(2f));
				closeWindow();
			}
		}
		if (!Inventory.inv.addItemToInventory(itemId, 1, false))
		{
			invFullConvo.startLineAlt.talkingAboutItem = Inventory.inv.allItems[itemId];
			ConversationManager.manage.talkToNPC(NPCManager.manage.sign, invFullConvo);
		}
		Inventory.inv.checkQuickSlotDesc();
	}

	private IEnumerator lettersAppear()
	{
		celebrationText.maxVisibleCharacters = 0;
		yield return new WaitForSeconds(0.25f);
		for (int i = 0; i < celebrationText.textInfo.characterCount + 1; i++)
		{
			if (!celebrationWindowOpen)
			{
				break;
			}
			celebrationText.maxVisibleCharacters = i;
			if (i % 3 == 0)
			{
				SoundManager.manage.play2DSound(SoundManager.manage.signTalk);
			}
			yield return null;
		}
	}

	public void closeWindow()
	{
		MenuButtonsTop.menu.closeButtonDelay();
		celebrationWindow.SetActive(false);
		celebrationWindowOpen = false;
	}
}
