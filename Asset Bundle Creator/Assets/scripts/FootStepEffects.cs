using System.Collections;
using UnityEngine;

public class FootStepEffects : MonoBehaviour
{
	private Vector3 oldPosition;

	public LayerMask jumpLayers;

	public LayerMask tileOnlyMask;

	private CharMovement hasCharMovement;

	public Animator hairAnim;

	public Transform leftFoot;

	public Transform rightFoot;

	public ASound wingSound;

	private Vector3 lastPassengerPos = Vector3.zero;

	public float smallStepVolume = 0.5f;

	public float smallStepPitch = 6f;

	private bool hasFeet;

	private WaitForSeconds waterWakeWait = new WaitForSeconds(0.15f);

	public void Start()
	{
		hasCharMovement = GetComponent<CharMovement>();
		tileOnlyMask = (int)tileOnlyMask | (1 << LayerMask.NameToLayer("Tiles"));
		if ((bool)hasCharMovement)
		{
			jumpLayers = hasCharMovement.jumpLayers;
		}
		if ((bool)leftFoot || (bool)rightFoot)
		{
			hasFeet = true;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(checkForWaterWake());
	}

	private IEnumerator checkForWaterWake()
	{
		while (true)
		{
			if (base.transform.position.y <= 0.6f && base.transform.position.y >= -5f)
			{
				int num = (int)(Mathf.Round(base.transform.position.x + 0.5f) / 2f);
				int num2 = (int)(Mathf.Round(base.transform.position.z + 0.5f) / 2f);
				if (num < 0 || num > WorldManager.manageWorld.getMapSize() - 1 || num2 < 0 || num2 > WorldManager.manageWorld.getMapSize() - 1)
				{
					ParticleManager.manage.waterWakePart(base.transform.position, 3);
					yield return waterWakeWait;
				}
				else if (WorldManager.manageWorld.waterMap[num, num2])
				{
					ParticleManager.manage.waterWakePart(base.transform.position, 3);
					yield return waterWakeWait;
				}
			}
			yield return null;
		}
	}

	public void enterDeepWater()
	{
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.bigWaterSplash, base.transform.position);
		ParticleManager.manage.bigSplash(base.transform, 5);
	}

	private void bigSplashDelay()
	{
		ParticleManager.manage.bigSplash(base.transform);
	}

