using System.Collections;
using UnityEngine;

public class CutSceneScript : MonoBehaviour
{
	public GameObject canvasToTurnOff;

	public Light sunToTurnOff;

	public NewChunkLoader chunkFeelerToTurnOff;

	public Camera cameraToTurnOff;

	public Animator FletchAnim;

	public Animator PlayerAnim;

	public CustomNetworkManager networkManager;

	private void OnEnable()
	{
		RenderSettings.fogStartDistance = 0f;
		RenderSettings.fogEndDistance = 250f;
		RenderSettings.fogColor = Color.grey;
		canvasToTurnOff.SetActive(false);
		sunToTurnOff.enabled = false;
		chunkFeelerToTurnOff.inside = true;
		cameraToTurnOff.enabled = false;
		MusicManager.manage.startCutsceneMusic();
	}

	public void endCutScene()
	{
		base.gameObject.SetActive(false);
	}

	private void Update()
	{
		if ((Inventory.inv.usingMouse && InputMaster.input.UICancel()) || (!Inventory.inv.usingMouse && InputMaster.input.OpenInventory()))
		{
			base.gameObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		canvasToTurnOff.SetActive(true);
		sunToTurnOff.enabled = true;
		chunkFeelerToTurnOff.inside = false;
		cameraToTurnOff.enabled = true;
		startNewSave();
		NetworkMapSharer.share.fadeToBlack.setBlack();
		CameraController.control.updateDepthOfFieldAndFog(NewChunkLoader.loader.getChunkDistance());
		Object.Destroy(base.gameObject);
	}

	public void startNewSave()
	{
		cameraWonderOnMenu.wonder.enabled = false;
		networkManager.StartUpHost();
	}

	public void fletchWave()
	{
		FletchAnim.SetInteger("Emotion", 4);
		Invoke("stopWave", 1f);
	}

	public void stopWave()
	{
		FletchAnim.SetInteger("Emotion", 0);
	}

	public void startWalking()
	{
		PlayerAnim.SetFloat("WalkSpeed", 1f);
	}

	public void stopWalking()
	{
		StartCoroutine(slowDown());
	}

	public void turnOffPlayer()
	{
		PlayerAnim.gameObject.SetActive(false);
	}

	private IEnumerator slowDown()
	{
		float walkSpeed = 1f;
		while (walkSpeed > 0f)
		{
			walkSpeed -= Time.deltaTime * 3f;
			PlayerAnim.SetFloat("WalkSpeed", walkSpeed);
			yield return null;
		}
		PlayerAnim.SetFloat("WalkSpeed", 0f);
	}
}
