using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfDayTally : MonoBehaviour
{
	public TextMeshProUGUI total;

	public Image icon;

	public void setUp(int itemId, int amount)
	{
		total.text = "x " + amount;
		icon.sprite = Inventory.inv.allItems[itemId].getSprite();
	}
}
