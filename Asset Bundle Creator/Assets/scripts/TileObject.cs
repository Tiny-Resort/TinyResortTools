using System.Collections;
using UnityEngine;

public class TileObject : MonoBehaviour
{
	public int tileObjectId;

	public int xPos;

	public int yPos;

	[Header("Death stuff --------------")]
	public float currentHealth = 100f;

	public Transform[] dropObjectFromPositions;

	[Header("Death Particles --------------")]
	public Transform[] particlePositions;

	[Header("Damage Stuff --------------")]
	public Transform[] damageParticlePositions;

	private bool damageAnimPlaying;

	private bool bounceAnimPlaying;

	[Header("Furniture settings --------------")]
	public Transform[] placedPositions;

	[Header("Connecting scripts--------------")]
	public TileObjectConnect tileObjectConnect;

	public TileObjectBridge tileObjectBridge;

	public ItemDepositAndChanger tileObjectItemChanger;

	public TileObjectGrowthStages tileObjectGrowthStages;

	public FurnitureStatus tileObjectFurniture;

	public DisplayPlayerHouseTiles displayPlayerHouseTiles;

	public ShowObjectOnStatusChange showObjectOnStatusChange;

	public TileObjectAnimalHouse tileObjectAnimalHouse;

	public AnimalDropOffSpot tileObjectAnimalDropOffSpot;

	public SprinklerTile sprinklerTile;

	public OnOffTile tileOnOff;

	public ChestPlaceable tileObjectChest;

	[Header("Connecting scripts Transforms --------------")]
	public Transform loadInsidePos;

	private float shakeX;

	private float shakeY;

	[Header("Local Stuff --------------")]
	public Transform _transform;

	public GameObject _gameObject;

	public Transform AnimDamage;

	public Transform AnimDamage2;

	public bool active = true;

	public bool hasExtensions;

	private ShowObjectOnTop onTop;

	public void setXAndY(int newXPos, int newYPos)
	{
		xPos = newXPos;
		yPos = newYPos;
		checkForAllObjectsOnXYChange();
		if (placedPositions.Length != 0)
		{
			if (onTop == null)
			{
				onTop = base.gameObject.AddComponent<ShowObjectOnTop>();
				onTop.setUp(placedPositions);
			}
			if (ItemOnTopManager.manage.hasItemsOnTop(xPos, yPos))
			{
				onTop.updateItemsOnTopOfMe(ItemOnTopManager.manage.getAllItemsOnTop(xPos, yPos, null));
			}
			else
			{
				onTop.clearObjectsOnTopOfMe();
			}
		}
	}

	public void setXAndYForHouse(int newXPos, int newYPos)
	{
		xPos = newXPos;
		yPos = newYPos;
		checkForAllObjectsOnXYChange();
	}

