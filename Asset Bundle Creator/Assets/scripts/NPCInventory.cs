using System;

[Serializable]
public class NPCInventory
{
	public bool isVillager;

	public bool isFem;

	public int nameId;

	public int hairId;

	public int hairColorId;

	public int eyesId;

	public int eyeColorId;

	public int noseId;

	public int mouthId;

	public int shirtId;

	public int pantsId;

	public int shoesId;

	public int skinId;

	public bool hasBeenRequested = true;

	public void fillAppearanceInv(bool voice, int name, int skin, int hair, int hairColour, int eye, int eyeColour, int shirt, int pants, int shoes)
	{
		hasBeenRequested = true;
		isVillager = true;
		isFem = voice;
		nameId = name;
		skinId = skin;
		hairId = hair;
		hairColorId = hairColour;
		eyesId = eye;
		eyeColorId = eyeColour;
		shirtId = shirt;
		pantsId = pants;
		shoesId = shoes;
	}
}
