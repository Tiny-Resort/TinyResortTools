using UnityEngine;

public class FishIdentity : MonoBehaviour
{
	public SeasonAndTime mySeason;

	public Sprite pediaImage;

	[TextArea(15, 20)]
	public string pediaDesc;

	public Vector3 fishScale()
	{
		return base.transform.localScale;
	}
}
