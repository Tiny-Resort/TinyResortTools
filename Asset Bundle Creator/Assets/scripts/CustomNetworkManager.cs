using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
	public enum lobbyType
	{
		Friends = 0,
		Invite = 1,
		Public = 2,
		Lan = 3
	}

	public static CustomNetworkManager manage;

	public TMP_InputField ipAddressField;

	public UnityEvent onClientDisconnect = new UnityEvent();

	public UnityEvent onClientConnect = new UnityEvent();

	private bool usingSteam = true;

	private bool connected;

	private bool hosting;

	public Transport steamTransport;

	public Transport lanTransport;

	public Text usingSteamOrLanText;

	public GameObject steamManager;

	private object myConfig;

	public SteamLobby lobby;

	public GameObject disconectedScreen;

	[Header("Lobby Settings-----")]
	private lobbyType selectedLobbyType;

	public TextMeshProUGUI friendGameText;

	public TextMeshProUGUI inviteOnlyText;

	public TextMeshProUGUI publicGameText;

	public TextMeshProUGUI lanGameText;

	public GameObject[] hostingExplainations;

	private void OnEnable()
	{
		manage = this;
		refreshLobbyTypeButtons();
		if (steamManager == null)
		{
			steamManager = GameObject.Find("Steam_Manager");
		}
	}

	public void setLobbyTypeButton(int newType)
	{
		selectedLobbyType = (lobbyType)newType;
		refreshLobbyTypeButtons();
		for (int i = 0; i < hostingExplainations.Length; i++)
		{
			hostingExplainations[i].SetActive(i == newType);
		}
		if (selectedLobbyType == lobbyType.Lan)
		{
			changeTransport(false);
		}
		else
		{
			changeTransport(true);
		}
	}

	public void refreshLobbyTypeButtons()
	{
		friendGameText.text = friendGameText.text.Replace("<sprite=17>", "<sprite=16>");
		inviteOnlyText.text = inviteOnlyText.text.Replace("<sprite=17>", "<sprite=16>");
		publicGameText.text = publicGameText.text.Replace("<sprite=17>", "<sprite=16>");
		lanGameText.text = lanGameText.text.Replace("<sprite=17>", "<sprite=16>");
		if (selectedLobbyType == lobbyType.Friends)
		{
			friendGameText.text = friendGameText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		if (selectedLobbyType == lobbyType.Invite)
		{
			inviteOnlyText.text = inviteOnlyText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		if (selectedLobbyType == lobbyType.Public)
		{
			publicGameText.text = publicGameText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		if (selectedLobbyType == lobbyType.Lan)
		{
			lanGameText.text = lanGameText.text.Replace("<sprite=16>", "<sprite=17>");
		}
	}

	public override void Start()
	{
		if (!Application.isEditor)
		{
			changeTransport(true);
			steamManager.GetComponent<SteamManager>().enabled = true;
			lobby.gameObject.SetActive(true);
		}
		else
		{
			changeTransport(false);
		}
	}

	public void turnSteamOnOrOff()
	{
		changeTransport(!usingSteam);
	}

	public void swapBackToHostButton()
	{
		setLobbyTypeButton((int)selectedLobbyType);
	}

	public void changeTransport(bool newIsSteam)
	{
		usingSteam = newIsSteam;
		lanTransport.enabled = !usingSteam;
		steamTransport.enabled = usingSteam;
		if (usingSteam)
		{
			transport = steamTransport;
			Transport.activeTransport = steamTransport;
		}
		else
		{
			transport = lanTransport;
			Transport.activeTransport = lanTransport;
		}
	}

	public void StartUpHost()
	{
		hosting = true;
		SetPort();
		NetworkManager.singleton.StartHost();
	}

	public void JoinGame()
	{
		SetIpAddress();
		SetPort();
		NetworkManager.singleton.StartClient();
	}

	public void JoinSteamGame(string steamId)
	{
		NetworkManager.singleton.networkAddress = steamId;
		NetworkManager.singleton.StartClient();
	}

	public void disconectClient()
	{
	}

	public void SetPort()
	{
	}

	public void SetIpAddress()
	{
		if (ipAddressField.text == "")
		{
			NetworkManager.singleton.networkAddress = "localhost";
		}
		else
		{
			NetworkManager.singleton.networkAddress = ipAddressField.text;
		}
	}

	public override void OnStartHost()
	{
		MusicManager.manage.changeFromMenu();
	}

	public bool checkIfLanGame()
	{
		return selectedLobbyType == lobbyType.Lan;
	}

	public void createLobbyBeforeConnection()
	{
		if (usingSteam)
		{
			if (selectedLobbyType == lobbyType.Friends)
			{
				SteamLobby.Instance.CreateLobbyWithSettings();
			}
			else if (selectedLobbyType == lobbyType.Invite)
			{
				SteamLobby.Instance.CreateLobbyWithSettings(manage.maxConnections, ELobbyType.k_ELobbyTypePrivate);
			}
			else
			{
				SteamLobby.Instance.CreateLobbyWithSettings(manage.maxConnections, ELobbyType.k_ELobbyTypePublic);
			}
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		SceneManager.LoadScene(1);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		MonoBehaviour.print("Client disconnected");
		onClientDisconnect.Invoke();
		base.OnClientDisconnect(conn);
		if (base.mode != NetworkManagerMode.Host)
		{
			if (connected && !NetworkMapSharer.share.isServer)
			{
				int connectionId = conn.connectionId;
				Inventory.inv.menuOpen = true;
				Inventory.inv.checkIfWindowIsNeeded();
				disconectedScreen.SetActive(true);
			}
			if ((bool)lobby)
			{
				lobby.LeaveGameLobby();
			}
		}
	}

	public void disconectionScreenButton()
	{
		connected = false;
		SceneManager.LoadScene(1);
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		connected = true;
		if (!hosting)
		{
			SaveLoad.saveOrLoad.LoadInv();
			StartCoroutine(loadingOnConnect());
		}
		base.OnClientConnect(conn);
	}

	private IEnumerator loadingOnConnect()
	{
		SaveLoad.saveOrLoad.loadingScreen.appear("connecting");
		cameraWonderOnMenu.wonder.enabled = false;
		yield return new WaitForSeconds(0.5f);
		onClientConnect.Invoke();
		MusicManager.manage.changeFromMenu();
		float time = 0f;
		while (time < 2f)
		{
			SaveLoad.saveOrLoad.loadingScreen.showPercentage(time / 2f);
			time += Time.deltaTime;
			yield return null;
		}
		SaveLoad.saveOrLoad.loadingScreen.completed();
		yield return new WaitForSeconds(0.5f);
		SaveLoad.saveOrLoad.loadingScreen.disappear();
	}

	public void leaveLobbyOnPressMultiplayer()
	{
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		foreach (NetworkIdentity item in new List<NetworkIdentity>(conn.clientOwnedObjects))
		{
			if (!NetworkIdentity.spawned.ContainsKey(item.netId))
			{
				continue;
			}
			Vehicle componentInParent = NetworkIdentity.spawned[item.netId].GetComponentInParent<Vehicle>();
			if (!componentInParent)
			{
				continue;
			}
			componentInParent.GetComponent<NetworkIdentity>().RemoveClientAuthority();
			ControlVehicle component = componentInParent.GetComponent<ControlVehicle>();
			componentInParent.stopDriving();
			if (!component || !component.isGrounded())
			{
				if (WorldManager.manageWorld.waterMap[Mathf.RoundToInt(componentInParent.transform.position.x / 2f), Mathf.RoundToInt(componentInParent.transform.position.z / 2f)])
				{
					componentInParent.transform.position = new Vector3(componentInParent.transform.position.x, 0.6f, componentInParent.transform.position.z);
				}
				else
				{
					componentInParent.transform.position = new Vector3(componentInParent.transform.position.x, WorldManager.manageWorld.heightMap[Mathf.RoundToInt(componentInParent.transform.position.x / 2f), Mathf.RoundToInt(componentInParent.transform.position.z / 2f)], componentInParent.transform.position.z);
				}
			}
		}
		base.OnServerDisconnect(conn);
	}

	public bool isServerRunning()
	{
		return NetworkServer.active;
	}
}
