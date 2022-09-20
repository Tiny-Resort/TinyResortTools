using UnityEngine;

public class ChristmasLight : MonoBehaviour
{
	public Color[] lightColours;

	public Light lightToChange;

	public Animator myAnim;

	public Renderer myRen;

	private int showingNo;

	private bool lerpEmision;

	private bool lerpToBlack;

	public void onStart()
	{
		myRen = GetComponent<Renderer>();
	}

	public void OnEnable()
	{
		myAnim.SetFloat("Offset", Random.Range(0f, 1f));
		myAnim.SetFloat("Speed", Random.Range(0.85f, 1.25f));
		showingNo = Random.Range(0, lightColours.Length - 1);
		changeToNextColour();
		StartLightEmission();
	}

	public void Update()
	{
		if (!myRen)
		{
			return;
		}
		if (lerpEmision)
		{
			Color value = Color.Lerp(myRen.material.GetColor("_EmissionColor"), lightColours[showingNo], lightToChange.intensity);
			myRen.material.SetColor("_EmissionColor", value);
			if (lightToChange.intensity == 1f)
			{
				lerpEmision = false;
			}
		}
		else if (lerpToBlack)
		{
			Color value = Color.Lerp(myRen.material.GetColor("_EmissionColor"), Color.black, 1f - lightToChange.intensity);
			myRen.material.SetColor("_EmissionColor", value);
			if (1f - lightToChange.intensity == 1f)
			{
				lerpToBlack = false;
			}
		}
	}

	public void changeToNextColour()
	{
		lightToChange.range = Random.Range(1.5f, 3f);
		showingNo++;
		if (showingNo > lightColours.Length - 1)
		{
			showingNo = 0;
		}
		lightToChange.color = lightColours[showingNo];
		StartLightEmission();
	}

	public void StartLightEmission()
	{
		lerpEmision = true;
	}

	public void StartLerpBlack()
	{
		lerpToBlack = true;
	}
}
