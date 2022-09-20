using UnityEngine;
using UnityEngine.UI;

public class EyesChangeColourMenu : MonoBehaviour
{
	public RawImage eyeColour;

	private void Start()
	{
		CharacterCreatorScript.create.changeEyeColourEvent.AddListener(onChangeEyes);
	}

	private void onChangeEyes()
	{
		eyeColour.texture = CharacterCreatorScript.create.eyeColours[CharacterCreatorScript.create.eyeColorNo].GetTexture("_MainTex");
	}
}
