using System;

[Serializable]
public class Chest
{
	public bool inside;

	public int insideX = -1;

	public int insideY = -1;

	public int xPos;

	public int yPos;

	public int[] itemIds = new int[24];

	public int[] itemStacks = new int[24];

	public int playingLookingInside;

	public Chest(int xPosIn, int yPosIn)
	{
		xPos = xPosIn;
		yPos = yPosIn;
	}
}