	public void treadWater()
	{
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.treadWater, base.transform.position);
		ParticleManager.manage.waterWakePart(base.transform.position + base.transform.forward + base.transform.right, 3);
		ParticleManager.manage.waterWakePart(base.transform.position + base.transform.forward + -base.transform.right, 3);
	}

	public void jumpNoise()
	{
	}

	public void takeAStep(int foot = -1)
	{
		if (Physics.CheckSphere(base.transform.position, 0.3f, jumpLayers) && Vector3.Distance(oldPosition, base.transform.position) > 0.5f)
		{
			bool flag = true;
			Vector3 position = base.transform.position;
			int num = Mathf.RoundToInt(position.x / 2f);
			int num2 = Mathf.RoundToInt(position.z / 2f);
			if (num < 0 || num > WorldManager.manageWorld.getMapSize() - 1 || num2 < 0 || num2 > WorldManager.manageWorld.getMapSize() - 1)
			{
				return;
			}
			if (base.transform.position.y <= 0.6f && base.transform.position.y >= -5f)
			{
				if (WorldManager.manageWorld.waterMap[num, num2])
				{
					ParticleManager.manage.waterSplash(base.transform.position);
				}
			}
			else if (base.transform.position.y < -6f)
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWood, base.transform.position);
			}
			else if ((bool)hasCharMovement && (bool)hasCharMovement.parentTrans)
			{
				if ((bool)hasCharMovement && Vector3.Distance(lastPassengerPos, hasCharMovement.transform.localPosition) > 0.1f)
				{
					SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWood, base.transform.position);
					lastPassengerPos = hasCharMovement.transform.localPosition;
				}
			}
			else
			{
				RaycastHit hitInfo;
				if (!Physics.CheckSphere(base.transform.position, 0.3f, tileOnlyMask))
				{
					flag = false;
				}
				else if (Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out hitInfo, 0.33f, jumpLayers) && hitInfo.transform.gameObject.layer != LayerMask.NameToLayer("Tiles"))
				{
					flag = false;
				}
				if (hasFeet && flag)
				{
					if (foot == 0)
					{
						ParticleManager.manage.emitFeetDust(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepParticle, leftFoot.position, base.transform.rotation);
					}
					if (foot == 1)
					{
						ParticleManager.manage.emitFeetDust(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepParticle, rightFoot.position, base.transform.rotation);
					}
				}
			}
			makeFootStepNoiseType(position, flag);
		}
		oldPosition = base.transform.position;
	}

	public void flapWing()
	{
		SoundManager.manage.playASoundAtPoint(wingSound, base.transform.position);
	}

	public void makeSmallFootStep()
	{
		if (Vector3.Distance(oldPosition, base.transform.position) < 0.05f)
		{
			return;
		}
		oldPosition = base.transform.position;
		int num = (int)(Mathf.Round(base.transform.position.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(base.transform.position.z + 0.5f) / 2f);
		if (base.transform.position.y < 0.4f && base.transform.position.y >= -5f)
		{
			if (WorldManager.manageWorld.waterMap[num, num2])
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWater, base.transform.position, smallStepVolume, smallStepPitch / 2f);
			}
		}
		else if (base.transform.position.y < -6f || ((bool)hasCharMovement && (bool)hasCharMovement.parentTrans))
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWood, base.transform.position, smallStepVolume, smallStepPitch);
		}
		else if (WorldManager.manageWorld.onTileMap[num, num2] == 15)
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWood, base.transform.position, smallStepVolume, smallStepPitch);
		}
		else if ((bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepSound)
		{
			SoundManager.manage.playASoundAtPoint(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepSound, base.transform.position, smallStepVolume, smallStepPitch);
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepDirt, base.transform.position, smallStepVolume, smallStepPitch);
		}
	}

	public void makeFootStepNoiseType(Vector3 pos, bool isOnATile)
	{
		int num = (int)(Mathf.Round(pos.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(pos.z + 0.5f) / 2f);
		if ((bool)hasCharMovement && (bool)hasCharMovement.parentTrans)
		{
			return;
		}
		if (base.transform.position.y < 0.4f && base.transform.position.y >= -5f)
		{
			if (WorldManager.manageWorld.waterMap[num, num2])
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWater, base.transform.position);
			}
		}
		else if (base.transform.position.y < -6f || ((bool)hasCharMovement && (bool)hasCharMovement.parentTrans))
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWood, base.transform.position);
		}
		else if (WorldManager.manageWorld.onTileMap[num, num2] == 15)
		{
			if ((bool)hasCharMovement && (bool)hasCharMovement.myEquip && hasCharMovement.myEquip.shoeId == -1)
			{
				SoundManager.manage.playASoundAtPointWithPitch(SoundManager.manage.footStepWood, base.transform.position, SoundManager.manage.footStepWood.getPitch() * 2f);
			}
			else
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepWood, base.transform.position);
			}
		}
		else if (isOnATile && (bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepSound)
		{
			if ((bool)hasCharMovement && (bool)hasCharMovement.myEquip && hasCharMovement.myEquip.shoeId == -1)
			{
				SoundManager.manage.playASoundAtPointWithPitch(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepSound, base.transform.position, WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepSound.getPitch() * 2f);
			}
			else
			{
				SoundManager.manage.playASoundAtPoint(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].footStepSound, base.transform.position);
			}
		}
		else if (isOnATile)
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.footStepDirt, base.transform.position);
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.genericFootStep, base.transform.position);
		}
	}
}
