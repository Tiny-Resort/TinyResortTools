using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;

public class NarrationReader : MonoBehaviour
{
	public TextMeshProUGUI textToRead;

	public WindowAnimator myPopup;

	public bool readOutText;

	public int narrationId = -1;

	private void OnEnable()
	{
		if (narrationId != -1)
		{
			textToRead.text = (LocalizedString)("Cutscene/Narration_00" + narrationId);
			textToRead.text = textToRead.text.Replace("<IslandName>", UIAnimationManager.manage.getCharacterNameTag(Inventory.inv.islandName));
			textToRead.text = textToRead.text.Replace("<SouthCity>", UIAnimationManager.manage.getCharacterNameTag("South City"));
		}
		if (readOutText)
		{
			StartCoroutine(lettersAppear());
		}
	}

	private IEnumerator lettersAppear()
	{
		myPopup.refreshAnimation();
		textToRead.maxVisibleCharacters = 0;
		for (int i = 0; i < textToRead.text.Length + 1; i++)
		{
			textToRead.maxVisibleCharacters = i;
			if (i % 2 == 0)
			{
				SoundManager.manage.play2DSound(SoundManager.manage.signTalk);
			}
			yield return null;
		}
	}
}
