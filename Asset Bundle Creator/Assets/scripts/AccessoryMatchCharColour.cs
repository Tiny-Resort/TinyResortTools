using UnityEngine;

public class AccessoryMatchCharColour : MonoBehaviour
{
	public Renderer myRen;

	private void Start()
	{
		EquipItemToChar componentInParent = base.transform.root.GetComponentInParent<EquipItemToChar>();
		if ((bool)componentInParent)
		{
			myRen.material.color = CharacterCreatorScript.create.getHairColour(componentInParent.hairColor);
		}
		else if ((bool)base.transform.root.GetComponentInParent<CharacterCreatorScript>())
		{
			myRen.material.color = CharacterCreatorScript.create.getHairColour(NetworkMapSharer.share.localChar.myEquip.hairColor);
		}
	}
}
