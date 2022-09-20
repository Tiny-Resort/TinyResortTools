using UnityEngine;

public class DoorOpen : MonoBehaviour
{
	public bool closeOnly;

	public ASound playOtherSoundToo;

	public ASound altOpenSound;

	public ASound altCloseSound;

	public Animator doorAnim;

	public EntryExit connectedToEntryExit;

	public GameObject signToShowOnClose;

	private bool isOpen;

	public int openOnlyOnNPCId = -1;

	private void Start()
	{
		if ((bool)connectedToEntryExit)
		{
			connectedToEntryExit.feedInNPCId(openOnlyOnNPCId);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((other.gameObject.layer == LayerMask.NameToLayer("Char") || other.gameObject.layer == LayerMask.NameToLayer("NPC")) && (!connectedToEntryExit || connectedToEntryExit.canEnter()) && checkIfFacingDoor(other.transform))
		{
			openDoor();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.layer != LayerMask.NameToLayer("Char") && other.gameObject.layer != LayerMask.NameToLayer("NPC"))
		{
			return;
		}
		if ((bool)signToShowOnClose && (bool)connectedToEntryExit && !connectedToEntryExit.canEnter())
		{
			signToShowOnClose.SetActive(true);
		}
		else if ((bool)signToShowOnClose && (bool)connectedToEntryExit && connectedToEntryExit.canEnter())
		{
			signToShowOnClose.SetActive(false);
		}
		if ((bool)connectedToEntryExit && !connectedToEntryExit.canEnter())
		{
			closeDoor();
		}
		else if (!closeOnly)
		{
			if (checkIfFacingDoor(other.transform))
			{
				openDoor();
			}
			else
			{
				closeDoor();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Char") || other.gameObject.layer == LayerMask.NameToLayer("NPC"))
		{
			closeDoor();
			if ((bool)connectedToEntryExit)
			{
				connectedToEntryExit.canEnter();
			}
		}
	}

	public void closeDoor()
	{
		if ((bool)doorAnim && isOpen)
		{
			isOpen = false;
			doorAnim.SetTrigger("Close");
		}
		if (closeOnly)
		{
			playCloseSound();
		}
	}

	public void openDoor()
	{
		if ((bool)doorAnim && !isOpen)
		{
			isOpen = true;
			doorAnim.SetTrigger("Open");
		}
		if (closeOnly)
		{
			playCloseSound();
			if ((bool)playOtherSoundToo)
			{
				SoundManager.manage.playASoundAtPoint(playOtherSoundToo, base.transform.position);
			}
		}
	}

	private bool checkIfFacingDoor(Transform checkTrans)
	{
		return true;
	}

	public void playCloseSound()
	{
		if ((bool)altCloseSound)
		{
			SoundManager.manage.playASoundAtPoint(altCloseSound, base.transform.position);
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.doorClose, base.transform.position);
		}
	}

	public void playOpenSound()
	{
		if ((bool)altOpenSound)
		{
			SoundManager.manage.playASoundAtPoint(altOpenSound, base.transform.position);
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.doorOpen, base.transform.position);
		}
	}
}
