using UnityEngine;

public class UseTeleCaller : MonoBehaviour
{
	private CharMovement isChar;

	public Conversation teleporterDoesntWork;

	private void Start()
	{
		isChar = base.transform.root.GetComponent<CharMovement>();
	}

	public void doDamageNow()
	{
		if ((bool)isChar && isChar.isLocalPlayer)
		{
			RenderMap.map.canTele = true;
			RenderMap.map.debugTeleport = true;
			MenuButtonsTop.menu.switchToMap();
		}
	}

	public bool moreThanOneTeleOn()
	{
		int num = 0;
		if (NetworkMapSharer.share.privateTowerPos != Vector2.zero)
		{
			num++;
		}
		if (NetworkMapSharer.share.northOn)
		{
			num++;
		}
		if (NetworkMapSharer.share.eastOn)
		{
			num++;
		}
		if (NetworkMapSharer.share.southOn)
		{
			num++;
		}
		if (NetworkMapSharer.share.westOn)
		{
			num++;
		}
		return num > 1;
	}
}
