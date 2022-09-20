using System;

[Serializable]
public class CurrentChanger
{
	public int xPos;

	public int yPos;

	public int counterSeconds = 1;

	public int counterDays;

	public int timePerCycles = 1;

	public int houseX = -1;

	public int houseY = -1;

	public int cycles = 1;

	public bool startedUnderground;

	public CurrentChanger(int tileX, int tileY)
	{
		xPos = tileX;
		yPos = tileY;
		startedUnderground = RealWorldTimeLight.time.underGround;
	}

	public CurrentChanger()
	{
	}
}
