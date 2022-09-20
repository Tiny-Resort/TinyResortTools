using UnityEngine;
using UnityEngine.UI;

public class QuestNotification : MonoBehaviour
{
	public Quest displayingQuest;

	public Text QuestText;

	public void showQuest(Quest questToShow)
	{
		displayingQuest = questToShow;
		QuestText.text = displayingQuest.QuestName;
	}
}
