using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Cinematic/Lens Aberrations")]
	public class LensAberrations : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SettingsGroup : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class SimpleSetting : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class AdvancedSetting : Attribute
		{
		}

		public enum SettingsMode
		{
			Simple = 0,
			Advanced = 1
		}

		[Serializable]
		public struct DistortionSettings
		{
			public bool enabled;

			[Range(-100f, 100f)]
			[Tooltip("Distortion amount.")]
			public float amount;

			[Range(0f, 1f)]
			[Tooltip("Distortion center point (X axis).")]
			public float centerX;

			[Range(0f, 1f)]
			[Tooltip("Distortion center point (Y axis).")]
			public float centerY;

			[Range(0f, 2f)]
			[Tooltip("Amount multiplier on X axis.")]
			public float amountX;

			[Range(0f, 2f)]
			[Tooltip("Amount multiplier on Y axis.")]
			public float amountY;

			[Range(0.5f, 2f)]
			[Tooltip("Global screen scaling.")]
			public float scale;

			public static DistortionSettings defaultSettings
			{
				get
				{
					DistortionSettings result = default(DistortionSettings);
					result.enabled = false;
					result.amount = 0f;
					result.centerX = 0.5f;
					result.centerY = 0.5f;
					result.amountX = 1f;
					result.amountY = 1f;
					result.scale = 1f;
					return result;
				}
			}
		}

		[Serializable]
		public struct VignetteSettings
		{
			public bool enabled;

			[Tooltip("Use the \"Advanced\" mode if you need more control over the vignette shape and smoothness at the expense of performances.")]
			public SettingsMode mode;

			[Tooltip("Vignette color. Use the alpha channel for transparency.")]
			public Color color;

			[SimpleSetting]
			[Range(0f, 3f)]
			[Tooltip("Amount of vignetting on screen.")]
			public float intensity;

			[SimpleSetting]
			[Range(0.1f, 3f)]
			[Tooltip("Smoothness of the vignette borders.")]
			public float smoothness;

			[AdvancedSetting]
			[Range(0f, 1f)]
			[Tooltip("Vignette radius in screen coordinates.")]
			public float radius;

			[AdvancedSetting]
			[Range(0f, 1f)]
			[Tooltip("Smoothness of the vignette border. Tweak this at the same time as \"Falloff\" to get more control over the vignette gradient.")]
			public float spread;

			[AdvancedSetting]
			[Range(0f, 1f)]
			[Tooltip("Smoothness of the vignette border. Tweak this at the same time as \"Spread\" to get more control over the vignette gradient.")]
			public float falloff;

			[AdvancedSetting]
			[Range(0f, 1f)]
			[Tooltip("Lower values will make a square-ish vignette.")]
			public float roundness;

			[Range(0f, 1f)]
			[Tooltip("Blurs the corners of the screen. Leave this at 0 to disable it.")]
			public float blur;

			[Range(0f, 1f)]
			[Tooltip("Desaturate the corners of the screen. Leave this to 0 to disable it.")]
			public float desaturate;

			public static VignetteSettings defaultSettings
			{
				get
				{
					VignetteSettings result = default(VignetteSettings);
					result.enabled = false;
					result.mode = SettingsMode.Simple;
					result.color = Color.black;
					result.intensity = 1.2f;
					result.smoothness = 1.5f;
					result.radius = 0.7f;
					result.spread = 0.4f;
					result.falloff = 0.5f;
					result.roundness = 1f;
					result.blur = 0f;
					result.desaturate = 0f;
					return result;
				}
			}
		}

		[Serializable]
		public struct ChromaticAberrationSettings
		{
			public bool enabled;

			[Tooltip("Use the \"Advanced\" mode if you need more control over the chromatic aberrations at the expense of performances.")]
			public SettingsMode mode;

			[Range(-2f, 2f)]
			public float tangential;

			[AdvancedSetting]
			[Range(0f, 2f)]
			public float axial;

			[AdvancedSetting]
			[Range(0f, 2f)]
			public float contrastDependency;

			public static ChromaticAberrationSettings defaultSettings
			{
				get
				{
					ChromaticAberrationSettings result = default(ChromaticAberrationSettings);
					result.enabled = false;
					result.mode = SettingsMode.Simple;
					result.tangential = 0f;
					result.axial = 0f;
					result.contrastDependency = 0f;
					return result;
				}
			}
		}

		private enum Pass
		{
			BlurPrePass = 0,
			Simple = 1,
			Desaturate = 2,
			Blur = 3,
			BlurDesaturate = 4,
			ChromaticAberrationOnly = 5,
			DistortOnly = 6
		}

		[SettingsGroup]
		public DistortionSettings distortion = DistortionSettings.defaultSettings;

		[SettingsGroup]
		public VignetteSettings vignette = VignetteSettings.defaultSettings;

		[SettingsGroup]
		public ChromaticAberrationSettings chromaticAberration = ChromaticAberrationSettings.defaultSettings;

		[SerializeField]
		private Shader m_Shader;

		private Material m_Material;

		public Shader shader
		{
			get
			{
				if (m_Shader == null)
				{
					m_Shader = Shader.Find("Hidden/LensAberrations");
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
			if (!ImageEffectHelper.IsSupported(shader, false, false, this))
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
			if (!vignette.enabled && !chromaticAberration.enabled && !distortion.enabled)
			{
				Graphics.Blit(source, destination);
				return;
			}
			material.DisableKeyword("DISTORT");
			material.DisableKeyword("UNDISTORT");
			if (distortion.enabled)
			{
				float val = 1.6f * Math.Max(Mathf.Abs(distortion.amount), 1f);
				float num = (float)Math.PI / 180f * Math.Min(160f, val);
				float y = 2f * Mathf.Tan(num * 0.5f);
				Vector4 value = new Vector4(2f * distortion.centerX - 1f, 2f * distortion.centerY - 1f, distortion.amountX, distortion.amountY);
				Vector3 vector = new Vector3((distortion.amount >= 0f) ? num : (1f / num), y, 1f / distortion.scale);
				material.SetVector("_DistCenterScale", value);
				material.SetVector("_DistAmount", vector);
				if (distortion.amount >= 0f)
				{
					material.EnableKeyword("DISTORT");
				}
				else
				{
					material.EnableKeyword("UNDISTORT");
				}
			}
			material.SetColor("_VignetteColor", vignette.color);
			if (vignette.mode == SettingsMode.Simple)
			{
				material.SetVector("_Vignette1", new Vector4(vignette.intensity, vignette.smoothness, vignette.blur, 1f - vignette.desaturate));
				material.DisableKeyword("VIGNETTE_ADVANCED");
			}
			else
			{
				float num2 = 0.5f * vignette.radius;
				float num3 = num2 + vignette.spread;
				float num4 = Math.Max(1E-06f, (1f - vignette.falloff) * 0.5f);
				float z = (1f - vignette.roundness) * 6f + vignette.roundness * 2f;
				material.SetVector("_Vignette1", new Vector4(num2, 1f / (num3 - num2), vignette.blur, 1f - vignette.desaturate));
				material.SetVector("_Vignette2", new Vector3(num4, 0.5f / num4, z));
				material.EnableKeyword("VIGNETTE_ADVANCED");
			}
			material.DisableKeyword("CHROMATIC_SIMPLE");
			material.DisableKeyword("CHROMATIC_ADVANCED");
			if (chromaticAberration.enabled && !Mathf.Approximately(chromaticAberration.tangential, 0f))
			{
				if (chromaticAberration.mode == SettingsMode.Advanced)
				{
					material.EnableKeyword("CHROMATIC_ADVANCED");
				}
				else
				{
					material.EnableKeyword("CHROMATIC_SIMPLE");
				}
				Vector4 value2 = new Vector4(2.5f * chromaticAberration.tangential, 5f * chromaticAberration.axial, 5f / Mathf.Max(Mathf.Epsilon, chromaticAberration.contrastDependency), 5f);
				material.SetVector("_ChromaticAberration", value2);
			}
			if (vignette.enabled && vignette.blur > 0f)
			{
				int num5 = source.width / 2;
				int num6 = source.height / 2;
				RenderTexture temporary = RenderTexture.GetTemporary(num5, num6, 0, source.format);
				RenderTexture temporary2 = RenderTexture.GetTemporary(num5, num6, 0, source.format);
				material.SetVector("_BlurPass", new Vector2(1f / (float)num5, 0f));
				Graphics.Blit(source, temporary, material, 0);
				material.SetVector("_BlurPass", new Vector2(0f, 1f / (float)num6));
				Graphics.Blit(temporary, temporary2, material, 0);
				material.SetVector("_BlurPass", new Vector2(1f / (float)num5, 0f));
				Graphics.Blit(temporary2, temporary, material, 0);
				material.SetVector("_BlurPass", new Vector2(0f, 1f / (float)num6));
				Graphics.Blit(temporary, temporary2, material, 0);
				material.SetTexture("_BlurTex", temporary2);
				if (vignette.desaturate > 0f)
				{
					Graphics.Blit(source, destination, material, 4);
				}
				else
				{
					Graphics.Blit(source, destination, material, 3);
				}
				RenderTexture.ReleaseTemporary(temporary2);
				RenderTexture.ReleaseTemporary(temporary);
			}
			else if (vignette.enabled && vignette.desaturate > 0f)
			{
				Graphics.Blit(source, destination, material, 2);
			}
			else if (vignette.enabled)
			{
				Graphics.Blit(source, destination, material, 1);
			}
			else if (chromaticAberration.enabled)
			{
				Graphics.Blit(source, destination, material, 5);
			}
			else
			{
				Graphics.Blit(source, destination, material, 6);
			}
		}
	}
}
