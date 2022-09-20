using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
	public static SteamLobby instance;

	public CustomNetworkManager netManager;

	public GameObject friendsGameButton;

	public Transform friendsGameButtonTrans;

	public InvButton joinButton;

	public InvButton joinWorldButton;

	public GameObject loadingBar;

	public GameObject noGamesFound;

	private Coroutine loadingCoroutine;

	public MultiplayerLoadWindow multiplayerLoadWindow;

	private Callback<LobbyEnter_t> m_LobbyEntered;

	private Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;

	private Callback<LobbyMatchList_t> m_ListOfLobbies;

	private Callback<LobbyDataUpdate_t> Callback_lobbyInfo;

	private Callback<LobbyChatUpdate_t> m_LobbyUpdate;

	public CSteamID currentLobby;

	private List<CSteamID> listOfLobbyIds;

	private List<FriendGameButton> friendButtons = new List<FriendGameButton>();

	public CustomNetworkManager.lobbyType searchType;

	public TextMeshProUGUI friendSearchButtonText;

	public TextMeshProUGUI publicSearchButtonText;

	public static SteamLobby Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<SteamLobby>();
				return instance;
			}
			return instance;
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		changeSearchType(0);
		SteamAPI.RunCallbacks();
		m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
		m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
		m_ListOfLobbies = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
		m_LobbyUpdate = Callback<LobbyChatUpdate_t>.Create(onLobbyMembersChange);
		Callback_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
	}

	public void changeSearchType(int newType)
	{
		friendSearchButtonText.text = friendSearchButtonText.text.Replace("<sprite=17>", "<sprite=16>");
		publicSearchButtonText.text = publicSearchButtonText.text.Replace("<sprite=17>", "<sprite=16>");
		if (newType == 0)
		{
			searchType = CustomNetworkManager.lobbyType.Friends;
			friendSearchButtonText.text = friendSearchButtonText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		else
		{
			searchType = CustomNetworkManager.lobbyType.Public;
			publicSearchButtonText.text = publicSearchButtonText.text.Replace("<sprite=16>", "<sprite=17>");
		}
	}

	public void removeFriendsList()
	{
		foreach (FriendGameButton friendButton in friendButtons)
		{
			Object.Destroy(friendButton.gameObject);
		}
		friendButtons.Clear();
	}

	public void searchForLobbies()
	{
		if (SteamManager.Initialized)
		{
			foreach (FriendGameButton friendButton in friendButtons)
			{
				Object.Destroy(friendButton.gameObject);
			}
			friendButtons.Clear();
			if (searchType == CustomNetworkManager.lobbyType.Friends)
			{
				int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
				for (int i = 0; i < friendCount; i++)
				{
					CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
					FriendGameInfo_t pFriendGameInfo;
					if (SteamFriends.GetFriendGamePlayed(friendByIndex, out pFriendGameInfo) && pFriendGameInfo.m_steamIDLobby.IsValid() && pFriendGameInfo.m_gameID.AppID() == (AppId_t)1062520u)
					{
						FriendGameButton component = Object.Instantiate(friendsGameButton, friendsGameButtonTrans).GetComponent<FriendGameButton>();
						component.lobbyId = pFriendGameInfo.m_steamIDLobby;
						component.ownerId = friendByIndex.ToString();
						component.buttonText.text = SteamFriends.GetFriendPersonaName(friendByIndex);
						SteamMatchmaking.RequestLobbyData(pFriendGameInfo.m_steamIDLobby);
						friendButtons.Add(component);
						component.gameObject.SetActive(false);
						component.enabled = false;
					}
				}
			}
			else
			{
				SteamMatchmaking.RequestLobbyList();
			}
			if (loadingCoroutine == null)
			{
				loadingCoroutine = StartCoroutine(refreshButtonRoll());
			}
		}
		else
		{
			loadingBar.SetActive(false);
			noGamesFound.SetActive(false);
		}
	}

	private IEnumerator refreshButtonRoll()
	{
		loadingBar.SetActive(true);
		noGamesFound.SetActive(false);
		yield return new WaitForSeconds(3f);
		if (friendButtons.Count <= 0)
		{
			int extraSearchTimes = 3;
			while (extraSearchTimes > 0 && friendButtons.Count <= 0)
			{
				float num = 0f;
				if (num < 3f)
				{
					float num2 = num + Time.deltaTime;
					yield return null;
					if (friendButtons.Count <= 0)
					{
						break;
					}
				}
				searchForLobbies();
				extraSearchTimes--;
			}
			if (friendButtons.Count <= 0)
			{
				noGamesFound.SetActive(true);
			}
		}
		else
		{
			noGamesFound.SetActive(false);
		}
		loadingBar.SetActive(false);
		loadingCoroutine = null;
	}

	private void OnGetLobbyInfo(LobbyDataUpdate_t result)
	{
		for (int i = 0; i < friendButtons.Count; i++)
		{
			if (friendButtons[i].lobbyId.m_SteamID == result.m_ulSteamIDLobby)
			{
				friendButtons[i].updateNumberOfPlayers(SteamMatchmaking.GetLobbyData(friendButtons[i].lobbyId, "noOfPlayers"), SteamMatchmaking.GetLobbyMemberLimit(friendButtons[i].lobbyId));
				friendButtons[i].updatePlayerAndIslandName(SteamMatchmaking.GetLobbyData(friendButtons[i].lobbyId, "characterName"), SteamMatchmaking.GetLobbyData(friendButtons[i].lobbyId, "islandName"));
				if (searchType == CustomNetworkManager.lobbyType.Friends && SteamMatchmaking.GetLobbyData(friendButtons[i].lobbyId, "ownerId") != friendButtons[i].ownerId.ToString())
				{
					friendButtons[i].gameObject.SetActive(false);
				}
				else
				{
					friendButtons[i].gameObject.SetActive(true);
				}
			}
		}
	}

	public void CreateLobbyWithSettings(int maxConnection = 4, ELobbyType lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly)
	{
		Debug.Log("STEAM - CreateLobbyWithSettings");
		if (!SteamManager.Initialized)
		{
			Debug.Log("STEAM - fail to create lobby");
			return;
		}
		CSteamID currentLobby2 = currentLobby;
		Debug.Log("STEAM - Leaving Previous lobby");
		LeaveGameLobby();
		SteamMatchmaking.CreateLobby(lobbyType, maxConnection);
	}

	private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t pCallback)
	{
		Debug.Log("STEAM - Joining lobby through request");
		JoinLobby(pCallback.m_steamIDLobby);
	}

	public void JoinLobby(CSteamID lobbyId)
	{
		Debug.Log("STEAM - Joining LobbyID " + lobbyId.ToString());
		CSteamID currentLobby2 = currentLobby;
		Debug.Log("STEAM - Leaving Previous lobby");
		LeaveGameLobby();
		if (SteamManager.Initialized)
		{
			SteamMatchmaking.JoinLobby(lobbyId);
		}
	}

	private void OnLobbyEntered(LobbyEnter_t pCallback)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		Debug.Log("STEAM - OnLobbyEntered");
		currentLobby = new CSteamID(pCallback.m_ulSteamIDLobby);
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(currentLobby);
		CSteamID steamID = SteamUser.GetSteamID();
		if (lobbyOwner.m_SteamID != steamID.m_SteamID)
		{
			netManager.ipAddressField.text = lobbyOwner.ToString();
			if (multiplayerLoadWindow.characterSelectedOnWindow)
			{
				joinButton.onButtonPress.Invoke();
			}
			else
			{
				multiplayerLoadWindow.openForInvite();
			}
		}
		else
		{
			SteamMatchmaking.SetLobbyData(currentLobby, "characterName", Inventory.inv.playerName);
			SteamMatchmaking.SetLobbyData(currentLobby, "islandName", Inventory.inv.islandName);
			SteamMatchmaking.SetLobbyData(currentLobby, "ownerId", SteamUser.GetSteamID().ToString());
		}
	}

	private void onLobbyMembersChange(LobbyChatUpdate_t result)
	{
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(currentLobby);
		CSteamID steamID = SteamUser.GetSteamID();
		if (lobbyOwner.m_SteamID == steamID.m_SteamID)
		{
			SteamMatchmaking.SetLobbyData(currentLobby, "noOfPlayers", SteamMatchmaking.GetNumLobbyMembers(currentLobby).ToString());
		}
	}

	public void LeaveGameLobbyButton()
	{
		if (SteamManager.Initialized)
		{
			LeaveGameLobby();
		}
	}

	public void LeaveGameLobby()
	{
		if (SteamManager.Initialized)
		{
			Debug.Log("STEAM - Leaving lobby " + currentLobby.ToString());
			SteamMatchmaking.LeaveLobby(currentLobby);
		}
		Debug.Log("STEAM - Disconnected");
		currentLobby = CSteamID.Nil;
	}

	private void OnGetLobbiesList(LobbyMatchList_t result)
	{
		Debug.Log("Found " + result.m_nLobbiesMatching + " lobbies!");
		for (int i = 0; i < result.m_nLobbiesMatching; i++)
		{
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(i);
			FriendGameButton component = Object.Instantiate(friendsGameButton, friendsGameButtonTrans).GetComponent<FriendGameButton>();
			component.lobbyId = lobbyByIndex;
			component.buttonText.text = "Public Game";
			friendButtons.Add(component);
			SteamMatchmaking.RequestLobbyData(lobbyByIndex);
		}
	}

	public void pressInviteButton()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(currentLobby);
		}
	}
}
