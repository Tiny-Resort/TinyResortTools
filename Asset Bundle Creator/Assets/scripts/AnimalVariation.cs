using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class AnimalVariation : NetworkBehaviour
{
	[SyncVar(hook = "setVariationOnChange")]
	private int variation;

	public string[] variationAdjective;

	public Material[] variations;

	public Mesh[] variationMesh;

	public Vector3[] sizeVariation;

	public SkinnedMeshRenderer render;

	public Sprite[] variationIcons;

	public Transform armatureToScale;

	public int randomVariationLimit = -1;

	public int Networkvariation
	{
		get
		{
			return variation;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref variation))
			{
				int old = variation;
				SetSyncVar(value, ref variation, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					setVariationOnChange(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public override void OnStartClient()
	{
		setVariationOnChange(variation, variation);
	}

	public override void OnStartServer()
	{
		setVariationOnChange(variation, variation);
	}

	private void OnEnable()
	{
		setVariation(variation);
	}

	public void setVariation(int newVariation)
	{
		Networkvariation = Mathf.Clamp(newVariation, 0, variations.Length - 1);
	}

	public int getVaritationNo()
	{
		return variation;
	}

	private void setVariationOnChange(int old, int newVariation)
	{
		Networkvariation = newVariation;
		render.material = variations[variation];
		if (variationMesh.Length != 0)
		{
			render.sharedMesh = variationMesh[variation];
		}
		if (sizeVariation.Length != 0)
		{
			armatureToScale.localScale = sizeVariation[variation];
		}
	}

	public override void OnStopClient()
	{
		if ((bool)armatureToScale)
		{
			armatureToScale.localScale = Vector3.one;
		}
	}

	public void OnDisable()
	{
		if ((bool)armatureToScale)
		{
			armatureToScale.localScale = Vector3.one;
		}
	}

	public int getRandomVariation()
	{
		return Random.Range(0, variations.Length);
	}

	public Sprite getSpriteForVariation()
	{
		return variationIcons[variation];
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(variation);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(variation);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = variation;
			Networkvariation = reader.ReadInt();
			if (!SyncVarEqual(num, ref variation))
			{
				setVariationOnChange(num, variation);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = variation;
			Networkvariation = reader.ReadInt();
			if (!SyncVarEqual(num3, ref variation))
			{
				setVariationOnChange(num3, variation);
			}
		}
	}
}
