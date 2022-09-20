using System;
using System.Collections.Generic;

[Serializable]
internal class PhotoSave
{
	public PhotoDetails[] allPhotos;

	public PhotoDetails[] displayedPhotosSave;

	public PhotoSave()
	{
	}

	public PhotoSave(List<PhotoDetails> listOfPhotos, PhotoDetails[] displayedPhotos)
	{
		allPhotos = listOfPhotos.ToArray();
		displayedPhotosSave = displayedPhotos;
	}

	public void loadPhotos(bool isClient)
	{
		for (int i = 0; i < allPhotos.Length; i++)
		{
			PhotoManager.manage.savedPhotos.Add(allPhotos[i]);
		}
		if (!isClient && displayedPhotosSave != null)
		{
			for (int j = 0; j < displayedPhotosSave.Length; j++)
			{
				PhotoManager.manage.displayedPhotos[j] = displayedPhotosSave[j];
			}
		}
	}
}
