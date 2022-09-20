using UnityEngine;

public class MailBoxShowsMail : MonoBehaviour
{
	public static MailBoxShowsMail showsMail;

	public GameObject hasMailBox;

	public GameObject noMailBox;

	public bool hasMail;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isMailBox = this;
		MailManager.manage.newMailEvent.AddListener(refresh);
	}

	private void OnEnable()
	{
		showsMail = this;
		refresh();
	}

	public void refresh()
	{
		if (MailManager.manage.mailWindowOpen)
		{
			hasMailBox.gameObject.SetActive(false);
		}
		else if (MailManager.manage.checkIfAnyUndreadLetters())
		{
			hasMailBox.SetActive(true);
			noMailBox.SetActive(false);
		}
		else
		{
			hasMailBox.SetActive(false);
			noMailBox.SetActive(true);
		}
	}
}
