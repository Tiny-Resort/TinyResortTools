using System;

[Serializable]
public class NetStall
{
	public ShopBuyDrop connectedStall;

	public bool hasBeenSold;

	public void sellIfConnected()
	{
		if ((bool)connectedStall)
		{
			connectedStall.sold();
		}
	}
}
