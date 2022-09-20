using UnityEngine;

public class conversationExporter : MonoBehaviour
{
	public Conversation[] toExport;

	public bool exportNow;

	private void Update()
	{
		if (exportNow)
		{
			exportNow = false;
			for (int i = 0; i < toExport.Length; i++)
			{
				toExport[i].forceNewFillConvoTranslations();
			}
		}
	}
}
