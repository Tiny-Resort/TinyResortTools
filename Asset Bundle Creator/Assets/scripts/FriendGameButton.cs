using Steamworks;
using TMPro;
using UnityEngine;

public class FriendGameButton : MonoBehaviour
{
	public CSteamID lobbyId;

	public string ownerId;

	public TextMeshProUGUI buttonText;

	public TextMeshProUGUI numberOfPlayers;

	public TextMeshProUGUI playerNameText;

	public TextMeshProUGUI islandNameText;

	public void onButtonPress()
	{
		SteamLobby.Instance.JoinLobby(lobbyId);
		SteamLobby.Instance.removeFriendsList();
	}

	public void updateNumberOfPlayers(string newTotal, int max)
	{
		numberOfPlayers.text = newTotal + " / " + max;
	}

	public void updatePlayerAndIslandName(string playerName, string islandName)
	{
		playerNameText.text = playerName;
		islandNameText.text = islandName;
	}
}
