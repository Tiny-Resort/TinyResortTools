using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public GameObject jamesLogo;

	public static MusicManager manage;

	public NetworkMapSharer share;

	private static float baseMusicVolume = 0.03f;

	public float musicMasterVolume = 0.03f;

	public AudioSource insideMusic;

	public AudioSource outsideMusic;

	public AudioSource menuMusic;

	public AudioSource dangerMusic;

	public MusicSource m_insideMusic;

	public MusicSource m_outsideMusic;

	public MusicSource m_menuMusic;

	public MusicSource m_dangerMusic;

	public AudioClip menuSong;

	public AudioClip[] dayTimeSongs;

	public AudioClip nightTimeSong;

	public AudioClip rainyDaySong;

	public AudioClip windyDaySong;

	public AudioClip undergroundSong;

	public AudioClip insideSong;

	public AudioClip shopSong;

	public AudioClip comabatSong;

	private bool inside;

	private bool inShop;

	private bool inDanger;

	public LayerMask predators;

	public WaitForSeconds one = new WaitForSeconds(1f);

	public Coroutine musicCoroutineRunning;

	private void Awake()
	{
		manage = this;
		m_insideMusic = new MusicSource(insideMusic);
		m_outsideMusic = new MusicSource(outsideMusic);
		m_menuMusic = new MusicSource(menuMusic);
		m_dangerMusic = new MusicSource(dangerMusic);
		share.onChangeMaps.AddListener(changeLevelMusic);
	}

	private IEnumerator Start()
	{
		changeVolume(musicMasterVolume);
		while (jamesLogo.activeInHierarchy)
		{
			yield return null;
		}
		m_menuMusic.changeMusicClip(menuSong);
		m_menuMusic.play();
		m_dangerMusic.changeMusicClip(comabatSong);
	}

	public void changeLevelMusic()
	{
		m_outsideMusic.changeMusicClip(getOutsideMusic());
	}

	public void openCharacterCreator()
	{
		m_menuMusic.changeMusicClip(insideSong);
		m_menuMusic.play();
	}

	public void closeCharacterCreator()
	{
		m_menuMusic.changeMusicClip(menuSong);
		m_menuMusic.play();
	}

	public void changeVolume(float newVolume)
	{
		musicMasterVolume = newVolume;
		m_menuMusic.updateVolumeToMaster();
		m_outsideMusic.updateVolumeToMaster();
		m_insideMusic.updateVolumeToMaster();
	}

	public void changeFromMenu()
	{
		m_menuMusic.fadeOut();
		if (!TownManager.manage.firstConnect && musicCoroutineRunning == null)
		{
			musicCoroutineRunning = StartCoroutine(musicLoop());
			StartCoroutine(checkForDanger());
		}
	}

	public void startCutsceneMusic()
	{
		m_menuMusic.fadeOut();
	}

	public void changeInside(bool newInside, bool isShop, bool noMusic = false)
	{
		inside = newInside;
		if (inShop != isShop)
		{
			if (isShop)
			{
				m_insideMusic.changeMusicClip(shopSong);
			}
			else
			{
				m_insideMusic.changeMusicClip(insideSong);
			}
			inShop = isShop;
		}
		if (newInside)
		{
			if (!noMusic)
			{
				m_insideMusic.play();
			}
			else
			{
				m_insideMusic.Stop();
			}
		}
	}

	public void enterBoomBoxZone()
	{
		if (inside)
		{
			m_insideMusic.fadeOut();
		}
		else
		{
			m_outsideMusic.fadeOut();
		}
	}

	public void exitBoomBoxZone()
	{
		if (inside)
		{
			m_insideMusic.fadeIn();
		}
		else
		{
			m_outsideMusic.fadeIn();
		}
	}

	private IEnumerator checkForDanger()
	{
		while (true)
		{
			if (NetworkNavMesh.nav.isPlayerInDangerNearCamera())
			{
				inDanger = true;
			}
			else
			{
				inDanger = false;
			}
			yield return one;
		}
	}

	public void stopMusic()
	{
		if (musicCoroutineRunning != null)
		{
			StopCoroutine(musicCoroutineRunning);
			musicCoroutineRunning = null;
		}
		m_outsideMusic.pause(true);
		m_insideMusic.pause(true);
		m_dangerMusic.pause(true);
	}

	public void startMusic()
	{
		if (musicCoroutineRunning == null)
		{
			musicCoroutineRunning = StartCoroutine(musicLoop());
		}
	}

	private IEnumerator musicLoop()
	{
		yield return new WaitForSeconds(1f);
		m_insideMusic.changeMusicClip(insideSong);
		m_outsideMusic.changeMusicClip(getOutsideMusic());
		m_outsideMusic.play();
		m_insideMusic.play();
		m_insideMusic.pause(true);
		m_dangerMusic.play();
		m_dangerMusic.pause(true);
		if (inside)
		{
			m_outsideMusic.pause(true);
			m_insideMusic.fadeIn();
		}
		else
		{
			m_insideMusic.pause(true);
			m_outsideMusic.fadeIn();
		}
		bool lastInside = inside;
		bool lastInDanger = inDanger;
		float lastTimeSincePlayed = 0f;
		int num = Random.Range(240, 360);
		int nextPlayTime = num;
		while (true)
		{
			yield return null;
			if (lastInDanger != inDanger)
			{
				lastInDanger = inDanger;
				if (inDanger)
				{
					if (!inside)
					{
						m_outsideMusic.fadeOut(1f);
					}
					else
					{
						m_insideMusic.fadeOut(1f);
					}
					m_dangerMusic.fadeIn(1f);
				}
				else
				{
					if (!inside)
					{
						m_outsideMusic.fadeIn(2f);
					}
					else
					{
						m_insideMusic.fadeIn(2f);
					}
					m_dangerMusic.fadeOut(1f);
				}
				while (inDanger)
				{
					yield return null;
				}
			}
			if (lastInside != inside)
			{
				lastInside = inside;
				if (lastInside)
				{
					m_outsideMusic.fadeOut();
					m_insideMusic.fadeIn();
				}
				else
				{
					m_outsideMusic.fadeIn();
					m_insideMusic.fadeOut();
				}
			}
			if (!inside && !m_outsideMusic.isPlaying())
			{
				if (lastTimeSincePlayed > (float)nextPlayTime)
				{
					if (RealWorldTimeLight.time.currentHour == 4)
					{
						nextPlayTime = 60;
						lastTimeSincePlayed = 0f;
					}
					else
					{
						m_outsideMusic.changeMusicClip(getOutsideMusic());
						m_outsideMusic.setLocalVolume(0f);
						m_outsideMusic.play();
						m_outsideMusic.fadeIn(15f);
						nextPlayTime = Random.Range(240, 360);
						lastTimeSincePlayed = 0f;
					}
				}
				else
				{
					lastTimeSincePlayed += Time.deltaTime;
				}
			}
			if (!inside && !m_outsideMusic.isPaused && m_outsideMusic.isPlaying())
			{
				m_outsideMusic.checkForLastSecondsFade();
			}
		}
	}

	public IEnumerator fadeOut(MusicSource source, float fadeTime = 3f)
	{
		while (source.getLocalVolume() > 0f)
		{
			yield return null;
			source.adjustLocalVolume((0f - Time.deltaTime) / fadeTime);
		}
		source.setLocalVolume(0f);
		source.pause(true);
		source.runningCoroutine = null;
	}

	public IEnumerator fadeIn(MusicSource source, float fadeTime = 3f)
	{
		source.pause(false);
		while (source.getLocalVolume() < 1f)
		{
			yield return null;
			source.adjustLocalVolume(Time.deltaTime / fadeTime);
		}
		source.setLocalVolume(1f);
		source.runningCoroutine = null;
	}

	public void switchUnderWater(bool newUnderWater)
	{
		if (newUnderWater)
		{
			outsideMusic.pitch = 0.75f;
			insideMusic.pitch = 0.75f;
		}
		else
		{
			outsideMusic.pitch = 1f;
			insideMusic.pitch = 1f;
		}
	}

	public AudioClip getOutsideMusic()
	{
		if (RealWorldTimeLight.time.underGround)
		{
			return undergroundSong;
		}
		if (RealWorldTimeLight.time.currentHour >= 17 || RealWorldTimeLight.time.currentHour == 0)
		{
			return nightTimeSong;
		}
		if (WeatherManager.manage.raining)
		{
			return rainyDaySong;
		}
		if (WeatherManager.manage.windy)
		{
			return windyDaySong;
		}
		return dayTimeSongs[Random.Range(0, dayTimeSongs.Length)];
	}
}
