using System;

[Serializable]
public class PlayerInv
{
	public string playerName = "Noob";

	public string islandName = "Dinkum";

	public int money;

	public int hair = 1;

	public int hairColour;

	public int eyeStyle;

	public int eyeColour;

	public int nose;

	public int mouth;

	public int face = -1;

	public int head = -1;

	public int body = -1;

	public int pants = 2;

	public int shoes = -1;

	public int skinTone = 1;

	public int[] itemsInInvSlots = new int[32];

	public int[] stacksInSlots = new int[32];

	public int bankBalance;

	public float stamina = 100f;

	public float staminaMax;

	public int health = 100;

	public int healthMax;

	public bool[] catalogue;

	public long savedTime;
}
