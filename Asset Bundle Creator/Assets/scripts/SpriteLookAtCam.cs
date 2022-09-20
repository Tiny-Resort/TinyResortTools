using System.Collections;
using UnityEngine;

public class SpriteLookAtCam : MonoBehaviour
{
	public SpriteRenderer spriteRen;

	public bool isBluePrint;

	public SpriteRenderer background;

	public GameObject bluePrintGameObject;

	public Transform blueprintSpriteSpot;

	private Vector3 startingPos;

	private Vector3 floatingPos;

	private bool floatingUp;

	public static WaitForSeconds distanceCheck = new WaitForSeconds(0.1f);

	private float spriteViewDistance = 8f;

	private float bounceSpeed;

	private void OnEnable()
	{
		if (!isBluePrint)
		{
			startingPos = spriteRen.transform.localPosition;
			floatingPos = new Vector3(0f, 0f, Random.Range(-0.5f, 0.5f));
			floatingUp = Random.Range(0, 2) == 1;
			StartCoroutine(lookAtCam());
		}
	}

	public void changeSprite(int itemNo)
	{
		if (isBluePrint)
		{
			spriteRen.sprite = Inventory.inv.allItems[itemNo].getSprite();
			if (spriteRen.sprite.rect.height > 64f)
			{
				base.transform.localScale = new Vector3(1f, 1f, 1f);
			}
		}
	}

	private IEnumerator lookAtCam()
	{
		Color white = Color.white;
		white.a = 0f;
		spriteRen.color = white;
		background.color = white;
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) <= spriteViewDistance)
		{
			yield return StartCoroutine(spriteFadeIn());
		}
		while (true)
		{
			if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) > spriteViewDistance)
			{
				yield return StartCoroutine(spriteFadeOut());
				spriteRen.enabled = false;
				while (Vector3.Distance(CameraController.control.transform.position, base.transform.position) > spriteViewDistance)
				{
					yield return distanceCheck;
				}
				yield return StartCoroutine(spriteFadeIn());
			}
			else
			{
				spriteFaceCamAndAdjust();
				yield return null;
			}
		}
	}

	public void spriteFaceCamAndAdjust()
	{
		spriteRen.enabled = true;
		base.transform.LookAt(CameraController.control.cameraTrans);
		spriteRen.transform.localPosition = startingPos + floatingPos;
		if (floatingUp)
		{
			floatingPos = Vector3.Lerp(new Vector3(0f, 0f, -0.15f), new Vector3(0f, 0f, 0.15f), bounceSpeed);
			bounceSpeed += Time.deltaTime / 6f;
			if (floatingPos.z >= 0.15f)
			{
				bounceSpeed = 0f;
				floatingUp = !floatingUp;
			}
		}
		else if (!floatingUp)
		{
			floatingPos = Vector3.Lerp(new Vector3(0f, 0f, 0.15f), new Vector3(0f, 0f, -0.15f), bounceSpeed);
			bounceSpeed += Time.deltaTime / 6f;
			if (floatingPos.z <= -0.15f)
			{
				bounceSpeed = 0f;
				floatingUp = !floatingUp;
			}
		}
	}

	private IEnumerator spriteFadeIn()
	{
		float fadeTimer = 0f;
		Color fadeFrom = Color.white;
		fadeFrom.a = 0f;
		while (fadeTimer <= 1f)
		{
			yield return null;
			fadeTimer += Time.deltaTime * 3f;
			spriteRen.color = Color.Lerp(fadeFrom, Color.white, fadeTimer);
			background.color = spriteRen.color;
			spriteFaceCamAndAdjust();
		}
	}

	private IEnumerator spriteFadeOut()
	{
		float fadeTimer = 0f;
		Color fadeTo = Color.white;
		fadeTo.a = 0f;
		while (fadeTimer <= 1f)
		{
			yield return null;
			fadeTimer += Time.deltaTime * 3f;
			spriteRen.color = Color.Lerp(Color.white, fadeTo, fadeTimer);
			background.color = spriteRen.color;
			spriteFaceCamAndAdjust();
		}
	}

	public void setAsBluePrint()
	{
		bluePrintGameObject.SetActive(true);
		spriteRen.transform.parent = blueprintSpriteSpot;
		spriteRen.transform.localPosition = Vector3.zero;
		spriteRen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		spriteRen.color = Color.Lerp(Color.white, Color.clear, 0.5f);
		isBluePrint = true;
	}
}
