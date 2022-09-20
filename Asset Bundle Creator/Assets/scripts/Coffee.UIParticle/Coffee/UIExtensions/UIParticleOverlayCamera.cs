using UnityEngine;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class UIParticleOverlayCamera : MonoBehaviour
	{
		private Camera m_Camera;

		private static UIParticleOverlayCamera s_Instance;

		public static UIParticleOverlayCamera instance
		{
			get
			{
				if ((object)s_Instance == null)
				{
					s_Instance = Object.FindObjectOfType<UIParticleOverlayCamera>() ?? new GameObject(typeof(UIParticleOverlayCamera).Name, typeof(UIParticleOverlayCamera)).GetComponent<UIParticleOverlayCamera>();
					s_Instance.gameObject.SetActive(true);
					s_Instance.enabled = true;
				}
				return s_Instance;
			}
		}

		private Camera cameraForOvrelay
		{
			get
			{
				if (!m_Camera)
				{
					if (!(m_Camera = GetComponent<Camera>()))
					{
						return m_Camera = base.gameObject.AddComponent<Camera>();
					}
					return m_Camera;
				}
				return m_Camera;
			}
		}

		public static Camera GetCameraForOvrelay(Canvas canvas)
		{
			UIParticleOverlayCamera uIParticleOverlayCamera = instance;
			RectTransform rectTransform = canvas.rootCanvas.transform as RectTransform;
			Camera camera = uIParticleOverlayCamera.cameraForOvrelay;
			Transform obj = uIParticleOverlayCamera.transform;
			camera.enabled = false;
			Vector3 localPosition = rectTransform.localPosition;
			camera.orthographic = true;
			camera.orthographicSize = Mathf.Max(localPosition.x, localPosition.y);
			camera.nearClipPlane = 0.3f;
			camera.farClipPlane = 1000f;
			localPosition.z -= 100f;
			obj.localPosition = localPosition;
			return camera;
		}

		private void Awake()
		{
			if (s_Instance == null)
			{
				s_Instance = GetComponent<UIParticleOverlayCamera>();
			}
			else if (s_Instance != this)
			{
				Debug.LogWarning("Multiple " + typeof(UIParticleOverlayCamera).Name + " in scene.", base.gameObject);
				base.enabled = false;
				Object.Destroy(base.gameObject);
				return;
			}
			cameraForOvrelay.enabled = false;
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		private void OnEnable()
		{
			base.gameObject.hideFlags = HideFlags.HideAndDontSave;
			base.gameObject.tag = "EditorOnly";
		}

		private void OnDestroy()
		{
			if (s_Instance == this)
			{
				s_Instance = null;
			}
		}
	}
}
