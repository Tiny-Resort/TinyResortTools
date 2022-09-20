using UnityEngine;

public class ShopKeeperManager : MonoBehaviour
{
	public static ShopKeeperManager manage;

	public NPCAI myAi;

	private void Awake()
	{
		manage = this;
	}
}
