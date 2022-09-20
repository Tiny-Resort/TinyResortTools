using UnityEngine;

public class MetalDetectorUse : MonoBehaviour
{
	public CharInteract myCharInteract;

	public EquipItemToChar myEquiped;

	public AudioSource metalDetectorSource;

	private float noisePitch = 1f;

	private float noiseVolume = 0.15f;

	private int xPos;

	private int yPos;

	private bool checking;

	private Vector2 lastChecked;

	private float partTimer = 5f;

	public bool foundSomething;

	private float foundTimer;

	public void Start()
	{
		myCharInteract = GetComponentInParent<CharInteract>();
		myEquiped = GetComponentInParent<EquipItemToChar>();
		if ((bool)myCharInteract)
		{
			metalDetectorSource.Play();
		}
		lastChecked = default(Vector2);
	}

	public void checkTileNow()
	{
		if ((bool)myCharInteract)
		{
			checking = true;
			if (!metalDetectorSource.isPlaying)
			{
				metalDetectorSource.Play();
			}
		}
	}

	public Vector3 getLastCheckPositionForNPCFollow()
	{
		return new Vector3(lastChecked.x * 2f, WorldManager.manageWorld.heightMap[(int)lastChecked.x, (int)lastChecked.y], lastChecked.y * 2f);
	}

	private void Update()
	{
		if (!myCharInteract)
		{
			return;
		}
		if (myEquiped.usingItem)
		{
			if (!checking)
			{
				checkTileNow();
			}
		}
		else if (checking)
		{
			stopChecking();
		}
		if (checking)
		{
			Vector3 vector = base.transform.root.position + base.transform.root.forward * 2f;
			xPos = (int)(Mathf.Round(vector.x + 0.5f) / 2f);
			yPos = (int)(Mathf.Round(vector.z + 0.5f) / 2f);
		}
		if ((checking && WorldManager.manageWorld.onTileMap[xPos, yPos] == 30) || (checking && WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 22))
		{
			if ((int)lastChecked.x != xPos || (int)lastChecked.y != yPos)
			{
				partTimer = 1f;
				Inventory.inv.useItemWithFuel();
			}
			noisePitch = Mathf.Lerp(noisePitch, 1f, Time.deltaTime * 5f);
			if (partTimer < 0.1f)
			{
				partTimer += Time.deltaTime * noisePitch;
			}
			else
			{
				partTimer = 0f;
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.metalDetectorFound, new Vector3(xPos * 2, (float)WorldManager.manageWorld.heightMap[xPos, yPos] + 0.1f, yPos * 2), 1);
			}
			noiseVolume = 0.75f;
			if (myCharInteract.isLocalPlayer)
			{
				InputMaster.input.doRumble(0.35f, 10f);
			}
			if (foundTimer <= 0.85f)
			{
				foundTimer += Time.deltaTime;
			}
			else
			{
				foundSomething = true;
			}
		}
		else if (checking && inSurroundingTiles(xPos, yPos))
		{
			foundSomething = false;
			foundTimer = 0f;
			noisePitch = Mathf.Lerp(noisePitch, 0.65f, Time.deltaTime * 5f);
			if (partTimer < 0.1f)
			{
				partTimer += Time.deltaTime * noisePitch;
			}
			else
			{
				partTimer = 0f;
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.metalDetectorClose, new Vector3(xPos * 2, (float)WorldManager.manageWorld.heightMap[xPos, yPos] + 0.1f, yPos * 2), 1);
			}
			noiseVolume = 0.4f;
		}
		else
		{
			foundSomething = false;
			foundTimer = 0f;
			noisePitch = Mathf.Lerp(noisePitch, 0.45f, Time.deltaTime * 5f);
			if (!checking)
			{
				metalDetectorSource.Stop();
			}
			noiseVolume = 0.2f;
		}
		if (checking)
		{
			lastChecked = new Vector2(xPos, yPos);
		}
		metalDetectorSource.pitch = noisePitch;
		metalDetectorSource.volume = Mathf.Lerp(metalDetectorSource.volume, noiseVolume * SoundManager.manage.getSoundVolume(), Time.deltaTime * 10f);
	}

	public void stopChecking()
	{
		checking = false;
		if (myCharInteract.isLocalPlayer)
		{
			InputMaster.input.stopRumble();
		}
	}

	public bool inSurroundingTiles(int xPos, int yPos)
	{
		if (xPos == 0 || yPos == 0 || xPos == WorldManager.manageWorld.getMapSize() - 1 || yPos == WorldManager.manageWorld.getMapSize() - 1)
		{
			return false;
		}
		if (WorldManager.manageWorld.onTileMap[xPos + 1, yPos] == 30)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos - 1, yPos] == 30)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos + 1] == 30)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos - 1] == 30)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos + 1, yPos + 1] == 30)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos + 1, yPos - 1] == 30)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos - 1, yPos + 1] == 30)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos + 1, yPos - 1] == 30)
		{
			return true;
		}
		return false;
	}
}
