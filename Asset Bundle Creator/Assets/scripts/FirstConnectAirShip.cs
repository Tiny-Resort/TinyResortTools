using System.Collections;
using UnityEngine;

public class FirstConnectAirShip : MonoBehaviour
{
	public static FirstConnectAirShip connect;

	public GameObject airshipToHide;

	public Transform followPos;

	private float followHeight = 1f;

	public Conversation introConvo;

	public Conversation landingConvo;

	public ASound airshipLands;

	public AudioSource shipSound;

	private bool needsSound = true;

	private void Awake()
	{
		connect = this;
	}

	private IEnumerator Start()
	{
		StartCoroutine(matchShipSoundToMaster());
		NetworkMapSharer.share.fadeToBlack.setBlack();
		CameraController.control.GetComponent<AudioListener>().enabled = false;
		CameraController.control.setFollowTransform(followPos);
		followHeight = Mathf.Clamp(Mathf.Lerp(followHeight, WorldManager.manageWorld.heightMap[(int)followPos.transform.position.x / 2, (int)followPos.transform.position.z / 2], Time.fixedDeltaTime * 5f), 1f, 5f) + 5f;
		followPos.transform.position = new Vector3(followPos.transform.position.x, followHeight, followPos.transform.position.z);
		CameraController.control.transform.position = followPos.transform.position;
		NewChunkLoader.loader.forceInstantUpdateAtPos();
		CameraController.control.transform.eulerAngles = new Vector3(0f, 90f, 0f);
		CameraController.control.lockCamera(true);
		while (!WorldManager.manageWorld.netMapSharer.isActiveAndEnabled)
		{
			yield return null;
		}
		while (!NetworkMapSharer.share.localChar)
		{
			yield return null;
		}
		NetworkMapSharer.share.fadeToBlack.fadeTime = 3f;
		NetworkMapSharer.share.fadeToBlack.fadeOut();
		CameraController.control.GetComponent<AudioListener>().enabled = true;
		ConversationManager.manage.talkToNPC(NPCManager.manage.sign, introConvo);
		bool fadeOutStarted = false;
		float textTimer = 8f;
		while (base.transform.position.x < WorldManager.manageWorld.spawnPos.position.x + 20f)
		{
			yield return new WaitForFixedUpdate();
			base.transform.position = base.transform.position + Vector3.right * 10f * Time.fixedDeltaTime;
			followPos.transform.position = new Vector3(followPos.transform.position.x, followHeight, followPos.transform.position.z);
			followHeight = Mathf.Clamp(Mathf.Lerp(followHeight, WorldManager.manageWorld.heightMap[(int)followPos.transform.position.x / 2, (int)followPos.transform.position.z / 2], Time.fixedDeltaTime * 5f), 1f, 5f) + 5f;
			if (ConversationManager.manage.inConversation && !ConversationManager.manage.ready)
			{
				textTimer -= Time.deltaTime;
				if (textTimer <= 0f && !ConversationManager.manage.ready)
				{
					ConversationManager.manage.ready = true;
					yield return null;
					ConversationManager.manage.ready = false;
					textTimer = 8f;
				}
			}
			if (!fadeOutStarted && WorldManager.manageWorld.spawnPos.position.x - base.transform.position.x <= 20f)
			{
				fadeOutStarted = true;
				NetworkMapSharer.share.fadeToBlack.fadeIn();
			}
		}
		yield return new WaitForSeconds(1f);
		Transform spawnPos = TownManager.manage.allShopFloors[13].transform.Find("FirstConnectSpawnPos").transform;
		if (Vector3.Distance(NetworkMapSharer.share.localChar.transform.position, spawnPos.position) >= 1f)
		{
			NetworkMapSharer.share.localChar.transform.position = spawnPos.position;
		}
		NetworkMapSharer.share.fadeToBlack.fadeTime = 0.75f;
		airshipToHide.SetActive(false);
		CameraController.control.lockCamera(false);
		CameraController.control.transform.eulerAngles = new Vector3(0f, 0f, 0f);
		CameraController.control.setFollowTransform(NetworkMapSharer.share.localChar.transform);
		CameraController.control.transform.position = NetworkMapSharer.share.localChar.transform.position;
		needsSound = false;
		yield return new WaitForSeconds(1f);
		if (Vector3.Distance(NetworkMapSharer.share.localChar.transform.position, spawnPos.position) >= 1f)
		{
			NetworkMapSharer.share.localChar.transform.position = spawnPos.position;
		}
		CameraController.control.shakeScreenMax(0.45f, 0.45f);
		SoundManager.manage.play2DSound(airshipLands);
		NetworkMapSharer.share.RpcGiveOnTileStatus(1, TownManager.manage.startingDockPosition[0], TownManager.manage.startingDockPosition[1]);
		NPCManager.manage.setUpNPCAgent(6, TownManager.manage.startingDockPosition[0], TownManager.manage.startingDockPosition[1]);
		NPCManager.manage.npcStatus[6].moveInNPC();
		NPCManager.manage.npcStatus[6].hasMet = true;
		NPCManager.manage.npcStatus[6].hasAskedToMoveIn = true;
		NPCManager.manage.moveNpcToPlayerAndStartTalking(6, false, QuestManager.manage.allQuests[0].transform.Find("SpecialIntro").GetComponent<Conversation>());
		NPCManager.manage.setNPCInSideBuilding(6, NPCSchedual.Locations.Harbour_House);
		NPCMapAgent fletchMapAgent = NPCManager.manage.getMapAgentForNPC(6);
		CapsuleCollider[] coliders = fletchMapAgent.activeNPC.gameObject.GetComponents<CapsuleCollider>();
		for (int i = 0; i < coliders.Length; i++)
		{
			coliders[i].enabled = false;
		}
		while (fletchMapAgent.activeNPC.transform.position.y < -10f)
		{
			yield return null;
		}
		TownManager.manage.firstConnect = false;
		ControlTutorial.tutorial.startTutorial();
		while (WeatherManager.manage.isInside())
		{
			yield return null;
		}
		if (!CameraController.control.isFreeCamOn())
		{
			CameraController.control.swapFreeCam();
		}
		WorldManager.manageWorld.spawnPos.position = GenerateMap.generate.originalSpawnPoint;
		NetworkMapSharer.share.personalSpawnPoint = WorldManager.manageWorld.spawnPos.position;
		MusicManager.manage.changeFromMenu();
		for (int j = 0; j < coliders.Length; j++)
		{
			coliders[j].enabled = true;
		}
		fletchMapAgent.activeNPC.myAgent.Warp(TownManager.manage.allShopFloors[13].workPos.transform.position);
		fletchMapAgent.activeNPC.transform.position = TownManager.manage.allShopFloors[13].workPos.transform.position;
		StartCoroutine(makeFletchStandInSpot());
		while (Vector3.Distance(NetworkMapSharer.share.localChar.transform.position, TownManager.manage.allShopFloors[13].workPos.transform.position) >= 2.6f && !(NetworkMapSharer.share.localChar.transform.position.x < TownManager.manage.allShopFloors[13].workPos.transform.position.x - 2.5f) && !ConversationManager.manage.inConversation)
		{
			yield return null;
		}
		if (!ConversationManager.manage.inConversation)
		{
			NPCManager.manage.moveNpcToPlayerAndStartTalking(6, true);
		}
	}

	private IEnumerator makeFletchStandInSpot()
	{
		NPCMapAgent fletchMapAgent = NPCManager.manage.getMapAgentForNPC(6);
		while (!QuestManager.manage.isQuestCompleted[0])
		{
			if ((bool)fletchMapAgent.activeNPC && fletchMapAgent.activeNPC.talkingTo == 0)
			{
				fletchMapAgent.activeNPC.myAgent.SetDestination(TownManager.manage.allShopFloors[13].workPos.transform.position);
				if (Vector3.Distance(fletchMapAgent.activeNPC.transform.position, TownManager.manage.allShopFloors[13].workPos.transform.position) <= 1f)
				{
					fletchMapAgent.activeNPC.myAgent.transform.rotation = Quaternion.Slerp(fletchMapAgent.activeNPC.myAgent.transform.rotation, TownManager.manage.allShopFloors[13].workPos.transform.rotation, Time.deltaTime * 2f);
				}
			}
			yield return null;
		}
	}

	public IEnumerator matchShipSoundToMaster()
	{
		float baseVolume = shipSound.volume;
		while (needsSound)
		{
			shipSound.volume = baseVolume * SoundManager.manage.getSoundVolume();
			yield return null;
		}
	}
}
