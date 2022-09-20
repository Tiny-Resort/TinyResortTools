using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TileDebug : MonoBehaviour
{
	public static TileDebug debug;

	public Text title;

	public Text tilePos;

	public Text tileType;

	public Text onTileId;

	public Text tileHeight;

	public Text tileStatus;

	public Text bytes;

	public Text chunkChangeText;

	private float deltaTime;

	public GameObject spawnAnimalButton;

	public Transform animalDebugWindow;

	public Transform spawnAnimalButtonsTransform;

	private void Awake()
	{
		debug = this;
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private IEnumerator countBPS()
	{
		yield return new WaitForSeconds(1f);
	}

	private void Update()
	{
		if (!NetworkMapSharer.share.localChar)
		{
			return;
		}
		if (NetworkMapSharer.share.localChar.myInteract.insidePlayerHouse)
		{
			int num = (int)NetworkMapSharer.share.localChar.myInteract.selectedTile.x;
			int num2 = (int)NetworkMapSharer.share.localChar.myInteract.selectedTile.y;
			int num3 = NetworkMapSharer.share.localChar.myInteract.insideHouseDetails.houseMapOnTile[num, num2];
			title.text = "House tile =  | [" + num + "," + num2 + "] =";
			tileType.text = "<color=yellow>On House Tile:</color>" + num3;
			tileHeight.text = "House tile Rotation: " + NetworkMapSharer.share.localChar.myInteract.insideHouseDetails.houseMapRotation[num, num2];
			tilePos.text = "";
			onTileId.text = "";
			tileStatus.text = "";
			return;
		}
		title.text = "Debug | [" + Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position.x / 2f) + "," + Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position.z / 2f) + "] =";
		title.text += WorldManager.manageWorld.onTileMap[Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position.z / 2f)];
		int num4 = (int)NetworkMapSharer.share.localChar.myInteract.selectedTile.x;
		int num5 = (int)NetworkMapSharer.share.localChar.myInteract.selectedTile.y;
		tilePos.text = num4 + "," + num5 + "  |  Biome: " + GenerateMap.generate.checkBiomType(num4, num5);
		tileType.text = "Tile Type [" + WorldManager.manageWorld.tileTypeMap[num4, num5] + "]<color=yellow> " + WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num4, num5]].name + "</color>";
		Text text = tileType;
		text.text = text.text + "    | Under Tile: [" + WorldManager.manageWorld.tileTypeStatusMap[num4, num5] + "] ";
		if (WorldManager.manageWorld.tileTypeStatusMap[num4, num5] >= 0)
		{
			Text text2 = tileType;
			text2.text = text2.text + "<color=yellow>" + WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeStatusMap[num4, num5]].name + "</color>";
		}
		else
		{
			tileType.text += "<color=red>Nothing Under</color>";
		}
		onTileId.text = "Ontile [" + WorldManager.manageWorld.onTileMap[num4, num5] + "] ";
		if (WorldManager.manageWorld.onTileMap[num4, num5] >= 0)
		{
			if (WorldManager.manageWorld.onTileMap[num4, num5] == 500)
			{
				onTileId.text += "<color=red> A request tile from server</color>";
			}
			else
			{
				Text text3 = onTileId;
				text3.text = text3.text + "<color=yellow>" + WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num4, num5]].name + "</color>";
			}
		}
		else
		{
			onTileId.text += "<color=red>Empty Tile</color>";
		}
		tileHeight.text = "Tile Height [" + WorldManager.manageWorld.heightMap[num4, num5] + "]--Rotation: " + WorldManager.manageWorld.rotationMap[num4, num5];
		if (WorldManager.manageWorld.onTileMap[num4, num5] >= 0 && WorldManager.manageWorld.onTileMap[num4, num5] != 500)
		{
			if ((bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num4, num5]].tileObjectItemChanger && WorldManager.manageWorld.onTileStatusMap[num4, num5] >= 0)
			{
				tileStatus.text = "Changing item ID: " + WorldManager.manageWorld.onTileStatusMap[num4, num5] + Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[num4, num5]].itemName;
			}
			else if ((bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num4, num5]].tileObjectGrowthStages)
			{
				tileStatus.text = "Growing: " + WorldManager.manageWorld.onTileStatusMap[num4, num5] + " of " + (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num4, num5]].tileObjectGrowthStages.objectStages.Length - 1);
			}
			else
			{
				tileStatus.text = "Tile Status [" + WorldManager.manageWorld.onTileStatusMap[num4, num5] + "]";
			}
		}
		else
		{
			tileStatus.text = "Tile Status [" + WorldManager.manageWorld.onTileStatusMap[num4, num5] + "]";
		}
		BuriedItem buriedItem = BuriedManager.manage.checkIfBuriedItem(num4, num5);
		if (buriedItem != null)
		{
			Text text4 = tileStatus;
			text4.text = text4.text + "| Burried Map = <color=red>" + Inventory.inv.allItems[buriedItem.itemId].itemName + "</color>";
		}
		else if (WorldManager.manageWorld.onTileMap[num4, num5] == 30)
		{
			tileStatus.text += "| Burried Map = <color=red> Buried item to be generated! </color>";
		}
		else
		{
			tileStatus.text += "| Burried Map = NONE";
		}
		if (WorldManager.manageWorld.fencedOffMap[num4, num5] == 0)
		{
			bytes.text = "Fenced off? : <b><color=red>No.</color></b>";
		}
		else
		{
			bytes.text = "Fenced off? : <b><color=green>YUP! - GroupId = " + WorldManager.manageWorld.fencedOffMap[num4, num5] + " </color></b>";
		}
		int num6 = Mathf.RoundToInt(num4 / 10) * 10;
		int num7 = Mathf.RoundToInt(num5 / 10) * 10;
		if (WorldManager.manageWorld.chunkChangedMap[num6 / 10, num7 / 10])
		{
			chunkChangeText.text = "Changed";
			if (WorldManager.manageWorld.changedMapOnTile[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>On Tile</color>]";
			}
			if (WorldManager.manageWorld.changedMapTileType[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>Type</color>]";
			}
			if (WorldManager.manageWorld.changedMapHeight[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>Height</color>]";
			}
			if (WorldManager.manageWorld.changedMapWater[num6 / 10, num7 / 10])
			{
				chunkChangeText.text += " [<color=green>Water</color>]";
			}
		}
		else
		{
			chunkChangeText.text = "No Change";
		}
	}
}
