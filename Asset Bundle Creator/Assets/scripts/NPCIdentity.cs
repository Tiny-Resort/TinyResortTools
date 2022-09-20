using System.Runtime.InteropServices;
using Mirror;
using UnityChan;
using UnityEngine;

public class NPCIdentity : NetworkBehaviour
{
	[SyncVar]
	public int NPCNo;

	public Transform onHeadPos;

	public EyesScript eyes;

	public MeshFilter noseMesh;

	public MeshRenderer[] eyeRenderers;

	public SkinnedMeshRenderer playerSkin;

	public SkinnedMeshRenderer shirtRen;

	public SkinnedMeshRenderer pantsRen;

	public SkinnedMeshRenderer shoeRen;

	public RuntimeAnimatorController defaultAnim;

	private GameObject onHead;

	public int NetworkNPCNo
	{
		get
		{
			return NPCNo;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref NPCNo))
			{
				int nPCNo = NPCNo;
				SetSyncVar(value, ref NPCNo, 1uL);
			}
		}
	}

	private void Start()
	{
	}

	public override void OnStartClient()
	{
		changeNPCAndEquip(NPCNo);
	}

	public override void OnStartServer()
	{
		changeNPCAndEquip(NPCNo);
		if (NPCNo > 0)
		{
			GetComponent<NPCAI>().myManager = NPCManager.manage.getNPCMapAgentForNPC(NPCNo);
			GetComponent<NPCAI>().myAgent.avoidancePriority = NPCNo;
		}
	}

	public void changeNPCAndEquip(int newId)
	{
		if (newId == -1)
		{
			return;
		}
		NetworkNPCNo = newId;
		if (!NetworkMapSharer.share.isServer && NPCManager.manage.NPCDetails[newId].isAVillager && !NPCManager.manage.npcInvs[newId].hasBeenRequested)
		{
			return;
		}
		if (NPCManager.manage.NPCDetails[NPCNo].isAVillager)
		{
			NPCManager.manage.NPCDetails[NPCNo].getName(NPCManager.manage.npcInvs[NPCNo].nameId, NPCManager.manage.npcInvs[NPCNo].isFem);
		}
		if ((bool)NPCManager.manage.NPCDetails[NPCNo].npcMesh)
		{
			playerSkin.sharedMesh = NPCManager.manage.NPCDetails[NPCNo].npcMesh;
		}
		else
		{
			playerSkin.sharedMesh = NPCManager.manage.defaultNpcMesh;
		}
		GetComponent<NPCJob>().NetworkvendorId = (int)NPCManager.manage.NPCDetails[newId].workLocation;
		if ((bool)NPCManager.manage.NPCDetails[newId].animationOverrride)
		{
			if (defaultAnim == null)
			{
				defaultAnim = GetComponent<Animator>().runtimeAnimatorController;
			}
			GetComponent<Animator>().runtimeAnimatorController = NPCManager.manage.NPCDetails[newId].animationOverrride;
		}
		else if (defaultAnim != null)
		{
			GetComponent<Animator>().runtimeAnimatorController = defaultAnim;
		}
		if (newId != 5 && (bool)GetComponent<AudioEchoFilter>())
		{
			Object.Destroy(GetComponent<AudioEchoFilter>());
		}
		else if (newId == 5 && !GetComponent<AudioEchoFilter>())
		{
			AudioEchoFilter audioEchoFilter = base.gameObject.AddComponent<AudioEchoFilter>();
			audioEchoFilter.delay = 10f;
			audioEchoFilter.decayRatio = 0.001f;
			audioEchoFilter.dryMix = 1f;
			audioEchoFilter.wetMix = 0.75f;
		}
		setUpNPCHead();
		setUpNPCMaterials();
	}

	private void setUpNPCHead()
	{
		if (NPCManager.manage.NPCDetails[NPCNo].isAVillager)
		{
			int hairId = NPCManager.manage.npcInvs[NPCNo].hairId;
			GameObject gameObject = CharacterCreatorScript.create.allHairStyles[hairId];
			if (onHead != gameObject)
			{
				Object.Destroy(onHead);
				onHead = Object.Instantiate(gameObject, onHeadPos);
				onHead.transform.localPosition = Vector3.zero;
				onHead.layer = LayerMask.NameToLayer("NPC");
				Color hairColour = CharacterCreatorScript.create.getHairColour(NPCManager.manage.npcInvs[NPCNo].hairColorId);
				MeshRenderer[] componentsInChildren = onHead.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material.color = hairColour;
				}
				SkinnedMeshRenderer[] componentsInChildren2 = onHead.GetComponentsInChildren<SkinnedMeshRenderer>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].material.color = hairColour;
				}
				Transform[] componentsInChildren3 = onHead.GetComponentsInChildren<Transform>();
				for (int i = 0; i < componentsInChildren3.Length; i++)
				{
					componentsInChildren3[i].gameObject.layer = LayerMask.NameToLayer("NPC");
				}
			}
			eyes.setMouthMesh(null);
			eyes.changeMouthMat(CharacterCreatorScript.create.mouthTypes[NPCManager.manage.npcInvs[NPCNo].mouthId], CharacterCreatorScript.create.skinTones[NPCManager.manage.npcInvs[NPCNo].skinId].color);
			noseMesh.mesh = CharacterCreatorScript.create.noseMeshes[NPCManager.manage.npcInvs[NPCNo].noseId];
			return;
		}
		if (onHead != NPCManager.manage.NPCDetails[NPCNo].NpcHair)
		{
			Object.Destroy(onHead);
			onHead = Object.Instantiate(NPCManager.manage.NPCDetails[NPCNo].NpcHair, onHeadPos);
			SpringManager componentInChildren = onHead.GetComponentInChildren<SpringManager>();
			if ((bool)componentInChildren)
			{
				GetComponent<NPCAI>().hairSpring = componentInChildren;
			}
			onHead.transform.localPosition = Vector3.zero;
			onHead.layer = LayerMask.NameToLayer("NPC");
			Transform[] componentsInChildren3 = onHead.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].gameObject.layer = LayerMask.NameToLayer("NPC");
			}
			eyes.faceMove = onHead.GetComponentInChildren<FaceItemMoveWithMouth>();
		}
		noseMesh.mesh = CharacterCreatorScript.create.noseMeshes[NPCManager.manage.NPCDetails[NPCNo].nose];
		eyes.setMouthMesh(NPCManager.manage.NPCDetails[NPCNo].insideMouthMesh);
	}

	private void setUpNPCMaterials()
	{
		if (NPCManager.manage.NPCDetails[NPCNo].isAVillager)
		{
			playerSkin.material = CharacterCreatorScript.create.skinTones[NPCManager.manage.npcInvs[NPCNo].skinId];
			int eyesId = NPCManager.manage.npcInvs[NPCNo].eyesId;
			int eyeColorId = NPCManager.manage.npcInvs[NPCNo].eyeColorId;
			eyes.changeEyeMat(CharacterCreatorScript.create.allEyeTypes[eyesId], playerSkin.material.color);
			eyes.changeEyeColor(CharacterCreatorScript.create.eyeColours[eyeColorId]);
			InventoryItem inventoryItem = Inventory.inv.allItems[NPCManager.manage.npcInvs[NPCNo].shirtId];
			if ((bool)inventoryItem.equipable.shirtMesh)
			{
				shirtRen.sharedMesh = inventoryItem.equipable.shirtMesh;
			}
			else
			{
				shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
			}
			InventoryItem inventoryItem2 = Inventory.inv.allItems[NPCManager.manage.npcInvs[NPCNo].pantsId];
			if ((bool)inventoryItem2 && (bool)inventoryItem2.equipable.useAltMesh)
			{
				pantsRen.sharedMesh = inventoryItem2.equipable.useAltMesh;
			}
			else
			{
				pantsRen.sharedMesh = EquipWindow.equip.defaultPants;
			}
			InventoryItem inventoryItem3 = Inventory.inv.allItems[NPCManager.manage.npcInvs[NPCNo].shoesId];
			if ((bool)inventoryItem3 && (bool)inventoryItem3.equipable.useAltMesh)
			{
				shoeRen.sharedMesh = inventoryItem3.equipable.useAltMesh;
			}
			else
			{
				shoeRen.sharedMesh = EquipWindow.equip.defualtShoeMesh;
			}
			equipRenderer(inventoryItem, shirtRen);
			equipRenderer(inventoryItem2, pantsRen);
			equipRenderer(inventoryItem3, shoeRen);
		}
		else
		{
			playerSkin.material = NPCManager.manage.NPCDetails[NPCNo].NpcSkin;
			eyes.changeEyeMat(NPCManager.manage.NPCDetails[NPCNo].NpcEyes, playerSkin.material.color);
			eyes.changeMouthMat(NPCManager.manage.NPCDetails[NPCNo].NPCMouth, playerSkin.material.color);
			eyes.changeEyeColor(NPCManager.manage.NPCDetails[NPCNo].NpcEyesColor);
			if ((bool)NPCManager.manage.NPCDetails[NPCNo].NPCShirt.equipable.shirtMesh)
			{
				shirtRen.sharedMesh = NPCManager.manage.NPCDetails[NPCNo].NPCShirt.equipable.shirtMesh;
			}
			else
			{
				shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
			}
			if ((bool)NPCManager.manage.NPCDetails[NPCNo].NPCPants.equipable.useAltMesh)
			{
				pantsRen.sharedMesh = NPCManager.manage.NPCDetails[NPCNo].NPCPants.equipable.useAltMesh;
			}
			else
			{
				pantsRen.sharedMesh = EquipWindow.equip.defaultPants;
			}
			InventoryItem nPCShoes = NPCManager.manage.NPCDetails[NPCNo].NPCShoes;
			if ((bool)nPCShoes && (bool)nPCShoes.equipable.useAltMesh)
			{
				shoeRen.sharedMesh = nPCShoes.equipable.useAltMesh;
			}
			else
			{
				shoeRen.sharedMesh = EquipWindow.equip.defualtShoeMesh;
			}
			equipRenderer(NPCManager.manage.NPCDetails[NPCNo].NPCShirt, shirtRen);
			equipRenderer(NPCManager.manage.NPCDetails[NPCNo].NPCPants, pantsRen);
			equipRenderer(NPCManager.manage.NPCDetails[NPCNo].NPCShoes, shoeRen);
		}
	}

	private void equipRenderer(InventoryItem invClothItem, SkinnedMeshRenderer render)
	{
		if (invClothItem != null)
		{
			render.gameObject.SetActive(true);
			render.material = invClothItem.equipable.material;
		}
		else
		{
			render.gameObject.SetActive(false);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(NPCNo);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(NPCNo);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int nPCNo = NPCNo;
			NetworkNPCNo = reader.ReadInt();
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			int nPCNo2 = NPCNo;
			NetworkNPCNo = reader.ReadInt();
		}
	}
}
