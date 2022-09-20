public class Buff
{
	private int timer = 60;

	private int currentLevel = 1;

	public Buff(int seconds, int level)
	{
		timer = seconds;
		currentLevel = level;
	}

	public int getTimeRemaining()
	{
		return timer;
	}

	public int getLevel()
	{
		return currentLevel;
	}

	public bool takeTick()
	{
		timer--;
		return timer < 0;
	}

	public void stackBuff(int newSeconds, int newLevel, bool overrideLevel = false)
	{
		if (newSeconds > timer)
		{
			timer = newSeconds;
		}
		if (overrideLevel || newLevel > currentLevel)
		{
			currentLevel = newLevel;
		}
	}
}
