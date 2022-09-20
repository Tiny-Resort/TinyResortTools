using System.Collections;
using UnityEngine;

public class BalloonFlyAway : MonoBehaviour
{
	public Transform balloonToFly;

	public Transform cameraPos;

	public GameObject invisibleWalls;

	private void OnEnable()
	{
		if ((bool)NetworkMapSharer.share.localChar)
		{
			StartCoroutine(StartFlyAway());
		}
	}

	private IEnumerator StartFlyAway()
	{
		invisibleWalls.SetActive(true);
		while (!NetworkMapSharer.share)
		{
			yield return null;
		}
		while (!NetworkMapSharer.share.localChar)
		{
			yield return null;
		}
		yield return null;
		yield return null;
		while (ConversationManager.manage.inConversation || WeatherManager.manage.isInside())
		{
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		while (balloonToFly.position.y < 5f)
		{
			balloonToFly.position += Vector3.up * Time.deltaTime;
			yield return null;
		}
		while (balloonToFly.position.y < 55f)
		{
			yield return null;
			balloonToFly.position = balloonToFly.position + Vector3.up * Time.deltaTime * 2f + Vector3.left * Time.deltaTime * 2f;
		}
		while (Vector3.Distance(NetworkMapSharer.share.localChar.transform.position, invisibleWalls.transform.position) < 15f)
		{
			MonoBehaviour.print("In range of invisible walls");
			yield return null;
		}
		invisibleWalls.SetActive(false);
		NetworkMapSharer.share.RpcGiveOnTileStatus(0, TownManager.manage.startingDockPosition[0], TownManager.manage.startingDockPosition[1]);
	}
}
