using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineEnterExit : MonoBehaviour
{
	public bool isEntrance = true;

	public static MineEnterExit mineEntrance;

	public static MineEnterExit mineExit;

	public List<CharMovement> charsInside;

	public AudioSource elevatorSounds;

	public AudioClip elevatorStarts;

	public AudioClip elevatorStops;

	public Transform position;

	public Transform[] allMinePositions;

	public Animator doorAnim;

	public GameObject exitToLock;

	private static bool doorsClosed;

	public GameObject lightsAnim;

	public AudioSource doorSound;

	public AudioClip doorOpen;

	public AudioClip doorClose;

	public void Start()
	{
		if (isEntrance)
		{
			mineEntrance = this;
		}
		else
		{
			mineExit = this;
		}
	}

	public void OnEnable()
	{
		if (isEntrance)
		{
			mineEntrance = this;
		}
		else
		{
			mineExit = this;
		}
		if (doorsClosed)
		{
			exitToLock.gameObject.SetActive(false);
		}
		else
		{
			Invoke("openDoorOnEnable", 0.01f);
		}
	}

	public void openDoorOnDeath()
	{
		doorsClosed = false;
		exitToLock.gameObject.SetActive(true);
	}

	public void openDoorOnEnable()
	{
		doorAnim.SetTrigger("Open");
	}

	public void closeDoors()
	{
		doorsClosed = true;
		exitToLock.SetActive(false);
		doorSound.PlayOneShot(doorClose);
		doorAnim.SetTrigger("Close");
	}

	public void startElevatorTimer()
	{
		StartCoroutine(elevatorTimer());
	}

	public IEnumerator elevatorTimer()
	{
		float timer = 0f;
		CameraController.control.shakeScreenMax(0.45f, 0.45f);
		elevatorSounds.PlayOneShot(elevatorStarts);
		lightsAnim.SetActive(true);
		for (; timer < 5.5f; timer += Time.deltaTime)
		{
			CameraController.control.shakeScreenMax(0.1f, 0.1f);
			yield return null;
		}
		elevatorSounds.Stop();
		CameraController.control.shakeScreenMax(0.45f, 0.45f);
		elevatorSounds.PlayOneShot(elevatorStops);
		lightsAnim.SetActive(false);
		doorsClosed = false;
		yield return new WaitForSeconds(1f);
		while (!WorldManager.manageWorld.chunkRefreshCompleted)
		{
			yield return null;
		}
		exitToLock.gameObject.SetActive(true);
		doorSound.PlayOneShot(doorOpen);
		doorAnim.SetTrigger("Open");
		yield return new WaitForSeconds(1f);
		NetworkMapSharer.share.canUseMineControls = true;
	}

	private void OnDisable()
	{
		if (isEntrance)
		{
			mineEntrance.charsInside.Clear();
		}
		else
		{
			mineExit.charsInside.Clear();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return;
		}
		CharMovement component = other.transform.GetComponent<CharMovement>();
		if (!component)
		{
			return;
		}
		if (isEntrance)
		{
			if (!mineEntrance.charsInside.Contains(component))
			{
				mineEntrance.charsInside.Add(component);
			}
		}
		else if (!mineExit.charsInside.Contains(component))
		{
			mineExit.charsInside.Add(component);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return;
		}
		CharMovement component = other.transform.GetComponent<CharMovement>();
		if ((bool)component)
		{
			if (isEntrance)
			{
				mineEntrance.charsInside.Remove(component);
			}
			else
			{
				mineExit.charsInside.Remove(component);
			}
		}
	}
}