	private void checkForAllObjectsOnXYChange()
	{
		if ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].tileObjectLoadInside)
		{
			WorldManager.manageWorld.allObjectSettings[tileObjectId].tileObjectLoadInside.checkForInterior(xPos, yPos);
		}
		if ((bool)tileObjectConnect)
		{
			tileObjectConnect.connectToTiles(xPos, yPos);
		}
		if (hasExtensions)
		{
			if ((bool)tileObjectBridge)
			{
				tileObjectBridge.setUpBridge(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]);
			}
			if ((bool)displayPlayerHouseTiles && WorldManager.manageWorld.rotationMap[xPos, yPos] != 0)
			{
				displayPlayerHouseTiles.setInteriorPosAndRotation(xPos, yPos);
			}
			if ((bool)tileObjectGrowthStages)
			{
				tileObjectGrowthStages.setStage(xPos, yPos);
			}
			else if ((bool)tileObjectFurniture)
			{
				tileObjectFurniture.updateOnTileStatus(xPos, yPos);
			}
			else if ((bool)tileObjectItemChanger)
			{
				tileObjectItemChanger.mapUpdatePos(xPos, yPos);
			}
			else if ((bool)tileOnOff)
			{
				tileOnOff.setOnOff(xPos, yPos);
			}
			else if ((bool)showObjectOnStatusChange)
			{
				showObjectOnStatusChange.showGameObject(xPos, yPos);
			}
		}
	}

	public void checkOnTopInside(int insideX, int insideY, HouseDetails details)
	{
		if (placedPositions.Length != 0)
		{
			if (onTop == null)
			{
				onTop = base.gameObject.AddComponent<ShowObjectOnTop>();
				onTop.setUp(placedPositions);
			}
			onTop.updateItemsOnTopOfMe(ItemOnTopManager.manage.getAllItemsOnTop(insideX, insideY, details));
		}
	}

	public bool isAtPos(int checkX, int checkY)
	{
		if (xPos == checkX && yPos == checkY)
		{
			return true;
		}
		return false;
	}

	public bool canBePlaceOn()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].canBePlacedOn();
	}

	public int getTileObjectChangeToOnDeath()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].changeToTileObjectOnDeath;
	}

	public bool getsRotationFromMap()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].getRotationFromMap;
	}

	public bool hasRandomScale()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].hasRandomScale;
	}

	public bool canBePickedUp()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].canBePickedUp;
	}

	public bool isMultiTileObject()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].isMultiTileObject;
	}

	public int getXSize()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].xSize;
	}

	public int getYSize()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].ySize;
	}

	public bool canBePlacedOntoFurniture()
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].canBePlacedOnTopOfFurniture;
	}

	public void checkForAllExtensions()
	{
		if ((bool)showObjectOnStatusChange || (bool)tileObjectItemChanger || (bool)tileObjectGrowthStages || (bool)tileObjectFurniture || (bool)displayPlayerHouseTiles || (bool)tileOnOff || (bool)tileObjectBridge)
		{
			hasExtensions = true;
		}
		else
		{
			hasExtensions = false;
		}
	}

	public bool checkIfHasAnyItemsOnTop(Vector3 housePos, HouseDetails insideHouse, int xPos, int yPos)
	{
		int num = 0;
		Transform[] array = placedPositions;
		foreach (Transform obj in array)
		{
			int num2 = (int)(obj.position.x - housePos.x) / 2;
			int num3 = (int)(obj.position.z - housePos.z) / 2;
		}
		if (num != 0)
		{
			return true;
		}
		return false;
	}

	public Transform findClosestPlacedPosition(Vector3 cursorPos)
	{
		float num = 10f;
		Transform result = null;
		if (placedPositions.Length == 1)
		{
			return placedPositions[0];
		}
		Transform[] array = placedPositions;
		foreach (Transform transform in array)
		{
			float num2 = Vector3.Distance(new Vector3(cursorPos.x, transform.position.y, cursorPos.z), transform.position);
			if (num2 < num)
			{
				num = num2;
				result = transform;
			}
		}
		return result;
	}

	public int returnClosestPlacedPositionId(Vector3 cursorPos)
	{
		if (placedPositions.Length == 1)
		{
			return 0;
		}
		float num = 10f;
		int result = 0;
		for (int i = 0; i < placedPositions.Length; i++)
		{
			float num2 = Vector3.Distance(new Vector3(cursorPos.x, 0f, cursorPos.z), new Vector3(placedPositions[i].position.x, 0f, placedPositions[i].position.z));
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public void addXp()
	{
		if (!WorldManager.manageWorld.allObjectSettings[tileObjectId].canBePickedUp && (!WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath || !WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable || WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable.tileObjectId != tileObjectId))
		{
			if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isStone || WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardStone)
			{
				CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Mining, 1 + Mathf.RoundToInt(WorldManager.manageWorld.allObjectSettings[tileObjectId].fullHealth / 10f));
			}
			else if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isWood || WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardWood)
			{
				CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Foraging, 1 + Mathf.RoundToInt(WorldManager.manageWorld.allObjectSettings[tileObjectId].fullHealth / 10f));
			}
		}
	}

	public int getXpTallyType()
	{
		if ((bool)tileObjectGrowthStages && tileObjectGrowthStages.needsTilledSoil)
		{
			return 0;
		}
		if ((bool)tileObjectGrowthStages && tileObjectGrowthStages.mustBeInWater)
		{
			return 3;
		}
		if ((bool)tileObjectGrowthStages && (bool)tileObjectGrowthStages.harvestDrop && (!tileObjectGrowthStages.harvestDrop.placeable || tileObjectGrowthStages.harvestDrop.placeable.tileObjectId != tileObjectId))
		{
			return 1;
		}
		if (WorldManager.manageWorld.allObjectSettings[tileObjectId].canBePickedUp || ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath && (bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable && WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath.placeable.tileObjectId == tileObjectId))
		{
			return -1;
		}
		if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isStone || WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardStone)
		{
			return 2;
		}
		if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isWood || WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardWood)
		{
			return 1;
		}
		return -1;
	}

	public void damage(bool damageWithSound = true, bool damageParticleOn = true)
	{
		if ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].damageSound && damageWithSound)
		{
			SoundManager.manage.playASoundAtPoint(WorldManager.manageWorld.allObjectSettings[tileObjectId].damageSound, _transform.position);
		}
		if ((bool)AnimDamage)
		{
			int num = Random.Range(-2, 2);
			int num2 = Random.Range(-2, 2);
			while (num == 0)
			{
				num = Random.Range(-2, 2);
			}
			while (num2 == 0)
			{
				num2 = Random.Range(-2, 2);
			}
			shakeX = 1f * (float)num;
			shakeY = 1f * (float)num2;
			if (!damageAnimPlaying && base.gameObject.activeSelf)
			{
				StartCoroutine(damageNoAnim());
			}
			if (!bounceAnimPlaying)
			{
				StartCoroutine(damageBounce());
			}
		}
		if (!damageParticleOn || WorldManager.manageWorld.allObjectSettings[tileObjectId].damageParticle == -1)
		{
			return;
		}
		Transform[] array = damageParticlePositions;
		foreach (Transform transform in array)
		{
			if (transform != null)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.manageWorld.allObjectSettings[tileObjectId].damageParticle], transform.position, WorldManager.manageWorld.allObjectSettings[tileObjectId].damageParticlesPerPosition);
			}
		}
	}

	public void onDeath()
	{
		if (WorldManager.manageWorld.allObjectSettings[tileObjectId].deathParticle != -1)
		{
			if (particlePositions.Length == 0)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.manageWorld.allObjectSettings[tileObjectId].deathParticle], _transform.position, WorldManager.manageWorld.allObjectSettings[tileObjectId].particlesPerPositon);
			}
			else if (!tileObjectGrowthStages || tileObjectGrowthStages.getShowingStage() != -1)
			{
				Transform[] array = particlePositions;
				foreach (Transform transform in array)
				{
					if (transform != null)
					{
						ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[WorldManager.manageWorld.allObjectSettings[tileObjectId].deathParticle], transform.position, WorldManager.manageWorld.allObjectSettings[tileObjectId].particlesPerPositon);
					}
				}
			}
		}
		if ((bool)tileObjectGrowthStages && tileObjectGrowthStages.diesOnHarvest && (bool)tileObjectGrowthStages.harvestSound)
		{
			SoundManager.manage.playASoundAtPoint(tileObjectGrowthStages.harvestSound, base.transform.position);
		}
		else if ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].deathSound)
		{
			SoundManager.manage.playASoundAtPoint(WorldManager.manageWorld.allObjectSettings[tileObjectId].deathSound, _transform.position);
		}
	}

	public void onDeathServer(int xPos, int yPos)
	{
		WorldManager.manageWorld.allObjectSettings[tileObjectId].onDeathServer(xPos, yPos, null, tileObjectGrowthStages, _transform, dropObjectFromPositions);
	}

	public void onDeathInsideServer(int xPos, int yPos, int houseX, int houseY)
	{
		WorldManager.manageWorld.allObjectSettings[tileObjectId].onDeathServer(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY), tileObjectGrowthStages, _transform, dropObjectFromPositions);
	}

	public bool checkIfMultiTileObjectCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].checkIfMultiTileObjectCanBePlaced(startingXPos, startingYPos, rotation);
	}

	public bool checkIfMultiTileObjectCanBePlacedMapGenerate(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].checkIfMultiTileObjectCanBePlacedMapGenerationOnly(startingXPos, startingYPos, rotation);
	}

	public bool checkIfDeedCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].checkIfDeedCanBePlaced(startingXPos, startingYPos, rotation);
	}

	public string getWhyCantPlaceDeedText(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].getWhyCantPlaceDeedText(startingXPos, startingYPos, rotation);
	}

	public bool checkIfBridgeCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].checkIfBridgeCanBePlaced(startingXPos, startingYPos, rotation);
	}

	public bool checkIfMultiTileObjectCanBePlacedUnderGround(int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].checkIfMultiTileObjectCanBePlacedUnderGround(startingXPos, startingYPos, rotation);
	}

	public bool checkIfMultiTileObjectCanBePlacedInside(HouseDetails house, int startingXPos, int startingYPos, int rotation)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].checkIfMultiTileObjectCanBePlacedInside(startingXPos, startingYPos, rotation, house);
	}

	public int[] placeBridgeTiledObject(int startingXPos, int startingYPos, int rotation = 4, int bridgeLength = 2)
	{
		return WorldManager.manageWorld.allObjectSettings[tileObjectId].placeBridgeTiledObject(startingXPos, startingYPos, rotation, bridgeLength);
	}

	public void placeMultiTiledObject(int startingXPos, int startingYPos, int rotation = 4)
	{
		WorldManager.manageWorld.allObjectSettings[tileObjectId].placeMultiTiledObject(startingXPos, startingYPos, rotation);
	}

	public void placeMultiTiledObjectUnderGround(int startingXPos, int startingYPos, int rotation = 4)
	{
		WorldManager.manageWorld.allObjectSettings[tileObjectId].placeMultiTiledObjectUnderGround(startingXPos, startingYPos, rotation);
	}

	public void placeMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails houseDetails)
	{
		WorldManager.manageWorld.allObjectSettings[tileObjectId].placeMultiTiledObjectInside(startingXPos, startingYPos, rotation, houseDetails);
	}

	public void removeMultiTiledObject(int startingXPos, int startingYPos, int rotation)
	{
		WorldManager.manageWorld.allObjectSettings[tileObjectId].removeMultiTiledObject(startingXPos, startingYPos, rotation);
	}

	public void removeMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails houseDetails)
	{
		WorldManager.manageWorld.allObjectSettings[tileObjectId].removeMultiTiledObjectInside(startingXPos, startingYPos, rotation, houseDetails);
	}

	private void OnDisable()
	{
		if ((bool)AnimDamage && bounceAnimPlaying)
		{
			AnimDamage.transform.localScale = Vector3.one;
			bounceAnimPlaying = false;
		}
		if ((damageAnimPlaying && (bool)AnimDamage) || bounceAnimPlaying)
		{
			AnimDamage.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			if ((bool)AnimDamage2)
			{
				AnimDamage2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			damageAnimPlaying = false;
		}
	}

	public void getRotation(int xPos, int yPos)
	{
		if (isMultiTileObject() || getsRotationFromMap())
		{
			if ((bool)tileObjectBridge)
			{
				setMapRotationBridge(xPos, yPos);
			}
			else
			{
				setMapRotation(xPos, yPos);
			}
		}
		else if (WorldManager.manageWorld.allObjectSettings[tileObjectId].hasRandomRotation)
		{
			getRandomRot(xPos, yPos);
		}
	}

	public void getRotationInside(int xPos, int yPos, HouseDetails inHouse = null)
	{
		if (isMultiTileObject() || getsRotationFromMap())
		{
			setMapRotationInside(xPos, yPos, inHouse);
		}
		else if (WorldManager.manageWorld.allObjectSettings[tileObjectId].hasRandomRotation)
		{
			getRandomRot(xPos, yPos);
		}
	}

	public Vector3 setRotatiomNumberForPreviewObject(int mapRot, int length = 5)
	{
		if (isMultiTileObject())
		{
			if ((bool)tileObjectBridge)
			{
				switch (mapRot)
				{
				case 1:
					length--;
					_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					return new Vector3(getXSize() * 2 - 2, 0f, -length * 2);
				case 2:
					length--;
					_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
					return new Vector3(0f, 0f, getXSize() * 2 - 2) - new Vector3(length * 2, 0f, 2f);
				case 3:
					_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					return new Vector3(getXSize() * 2 - 2, 0f, getYSize() * 2 - 2);
				case 4:
					_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
					return new Vector3(getYSize() * 2 - 2, 0f, 0f);
				}
			}
			else
			{
				switch (mapRot)
				{
				case 1:
					_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
					return Vector3.zero;
				case 2:
					_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
					return new Vector3(0f, 0f, getXSize() * 2 - 2);
				case 3:
					_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
					return new Vector3(getXSize() * 2 - 2, 0f, getYSize() * 2 - 2);
				case 4:
					_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
					return new Vector3(getYSize() * 2 - 2, 0f, 0f);
				}
			}
		}
		else
		{
			switch (mapRot)
			{
			case 1:
				_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case 2:
				_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
				break;
			case 3:
				_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 4:
				_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				break;
			}
		}
		return Vector3.zero;
	}

	public void setRotatiomNumber(int mapRot)
	{
		if (isMultiTileObject())
		{
			switch (mapRot)
			{
			case 1:
				_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case 2:
				_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
				_transform.position += new Vector3(0f, 0f, getXSize() * 2 - 2);
				break;
			case 3:
				_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				_transform.position += new Vector3(getXSize() * 2 - 2, 0f, getYSize() * 2 - 2);
				break;
			case 4:
				_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				_transform.position += new Vector3(getYSize() * 2 - 2, 0f, 0f);
				break;
			}
		}
		else
		{
			switch (mapRot)
			{
			case 1:
				_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case 2:
				_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
				break;
			case 3:
				_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 4:
				_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
				break;
			}
		}
	}

	private void setMapRotationInside(int xPos, int yPos, HouseDetails house)
	{
		setRotatiomNumber(house.houseMapRotation[xPos, yPos]);
	}

	private void setMapRotationBridge(int xPos, int yPos)
	{
		_transform.position = new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2);
		if (WorldManager.manageWorld.rotationMap[xPos, yPos] == 1)
		{
			_transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			_transform.position += new Vector3(0f, 0f, WorldManager.manageWorld.onTileStatusMap[xPos, yPos] * 2 - 2);
		}
		else if (WorldManager.manageWorld.rotationMap[xPos, yPos] == 2)
		{
			_transform.rotation = Quaternion.Euler(0f, 90f, 0f);
			_transform.position += new Vector3(WorldManager.manageWorld.onTileStatusMap[xPos, yPos] * 2 - 2, 0f, 2f);
		}
		else if (WorldManager.manageWorld.rotationMap[xPos, yPos] == 3)
		{
			_transform.rotation = Quaternion.Euler(0f, 180f, 0f);
			_transform.position += new Vector3(getXSize() * 2 - 2, 0f, getYSize() * 2 - 2);
		}
		else if (WorldManager.manageWorld.rotationMap[xPos, yPos] == 4)
		{
			_transform.rotation = Quaternion.Euler(0f, 270f, 0f);
			_transform.position += new Vector3(getYSize() * 2 - 2, 0f, 0f);
		}
	}

	private void setMapRotation(int xPos, int yPos)
	{
		setRotatiomNumber(WorldManager.manageWorld.rotationMap[xPos, yPos]);
	}

	public void getRandomRot(int xPos, int yPos)
	{
		Random.InitState(xPos * yPos + xPos - yPos);
		_transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		if (hasRandomScale())
		{
			float num = Random.Range(0.75f, 1.1f);
			_transform.localScale = new Vector3(num, num, num);
		}
	}

	public void placeDown()
	{
		StartCoroutine(AnimateAppear());
	}

	private IEnumerator AnimateAppear()
	{
		if ((bool)AnimDamage)
		{
			float journey = 0f;
			float duration = 0.35f;
			while (journey <= duration)
			{
				journey += Time.deltaTime;
				float time = Mathf.Clamp01(journey / duration);
				float t = UIAnimationManager.manage.windowsOpenCurve.Evaluate(time);
				float num = Mathf.LerpUnclamped(0.25f, 1f, t);
				float y = Mathf.LerpUnclamped(0.1f, 1f, t);
				AnimDamage.transform.localScale = new Vector3(num, y, num);
				yield return null;
			}
		}
	}

	private IEnumerator damageBounce()
	{
		float scaleTimer = 0f;
		bounceAnimPlaying = true;
		float bounceAmount = 0.02f;
		if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isGrass)
		{
			bounceAmount = 0.03f;
		}
		else if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isWood || WorldManager.manageWorld.allObjectSettings[tileObjectId].isStone || WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardWood || WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardWood)
		{
			bounceAmount = 0.01f;
		}
		for (; scaleTimer < bounceAmount; scaleTimer += Time.deltaTime / 4f)
		{
			yield return null;
			AnimDamage.transform.localScale = new Vector3(1f - scaleTimer * 1.1f, 1f - scaleTimer, 1f - scaleTimer * 1.1f);
		}
		while (scaleTimer > 0f)
		{
			yield return null;
			AnimDamage.transform.localScale = new Vector3(1f - scaleTimer * 1.1f, 1f - scaleTimer, 1f - scaleTimer * 1.1f);
			scaleTimer -= Time.deltaTime / 5f;
		}
		for (; scaleTimer < bounceAmount; scaleTimer += Time.deltaTime / 8f)
		{
			yield return null;
			AnimDamage.transform.localScale = new Vector3(1f + scaleTimer * 1.1f, 1f + scaleTimer, 1f + scaleTimer * 1.1f);
		}
		while (scaleTimer > 0f)
		{
			yield return null;
			AnimDamage.transform.localScale = new Vector3(1f + scaleTimer * 1.1f, 1f + scaleTimer, 1f + scaleTimer * 1.1f);
			scaleTimer -= Time.deltaTime / 10f;
		}
		AnimDamage.transform.localScale = Vector3.one;
		bounceAnimPlaying = false;
	}

	private IEnumerator damageNoAnim()
	{
		damageAnimPlaying = true;
		float currentVelocityX = 0f;
		float currentVelocityY = 0f;
		while (damageAnimPlaying)
		{
			AnimDamage.localRotation = Quaternion.Lerp(AnimDamage.localRotation, Quaternion.Euler(shakeX, 0f, shakeY), Time.deltaTime * 50f);
			if ((bool)AnimDamage2)
			{
				AnimDamage2.localRotation = Quaternion.Lerp(AnimDamage2.localRotation, Quaternion.Euler((0f - shakeX) * 2f, 0f, (0f - shakeY) * 2f), Time.deltaTime * 25f);
			}
			if (shakeX == 0f && currentVelocityX == 0f && shakeY == 0f && currentVelocityY == 0f)
			{
				if (AnimDamage.localRotation.x < 0.15f && AnimDamage.localRotation.x > -0.15f && AnimDamage.localRotation.z < 0.15f && AnimDamage.localRotation.z > -0.15f)
				{
					AnimDamage.localRotation = Quaternion.Euler(0f, 0f, 0f);
					if ((bool)AnimDamage2)
					{
						AnimDamage2.localRotation = Quaternion.Euler(0f, 0f, 0f);
					}
					damageAnimPlaying = false;
				}
			}
			else
			{
				calcSpring(shakeX, currentVelocityX, out shakeX, out currentVelocityX);
				calcSpring(shakeY, currentVelocityY, out shakeY, out currentVelocityY);
			}
			yield return null;
		}
	}

	public void calcSpring(float shake, float currentVelocity, out float outShake, out float outVel)
	{
		currentVelocity = currentVelocity * Mathf.Max(0f, 1f - 0.05f * Time.fixedDeltaTime * 50f) + (0f - shake) * 1f * Time.fixedDeltaTime * 50f;
		shake += currentVelocity * Time.fixedDeltaTime;
		if (Mathf.Abs(shake - 0f) < 0.01f && Mathf.Abs(currentVelocity) < 0.01f)
		{
			shake = 0f;
			currentVelocity = 0f;
		}
		outShake = shake;
		outVel = currentVelocity;
	}

	public int getEffectedBuffLevel()
	{
		if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardWood || WorldManager.manageWorld.allObjectSettings[tileObjectId].isWood)
		{
			return StatusManager.manage.getBuffLevel(StatusManager.BuffType.loggingBuff);
		}
		if (WorldManager.manageWorld.allObjectSettings[tileObjectId].isStone || WorldManager.manageWorld.allObjectSettings[tileObjectId].isHardStone)
		{
			return StatusManager.manage.getBuffLevel(StatusManager.BuffType.miningBuff);
		}
		return 0;
	}
}
