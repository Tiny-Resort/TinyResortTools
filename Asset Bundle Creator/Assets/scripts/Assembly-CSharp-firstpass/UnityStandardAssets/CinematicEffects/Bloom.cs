using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Cinematic/Bloom")]
	public class Bloom : MonoBehaviour
	{
		[Serializable]
		public struct Settings
		{
			[SerializeField]
			[Range(0f, 1f)]
			[Tooltip("Filters out pixels under this level of brightness.")]
			public float threshold;

			[SerializeField]
			[Range(0f, 1f)]
			[Tooltip("Sensitivity of the effect\n(0=less sensitive, 1=fully sensitive).")]
			public float exposure;

			[SerializeField]
			[Range(0f, 5f)]
			[Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
			public float radius;

			[SerializeField]
			[Range(0f, 2f)]
			[Tooltip("Blend factor of the result image.")]
			public float intensity;

			[SerializeField]
			[Tooltip("Controls filter quality and buffer resolution.")]
			public bool highQuality;

			[SerializeField]
			[Tooltip("Reduces flashing noise with an additional filter.")]
			public bool antiFlicker;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.threshold = 0.9f;
					result.exposure = 0.3f;
					result.radius = 2f;
					result.intensity = 1f;
					result.highQuality = true;
					result.antiFlicker = false;
					return result;
				}
			}
		}

		[SerializeField]
		public Settings settings = Settings.defaultSettings;

		[SerializeField]
		[HideInInspector]
		private Shader m_Shader;

		private Material m_Material;

		public Shader shader
		{
			get
			{
				if (m_Shader == null)
				{
					m_Shader = Shader.Find("Hidden/Image Effects/Cinematic/Bloom");
				}
				return m_Shader;
			}
		}

		public Material material
		{
			get
			{
				if (m_Material == null)
				{
					m_Material = ImageEffectHelper.CheckShaderAndCreateMaterial(shader);
				}
				return m_Material;
			}
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(shader, true, false, this))
			{
				base.enabled = false;
			}
		}

		private void OnDisable()
		{
			if (m_Material != null)
			{
				UnityEngine.Object.DestroyImmediate(m_Material);
			}
			m_Material = null;
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			bool isMobilePlatform = Application.isMobilePlatform;
			bool flag = QualitySettings.activeColorSpace == ColorSpace.Gamma;
			int num = source.width;
			int num2 = source.height;
			if (!settings.highQuality)
			{
				num /= 2;
				num2 /= 2;
			}
			RenderTextureFormat format = (isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
			float num3 = Mathf.Log(num2, 2f) + settings.radius - 6f;
			int num4 = (int)num3;
			int num5 = Mathf.Max(2, num4);
			material.SetFloat("_Threshold", settings.threshold);
			float num6 = 0f - Mathf.Log(Mathf.Lerp(0.01f, 0.99999f, settings.exposure), 10f);
			material.SetFloat("_Cutoff", settings.threshold + num6 * 10f);
			bool flag2 = !settings.highQuality && settings.antiFlicker;
			material.SetFloat("_PrefilterOffs", flag2 ? (-0.5f) : 0f);
			material.SetFloat("_SampleScale", 0.5f + num3 - (float)num4);
			material.SetFloat("_Intensity", settings.intensity);
			if (settings.highQuality)
			{
				material.EnableKeyword("HIGH_QUALITY");
			}
			else
			{
				material.DisableKeyword("HIGH_QUALITY");
			}
			if (settings.antiFlicker)
			{
				material.EnableKeyword("ANTI_FLICKER");
			}
			else
			{
				material.DisableKeyword("ANTI_FLICKER");
			}
			if (flag)
			{
				material.DisableKeyword("LINEAR_COLOR");
				material.EnableKeyword("GAMMA_COLOR");
			}
			else
			{
				material.EnableKeyword("LINEAR_COLOR");
				material.DisableKeyword("GAMMA_COLOR");
			}
			RenderTexture[] array = new RenderTexture[num5 + 1];
			RenderTexture[] array2 = new RenderTexture[num5 + 1];
			for (int i = 0; i < num5 + 1; i++)
			{
				array[i] = RenderTexture.GetTemporary(num, num2, 0, format);
				if (i > 0 && i < num5)
				{
					array2[i] = RenderTexture.GetTemporary(num, num2, 0, format);
				}
				num /= 2;
				num2 /= 2;
			}
			Graphics.Blit(source, array[0], material, 0);
			Graphics.Blit(array[0], array[1], material, 1);
			for (int j = 1; j < num5; j++)
			{
				Graphics.Blit(array[j], array[j + 1], material, 2);
			}
			material.SetTexture("_BaseTex", array[num5 - 1]);
			Graphics.Blit(array[num5], array2[num5 - 1], material, 3);
			for (int num7 = num5 - 1; num7 > 1; num7--)
			{
				material.SetTexture("_BaseTex", array[num7 - 1]);
				Graphics.Blit(array2[num7], array2[num7 - 1], material, 3);
			}
			material.SetTexture("_BaseTex", source);
			Graphics.Blit(array2[1], destination, material, 4);
			for (int k = 0; k < num5 + 1; k++)
			{
				RenderTexture.ReleaseTemporary(array[k]);
				RenderTexture.ReleaseTemporary(array2[k]);
			}
		}
	}
}
