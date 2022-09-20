using UnityEngine;

public class WorkTable : MonoBehaviour
{
	public string workTableName = "Table";

	public CraftingManager.CraftingMenuType typeOfCrafting;

	public ReadableSign tableText;

	public InventoryItem itemNeeded;

	public Conversation noItemText;

	public Conversation hasItemText;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isWorkTable = this;
	}

	public void checkForItemAndChangeText()
	{
		if (Inventory.inv.getAmountOfItemInAllSlots(itemNeeded.getItemId()) > 0)
		{
			tableText.signSays = hasItemText;
		}
		else
		{
			tableText.signSays = noItemText;
		}
	}
}
