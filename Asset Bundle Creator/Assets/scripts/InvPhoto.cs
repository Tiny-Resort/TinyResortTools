using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvPhoto : MonoBehaviour
{
	public RawImage photoImage;

	public int frameNo;

	public TextMeshProUGUI islandName;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI dateText;

	public void fillPhotoImage(Texture2D text, int newFrameNo)
	{
		photoImage.enabled = true;
		photoImage.texture = text;
		frameNo = newFrameNo;
		if ((bool)dateText)
		{
			dateText.text = PhotoManager.manage.savedPhotos[frameNo].getDateString() + "\n" + PhotoManager.manage.savedPhotos[frameNo].getTimeString();
			islandName.text = PhotoManager.manage.savedPhotos[frameNo].getIslandName();
			titleText.text = PhotoManager.manage.savedPhotos[frameNo].photoNickname;
		}
	}

	public void updatePhotoId(int newFrameNo)
	{
		frameNo = newFrameNo;
		titleText.text = PhotoManager.manage.savedPhotos[frameNo].photoNickname;
	}

	public void blowUpImage()
	{
		if (photoImage.enabled)
		{
			PhotoManager.manage.blowUpImage(frameNo);
		}
	}
}
