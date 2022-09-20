using UnityEngine;

public class HuntingChallengeManager : MonoBehaviour
{
	public static HuntingChallengeManager manage;

	private void Awake()
	{
		manage = this;
	}

	public HuntingChallenge createNewChallengeAndAttachToPost()
	{
		switch (Random.Range(0, 3))
		{
		case 0:
			return new HuntingChallenge(2, 25);
		case 1:
			return new HuntingChallenge(7, 24);
		default:
			return new HuntingChallenge(6, 26);
		}
	}
}
