using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class NetworkPlayersManager : MonoBehaviour
{
	public static NetworkPlayersManager manage;

	public GameObject[] playerButttons;

	public TextMeshProUGUI[] playerNames;

	public List<CharMovement> connectedChars = new List<CharMovement>();

	public GameObject multiplayerWindow;

	public GameObject inviteButton;

	public GameObject fakeInviteButton;

	public GameObject LANIPField;

	public TextMeshProUGUI localIp;

	public GameObject singlePlayerOptions;

	public GameObject gamePausedScreen;

	private bool isGamePaused;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		refreshButtons();
	}

	public void addPlayer(CharMovement newChar)
	{
		if (!connectedChars.Contains(newChar) && !newChar.isLocalPlayer)
		{
			connectedChars.Add(newChar);
		}
		StartCoroutine(waitForNameUpdateAndRefreshName(newChar));
	}

	private IEnumerator waitForNameUpdateAndRefreshName(CharMovement newChar)
	{
		while (!newChar.myEquip.nameHasBeenUpdated)
		{
			yield return null;
		}
		refreshButtons();
	}

	public void removePlayer(CharMovement newChar)
	{
		if (connectedChars.Contains(newChar))
		{
			connectedChars.Remove(newChar);
		}
		refreshButtons();
	}

	public void refreshButtons()
	{
		for (int i = 0; i < playerButttons.Length; i++)
		{
			if (i < connectedChars.Count)
			{
				playerButttons[i].SetActive(true);
				playerNames[i].text = connectedChars[i].myEquip.playerName;
			}
			else
			{
				playerButttons[i].SetActive(false);
			}
		}
		if (CustomNetworkManager.manage.checkIfLanGame())
		{
			inviteButton.SetActive(false);
			fakeInviteButton.SetActive(false);
			localIp.text = GetLocalIPAddress();
			if (localIp.text != "")
			{
				LANIPField.SetActive(true);
			}
		}
		else if (connectedChars.Count == 3)
		{
			inviteButton.SetActive(false);
		}
		else
		{
			inviteButton.SetActive(true);
		}
	}

	public void kickPlayer(int id)
	{
		connectedChars[id].TargetKick(connectedChars[id].connectionToClient);
	}

	public void openMultiplayerOptions()
	{
		multiplayerWindow.SetActive(true);
	}

	public void openSinglePlayerOptions()
	{
		singlePlayerOptions.SetActive(true);
	}

	public static string GetLocalIPAddress()
	{
		IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return iPAddress.ToString();
			}
		}
		return "";
	}

	public void pauseButton()
	{
		if (!isGamePaused)
		{
			Time.timeScale = 0f;
			isGamePaused = true;
			StartCoroutine(pauseScreen());
		}
		else
		{
			Time.timeScale = 1f;
			isGamePaused = false;
		}
	}

	public IEnumerator pauseScreen()
	{
		gamePausedScreen.SetActive(true);
		NPCManager.manage.refreshAllAnimators(false);
		while (isGamePaused)
		{
			yield return null;
			if (InputMaster.input.UISelectActiveConfirmButton() || InputMaster.input.UICancel())
			{
				pauseButton();
			}
		}
		NPCManager.manage.refreshAllAnimators(true);
		gamePausedScreen.SetActive(false);
	}

	public void saveButton()
	{
		if (WeatherManager.manage.isInside())
		{
			NotificationManager.manage.createChatNotification("You must be outside to save", true);
		}
		else
		{
			StartCoroutine(SaveLoad.saveOrLoad.saveRoutine(NetworkMapSharer.share.isServer, true, false));
		}
	}
}
