using System.Collections;
using UnityEngine;

public class ItemDepositAndChanger : MonoBehaviour
{
	public Transform ejectPos;

	public Animator processAnimator;

	public Transform proccessingItemPrefabPosition;

	private GameObject displayingPrefab;

	public ASound processingSound;

	public AudioSource myAudioSource;

	public int currentXPos;

	public int currentYPos;

	public bool useWindMill;

	private void Start()
	{
		SoundManager.manage.onMasterChange.AddListener(onVolumeChange);
		base.gameObject.AddComponent<InteractableObject>().isItemChanger = this;
	}

	public void mapUpdatePos(int xPos, int yPos, HouseDetails inside = null)
	{
		if ((inside == null && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == -1) || (inside == null && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 0))
		{
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = -2;
		}
		else if ((inside != null && inside.houseMapOnTileStatus[xPos, yPos] == -1) || (inside != null && inside.houseMapOnTileStatus[xPos, yPos] == 0))
		{
			inside.houseMapOnTileStatus[xPos, yPos] = -2;
		}
		if ((bool)processAnimator)
		{
			if ((inside == null && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] != -2) || (inside != null && inside.houseMapOnTileStatus[xPos, yPos] != -2))
			{
				startProcessingSound();
				processAnimator.SetBool("Processing", true);
			}
			else
			{
				stopProcessingSound();
				processAnimator.SetBool("Processing", false);
			}
		}
		if (((inside == null && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] != -2) || (inside != null && inside.houseMapOnTileStatus[xPos, yPos] != -2)) && displayingPrefab == null)
		{
			if (inside == null)
			{
				if ((bool)Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].altDropPrefab)
				{
					displayingPrefab = Object.Instantiate(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
				}
				else
				{
					displayingPrefab = Object.Instantiate(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
				}
			}
			else if ((bool)Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab)
			{
				displayingPrefab = Object.Instantiate(Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
			}
			else
			{
				displayingPrefab = Object.Instantiate(Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
			}
			displayingPrefab.transform.localPosition = Vector3.zero;
			displayingPrefab.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Object.Destroy(displayingPrefab.GetComponent<Animator>());
		}
		currentXPos = xPos;
		currentYPos = yPos;
	}

	public bool getIfProcessing()
	{
		return processAnimator.GetBool("Processing");
	}

	public void ejectItemOnCycle(int xPos, int yPos, HouseDetails inside = null)
	{
		if (inside != null)
		{
			int stackAmount = 1;
			int changerResultId = Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemChange.getChangerResultId(inside.houseMapOnTile[xPos, yPos]);
			if (Inventory.inv.allItems[changerResultId].hasFuel)
			{
				stackAmount = Inventory.inv.allItems[changerResultId].fuelMax;
			}
			NetworkMapSharer.share.spawnAServerDropToSave(changerResultId, stackAmount, ejectPos.position, inside);
			return;
		}
		int stackAmount2 = 1;
		MonoBehaviour.print(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]);
		MonoBehaviour.print(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]]);
		MonoBehaviour.print(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].itemChange);
		MonoBehaviour.print(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].itemChange.getChangerResultId(WorldManager.manageWorld.onTileMap[xPos, yPos]));
		int changerResultId2 = Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].itemChange.getChangerResultId(WorldManager.manageWorld.onTileMap[xPos, yPos]);
		if (Inventory.inv.allItems[changerResultId2].hasFuel)
		{
			stackAmount2 = Inventory.inv.allItems[changerResultId2].fuelMax;
		}
		NetworkMapSharer.share.spawnAServerDropToSave(changerResultId2, stackAmount2, ejectPos.position, inside);
	}

	public void ejectItem(int xPos, int yPos, HouseDetails inside = null)
	{
		if (inside != null)
		{
			NetworkMapSharer.share.spawnAServerDropToSave(Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemChange.getChangerResultId(inside.houseMapOnTile[xPos, yPos]), 1, ejectPos.position, inside);
		}
		else
		{
			NetworkMapSharer.share.spawnAServerDropToSave(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].itemChange.getChangerResultId(WorldManager.manageWorld.onTileMap[xPos, yPos]), 1, ejectPos.position, inside);
		}
		if ((bool)processAnimator)
		{
			processAnimator.SetBool("Processing", false);
		}
		if (displayingPrefab != null)
		{
			Object.Destroy(displayingPrefab);
		}
		if (inside != null)
		{
			inside.houseMapOnTileStatus[xPos, yPos] = -2;
		}
		else
		{
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = -2;
		}
	}

	public void depositItem(InventoryItem placeIn, int xPos, int yPos, HouseDetails inside = null)
	{
	}

	public void playLocalDeposit(int xPos, int yPos, HouseDetails inside = null)
	{
		if ((bool)processAnimator)
		{
			startProcessingSound();
			processAnimator.SetBool("Processing", true);
			processAnimator.SetTrigger("PlaceInTo");
		}
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.placeItemInChanger, base.transform.position);
		if (((inside != null || WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == -2) && (inside == null || inside.houseMapOnTileStatus[xPos, yPos] == -2)) || !(displayingPrefab == null))
		{
			return;
		}
		if (inside == null)
		{
			if ((bool)Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].altDropPrefab)
			{
				displayingPrefab = Object.Instantiate(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
			}
			else
			{
				displayingPrefab = Object.Instantiate(Inventory.inv.allItems[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
			}
		}
		else if ((bool)Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab)
		{
			displayingPrefab = Object.Instantiate(Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].altDropPrefab, proccessingItemPrefabPosition);
		}
		else
		{
			displayingPrefab = Object.Instantiate(Inventory.inv.allItems[inside.houseMapOnTileStatus[xPos, yPos]].itemPrefab, proccessingItemPrefabPosition);
		}
		displayingPrefab.transform.localPosition = Vector3.zero;
		displayingPrefab.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Object.Destroy(displayingPrefab.GetComponent<Animator>());
	}

	public void stopLocalProcessing()
	{
		if ((bool)processAnimator)
		{
			stopProcessingSound();
			processAnimator.SetBool("Processing", false);
		}
		if (displayingPrefab != null)
		{
			Object.Destroy(displayingPrefab);
		}
	}

	public void OnDisable()
	{
		Object.Destroy(displayingPrefab);
	}

	public int returnAmountNeeded(InventoryItem itemToCheck)
	{
		if ((bool)itemToCheck.itemChange)
		{
			return itemToCheck.itemChange.getAmountNeeded(GetComponent<TileObject>().tileObjectId);
		}
		return 0;
	}

	public bool canDepositThisItem(InventoryItem canDeposit, HouseDetails inside = null, int xPos = -2, int yPos = -2)
	{
		if (xPos == -2 && yPos == -2)
		{
			xPos = currentXPos;
			yPos = currentYPos;
		}
		if (inside == null && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] != -2)
		{
			return false;
		}
		if (inside != null && inside.houseMapOnTileStatus[xPos, yPos] != -2)
		{
			return false;
		}
		if ((bool)canDeposit.itemChange)
		{
			return canDeposit.itemChange.checkIfCanBeDeposited(GetComponent<TileObject>().tileObjectId);
		}
		return false;
	}

	public void startProcessingSound()
	{
		StopCoroutine("stopSound");
		if ((bool)myAudioSource)
		{
			myAudioSource.Stop();
			if (!myAudioSource.isPlaying)
			{
				myAudioSource.clip = processingSound.getSound();
				myAudioSource.pitch = processingSound.getPitch() * 4f;
				myAudioSource.volume = 0f;
				myAudioSource.Play();
				StartCoroutine("startSound");
			}
		}
	}

	public void stopProcessingSound()
	{
		StopCoroutine("startSound");
		if ((bool)myAudioSource && myAudioSource.isPlaying)
		{
			StartCoroutine("stopSound");
		}
	}

	private IEnumerator startSound()
	{
		while (myAudioSource.volume < processingSound.volume * SoundManager.manage.getSoundVolume())
		{
			yield return null;
			myAudioSource.volume += 0.01f;
			myAudioSource.pitch = Mathf.Lerp(myAudioSource.pitch, processingSound.getPitch(), myAudioSource.volume / (processingSound.volume * SoundManager.manage.getSoundVolume()));
		}
		myAudioSource.volume = processingSound.volume * SoundManager.manage.getSoundVolume();
		myAudioSource.pitch = processingSound.getPitch();
	}

	private IEnumerator stopSound()
	{
		while (myAudioSource.volume > 0f)
		{
			yield return null;
			myAudioSource.volume -= 0.01f;
			myAudioSource.pitch += 0.01f;
		}
		myAudioSource.volume = 0f;
		myAudioSource.Stop();
	}

	private void onVolumeChange()
	{
		if ((bool)myAudioSource)
		{
			myAudioSource.volume = processingSound.volume * SoundManager.manage.getSoundVolume();
		}
	}
}
