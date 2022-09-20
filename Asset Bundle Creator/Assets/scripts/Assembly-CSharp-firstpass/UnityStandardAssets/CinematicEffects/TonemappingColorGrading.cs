using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Cinematic/Tonemapping and Color Grading")]
	public class TonemappingColorGrading : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SettingsGroup : Attribute
		{
		}

		public class IndentedGroup : PropertyAttribute
		{
		}

		public class ChannelMixer : PropertyAttribute
		{
		}

		public class ColorWheelGroup : PropertyAttribute
		{
			public int minSizePerWheel = 60;

			public int maxSizePerWheel = 150;

			public ColorWheelGroup()
			{
			}

			public ColorWheelGroup(int minSizePerWheel, int maxSizePerWheel)
			{
				this.minSizePerWheel = minSizePerWheel;
				this.maxSizePerWheel = maxSizePerWheel;
			}
		}

		public class Curve : PropertyAttribute
		{
			public Color color = Color.white;

			public Curve()
			{
			}

			public Curve(float r, float g, float b, float a)
			{
				color = new Color(r, g, b, a);
			}
		}

		[Serializable]
		public struct EyeAdaptationSettings
		{
			public bool enabled;

			[Min(0f)]
			[Tooltip("Midpoint Adjustment.")]
			public float middleGrey;

			[Tooltip("The lowest possible exposure value; adjust this value to modify the brightest areas of your level.")]
			public float min;

			[Tooltip("The highest possible exposure value; adjust this value to modify the darkest areas of your level.")]
			public float max;

			[Min(0f)]
			[Tooltip("Speed of linear adaptation. Higher is faster.")]
			public float speed;

			[Tooltip("Displays a luminosity helper in the GameView.")]
			public bool showDebug;

			public static EyeAdaptationSettings defaultSettings
			{
				get
				{
					EyeAdaptationSettings result = default(EyeAdaptationSettings);
					result.enabled = false;
					result.showDebug = false;
					result.middleGrey = 0.5f;
					result.min = -3f;
					result.max = 3f;
					result.speed = 1.5f;
					return result;
				}
			}
		}

		public enum Tonemapper
		{
			ACES = 0,
			Curve = 1,
			Hable = 2,
			HejlDawson = 3,
			Photographic = 4,
			Reinhard = 5
		}

		[Serializable]
		public struct TonemappingSettings
		{
			public bool enabled;

			[Tooltip("Tonemapping technique to use. ACES is the recommended one.")]
			public Tonemapper tonemapper;

			[Min(0f)]
			[Tooltip("Adjusts the overall exposure of the scene.")]
			public float exposure;

			[Tooltip("Custom tonemapping curve.")]
			public AnimationCurve curve;

			public static TonemappingSettings defaultSettings
			{
				get
				{
					TonemappingSettings result = default(TonemappingSettings);
					result.enabled = false;
					result.tonemapper = Tonemapper.ACES;
					result.exposure = 1f;
					result.curve = CurvesSettings.defaultCurve;
					return result;
				}
			}
		}

		[Serializable]
		public struct LUTSettings
		{
			public bool enabled;

			[Tooltip("Custom lookup texture (strip format, e.g. 256x16).")]
			public Texture texture;

			[Range(0f, 1f)]
			[Tooltip("Blending factor.")]
			public float contribution;

			public static LUTSettings defaultSettings
			{
				get
				{
					LUTSettings result = default(LUTSettings);
					result.enabled = false;
					result.texture = null;
					result.contribution = 1f;
					return result;
				}
			}
		}

		[Serializable]
		public struct ColorWheelsSettings
		{
			[ColorUsage(false)]
			public Color shadows;

			[ColorUsage(false)]
			public Color midtones;

			[ColorUsage(false)]
			public Color highlights;

			public static ColorWheelsSettings defaultSettings
			{
				get
				{
					ColorWheelsSettings result = default(ColorWheelsSettings);
					result.shadows = Color.white;
					result.midtones = Color.white;
					result.highlights = Color.white;
					return result;
				}
			}
		}

		[Serializable]
		public struct BasicsSettings
		{
			[Range(-2f, 2f)]
			[Tooltip("Sets the white balance to a custom color temperature.")]
			public float temperatureShift;

			[Range(-2f, 2f)]
			[Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
			public float tint;

			[Space]
			[Range(-0.5f, 0.5f)]
			[Tooltip("Shift the hue of all colors.")]
			public float hue;

			[Range(0f, 2f)]
			[Tooltip("Pushes the intensity of all colors.")]
			public float saturation;

			[Range(-1f, 1f)]
			[Tooltip("Adjusts the saturation so that clipping is minimized as colors approach full saturation.")]
			public float vibrance;

			[Range(0f, 10f)]
			[Tooltip("Brightens or darkens all colors.")]
			public float value;

			[Space]
			[Range(0f, 2f)]
			[Tooltip("Expands or shrinks the overall range of tonal values.")]
			public float contrast;

			[Range(0.01f, 5f)]
			[Tooltip("Contrast gain curve. Controls the steepness of the curve.")]
			public float gain;

			[Range(0.01f, 5f)]
			[Tooltip("Applies a pow function to the source.")]
			public float gamma;

			public static BasicsSettings defaultSettings
			{
				get
				{
					BasicsSettings result = default(BasicsSettings);
					result.temperatureShift = 0f;
					result.tint = 0f;
					result.contrast = 1f;
					result.hue = 0f;
					result.saturation = 1f;
					result.value = 1f;
					result.vibrance = 0f;
					result.gain = 1f;
					result.gamma = 1f;
					return result;
				}
			}
		}

		[Serializable]
		public struct ChannelMixerSettings
		{
			public int currentChannel;

			public Vector3[] channels;

			public static ChannelMixerSettings defaultSettings
			{
				get
				{
					ChannelMixerSettings result = default(ChannelMixerSettings);
					result.currentChannel = 0;
					result.channels = new Vector3[3]
					{
						new Vector3(1f, 0f, 0f),
						new Vector3(0f, 1f, 0f),
						new Vector3(0f, 0f, 1f)
					};
					return result;
				}
			}
		}

		[Serializable]
		public struct CurvesSettings
		{
			[Curve]
			public AnimationCurve master;

			[Curve(1f, 0f, 0f, 1f)]
			public AnimationCurve red;

			[Curve(0f, 1f, 0f, 1f)]
			public AnimationCurve green;

			[Curve(0f, 1f, 1f, 1f)]
			public AnimationCurve blue;

			public static CurvesSettings defaultSettings
			{
				get
				{
					CurvesSettings result = default(CurvesSettings);
					result.master = defaultCurve;
					result.red = defaultCurve;
					result.green = defaultCurve;
					result.blue = defaultCurve;
					return result;
				}
			}

			public static AnimationCurve defaultCurve
			{
				get
				{
					return new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
				}
			}
		}

		public enum ColorGradingPrecision
		{
			Normal = 0x10,
			High = 0x20
		}

		[Serializable]
		public struct ColorGradingSettings
		{
			public bool enabled;

			[Tooltip("Internal LUT precision. \"Normal\" is 256x16, \"High\" is 1024x32. Prefer \"Normal\" on mobile devices.")]
			public ColorGradingPrecision precision;

			[Space]
			[ColorWheelGroup]
			public ColorWheelsSettings colorWheels;

			[Space]
			[IndentedGroup]
			public BasicsSettings basics;

			[Space]
			[ChannelMixer]
			public ChannelMixerSettings channelMixer;

			[Space]
			[IndentedGroup]
			public CurvesSettings curves;

			[Space]
			[Tooltip("Use dithering to try and minimize color banding in dark areas.")]
			public bool useDithering;

			[Tooltip("Displays the generated LUT in the top left corner of the GameView.")]
			public bool showDebug;

			public static ColorGradingSettings defaultSettings
			{
				get
				{
					ColorGradingSettings result = default(ColorGradingSettings);
					result.enabled = false;
					result.useDithering = false;
					result.showDebug = false;
					result.precision = ColorGradingPrecision.Normal;
					result.colorWheels = ColorWheelsSettings.defaultSettings;
					result.basics = BasicsSettings.defaultSettings;
					result.channelMixer = ChannelMixerSettings.defaultSettings;
					result.curves = CurvesSettings.defaultSettings;
					return result;
				}
			}

			internal void Reset()
			{
				curves = CurvesSettings.defaultSettings;
			}
		}

		private enum Pass
		{
			LutGen = 0,
			AdaptationLog = 1,
			AdaptationExpBlend = 2,
			AdaptationExp = 3,
			TonemappingOff = 4,
			TonemappingACES = 5,
			TonemappingCurve = 6,
			TonemappingHable = 7,
			TonemappingHejlDawson = 8,
			TonemappingPhotographic = 9,
			TonemappingReinhard = 10,
			AdaptationDebug = 11
		}

		[SerializeField]
		[SettingsGroup]
		private EyeAdaptationSettings m_EyeAdaptation = EyeAdaptationSettings.defaultSettings;

		[SerializeField]
		[SettingsGroup]
		private TonemappingSettings m_Tonemapping = TonemappingSettings.defaultSettings;

		[SerializeField]
		[SettingsGroup]
		private LUTSettings m_Lut = LUTSettings.defaultSettings;

		[SerializeField]
		[SettingsGroup]
		private ColorGradingSettings m_ColorGrading = ColorGradingSettings.defaultSettings;

		private Texture2D m_IdentityLut;

		private RenderTexture m_InternalLut;

		private Texture2D m_CurveTexture;

		private Texture2D m_TonemapperCurve;

		private float m_TonemapperCurveRange;

		[SerializeField]
		private Shader m_Shader;

		private Material m_Material;

		private bool m_Dirty = true;

		private bool m_TonemapperDirty = true;

		private RenderTexture m_SmallAdaptiveRt;

		private RenderTextureFormat m_AdaptiveRtFormat;

		public EyeAdaptationSettings eyeAdaptation
		{
			get
			{
				return m_EyeAdaptation;
			}
			set
			{
				m_EyeAdaptation = value;
			}
		}

		public TonemappingSettings tonemapping
		{
			get
			{
				return m_Tonemapping;
			}
			set
			{
				m_Tonemapping = value;
				SetTonemapperDirty();
			}
		}

		public LUTSettings lut
		{
			get
			{
				return m_Lut;
			}
			set
			{
				m_Lut = value;
				SetDirty();
			}
		}

		public ColorGradingSettings colorGrading
		{
			get
			{
				return m_ColorGrading;
			}
			set
			{
				m_ColorGrading = value;
				SetDirty();
			}
		}

		private Texture2D identityLut
		{
			get
			{
				if (m_IdentityLut == null || m_IdentityLut.height != lutSize)
				{
					UnityEngine.Object.DestroyImmediate(m_IdentityLut);
					m_IdentityLut = GenerateIdentityLut(lutSize);
				}
				return m_IdentityLut;
			}
		}

		private RenderTexture internalLutRt
		{
			get
			{
				if (m_InternalLut == null || !m_InternalLut.IsCreated() || m_InternalLut.height != lutSize)
				{
					UnityEngine.Object.DestroyImmediate(m_InternalLut);
					m_InternalLut = new RenderTexture(lutSize * lutSize, lutSize, 0, RenderTextureFormat.ARGB32)
					{
						name = "Internal LUT",
						filterMode = FilterMode.Bilinear,
						anisoLevel = 0,
						hideFlags = HideFlags.DontSave
					};
				}
				return m_InternalLut;
			}
		}

		private Texture2D curveTexture
		{
			get
			{
				if (m_CurveTexture == null)
				{
					m_CurveTexture = new Texture2D(256, 1, TextureFormat.ARGB32, false, true)
					{
						name = "Curve texture",
						wrapMode = TextureWrapMode.Clamp,
						filterMode = FilterMode.Bilinear,
						anisoLevel = 0,
						hideFlags = HideFlags.DontSave
					};
				}
				return m_CurveTexture;
			}
		}

		private Texture2D tonemapperCurve
		{
			get
			{
				if (m_TonemapperCurve == null)
				{
					TextureFormat textureFormat = TextureFormat.RGB24;
					if (SystemInfo.SupportsTextureFormat(TextureFormat.RFloat))
					{
						textureFormat = TextureFormat.RFloat;
					}
					else if (SystemInfo.SupportsTextureFormat(TextureFormat.RHalf))
					{
						textureFormat = TextureFormat.RHalf;
					}
					m_TonemapperCurve = new Texture2D(256, 1, textureFormat, false, true)
					{
						name = "Tonemapper curve texture",
						wrapMode = TextureWrapMode.Clamp,
						filterMode = FilterMode.Bilinear,
						anisoLevel = 0,
						hideFlags = HideFlags.DontSave
					};
				}
				return m_TonemapperCurve;
			}
		}

		public Shader shader
		{
			get
			{
				if (m_Shader == null)
				{
					m_Shader = Shader.Find("Hidden/TonemappingColorGrading");
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

		public bool isGammaColorSpace
		{
			get
			{
				return QualitySettings.activeColorSpace == ColorSpace.Gamma;
			}
		}

		public int lutSize
		{
			get
			{
				return (int)colorGrading.precision;
			}
		}

		public bool validRenderTextureFormat { get; private set; }

		public bool validUserLutSize { get; private set; }

		public void SetDirty()
		{
			m_Dirty = true;
		}

		public void SetTonemapperDirty()
		{
			m_TonemapperDirty = true;
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(shader, false, true, this))
			{
				base.enabled = false;
				return;
			}
			SetDirty();
			SetTonemapperDirty();
		}

		private void OnDisable()
		{
			if (m_Material != null)
			{
				UnityEngine.Object.DestroyImmediate(m_Material);
			}
			if (m_IdentityLut != null)
			{
				UnityEngine.Object.DestroyImmediate(m_IdentityLut);
			}
			if (m_InternalLut != null)
			{
				UnityEngine.Object.DestroyImmediate(internalLutRt);
			}
			if (m_SmallAdaptiveRt != null)
			{
				UnityEngine.Object.DestroyImmediate(m_SmallAdaptiveRt);
			}
			if (m_CurveTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(m_CurveTexture);
			}
			if (m_TonemapperCurve != null)
			{
				UnityEngine.Object.DestroyImmediate(m_TonemapperCurve);
			}
			m_Material = null;
			m_IdentityLut = null;
			m_InternalLut = null;
			m_SmallAdaptiveRt = null;
			m_CurveTexture = null;
			m_TonemapperCurve = null;
		}

		private void OnValidate()
		{
			SetDirty();
			SetTonemapperDirty();
		}

		private static Texture2D GenerateIdentityLut(int dim)
		{
			Color[] array = new Color[dim * dim * dim];
			float num = 1f / ((float)dim - 1f);
			for (int i = 0; i < dim; i++)
			{
				for (int j = 0; j < dim; j++)
				{
					for (int k = 0; k < dim; k++)
					{
						array[i + j * dim + k * dim * dim] = new Color((float)i * num, Mathf.Abs((float)k * num), (float)j * num, 1f);
					}
				}
			}
			Texture2D texture2D = new Texture2D(dim * dim, dim, TextureFormat.RGB24, false, true);
			texture2D.name = "Identity LUT";
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.anisoLevel = 0;
			texture2D.hideFlags = HideFlags.DontSave;
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		private float StandardIlluminantY(float x)
		{
			return 2.87f * x - 3f * x * x - 0.27509508f;
		}

		private Vector3 CIExyToLMS(float x, float y)
		{
			float num = 1f;
			float num2 = num * x / y;
			float num3 = num * (1f - x - y) / y;
			float x2 = 0.7328f * num2 + 0.4296f * num - 0.1624f * num3;
			float y2 = -0.7036f * num2 + 1.6975f * num + 0.0061f * num3;
			float z = 0.003f * num2 + 0.0136f * num + 0.9834f * num3;
			return new Vector3(x2, y2, z);
		}

		private Vector3 GetWhiteBalance()
		{
			float temperatureShift = colorGrading.basics.temperatureShift;
			float tint = colorGrading.basics.tint;
			float x = 0.31271f - temperatureShift * ((temperatureShift < 0f) ? 0.1f : 0.05f);
			float y = StandardIlluminantY(x) + tint * 0.05f;
			Vector3 vector = new Vector3(0.949237f, 1.03542f, 1.08728f);
			Vector3 vector2 = CIExyToLMS(x, y);
			return new Vector3(vector.x / vector2.x, vector.y / vector2.y, vector.z / vector2.z);
		}

		private static Color NormalizeColor(Color c)
		{
			float num = (c.r + c.g + c.b) / 3f;
			if (Mathf.Approximately(num, 0f))
			{
				return new Color(1f, 1f, 1f, 1f);
			}
			Color result = default(Color);
			result.r = c.r / num;
			result.g = c.g / num;
			result.b = c.b / num;
			result.a = 1f;
			return result;
		}

		private void GenerateLiftGammaGain(out Color lift, out Color gamma, out Color gain)
		{
			Color color = NormalizeColor(colorGrading.colorWheels.shadows);
			Color color2 = NormalizeColor(colorGrading.colorWheels.midtones);
			Color color3 = NormalizeColor(colorGrading.colorWheels.highlights);
			float num = (color.r + color.g + color.b) / 3f;
			float num2 = (color2.r + color2.g + color2.b) / 3f;
			float num3 = (color3.r + color3.g + color3.b) / 3f;
			float r = (color.r - num) * 0.1f;
			float g = (color.g - num) * 0.1f;
			float b = (color.b - num) * 0.1f;
			float b2 = Mathf.Pow(2f, (color2.r - num2) * 0.5f);
			float b3 = Mathf.Pow(2f, (color2.g - num2) * 0.5f);
			float b4 = Mathf.Pow(2f, (color2.b - num2) * 0.5f);
			float r2 = Mathf.Pow(2f, (color3.r - num3) * 0.5f);
			float g2 = Mathf.Pow(2f, (color3.g - num3) * 0.5f);
			float b5 = Mathf.Pow(2f, (color3.b - num3) * 0.5f);
			float r3 = 1f / Mathf.Max(0.01f, b2);
			float g3 = 1f / Mathf.Max(0.01f, b3);
			float b6 = 1f / Mathf.Max(0.01f, b4);
			lift = new Color(r, g, b);
			gamma = new Color(r3, g3, b6);
			gain = new Color(r2, g2, b5);
		}

		private void GenCurveTexture()
		{
			AnimationCurve master = colorGrading.curves.master;
			AnimationCurve red = colorGrading.curves.red;
			AnimationCurve green = colorGrading.curves.green;
			AnimationCurve blue = colorGrading.curves.blue;
			Color[] array = new Color[256];
			for (float num = 0f; num <= 1f; num += 0.003921569f)
			{
				float a = Mathf.Clamp(master.Evaluate(num), 0f, 1f);
				float r = Mathf.Clamp(red.Evaluate(num), 0f, 1f);
				float g = Mathf.Clamp(green.Evaluate(num), 0f, 1f);
				float b = Mathf.Clamp(blue.Evaluate(num), 0f, 1f);
				array[(int)Mathf.Floor(num * 255f)] = new Color(r, g, b, a);
			}
			curveTexture.SetPixels(array);
			curveTexture.Apply();
		}

		private bool CheckUserLut()
		{
			validUserLutSize = lut.texture.height == (int)Mathf.Sqrt(lut.texture.width);
			return validUserLutSize;
		}

		private bool CheckSmallAdaptiveRt()
		{
			if (m_SmallAdaptiveRt != null)
			{
				return false;
			}
			m_AdaptiveRtFormat = RenderTextureFormat.ARGBHalf;
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf))
			{
				m_AdaptiveRtFormat = RenderTextureFormat.RGHalf;
			}
			m_SmallAdaptiveRt = new RenderTexture(1, 1, 0, m_AdaptiveRtFormat);
			m_SmallAdaptiveRt.hideFlags = HideFlags.DontSave;
			return true;
		}

		private void OnGUI()
		{
			if (Event.current.type == EventType.Repaint)
			{
				int num = 0;
				if (m_InternalLut != null && colorGrading.enabled && colorGrading.showDebug)
				{
					Graphics.DrawTexture(new Rect(0f, num, lutSize * lutSize, lutSize), internalLutRt);
					num += lutSize;
				}
				if (m_SmallAdaptiveRt != null && eyeAdaptation.enabled && eyeAdaptation.showDebug)
				{
					m_Material.SetPass(11);
					Graphics.DrawTexture(new Rect(0f, num, 256f, 16f), m_SmallAdaptiveRt, m_Material);
				}
			}
		}

		[ImageEffectTransformsToLDR]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (isGammaColorSpace)
			{
				material.EnableKeyword("GAMMA_COLORSPACE");
			}
			else
			{
				material.DisableKeyword("GAMMA_COLORSPACE");
			}
			material.DisableKeyword("ENABLE_EYE_ADAPTATION");
			material.DisableKeyword("ENABLE_COLOR_GRADING");
			material.DisableKeyword("ENABLE_DITHERING");
			Texture texture = null;
			float w = 1f;
			RenderTexture renderTexture = null;
			RenderTexture[] array = null;
			if (eyeAdaptation.enabled)
			{
				bool flag = CheckSmallAdaptiveRt();
				int num = ((source.width < source.height) ? source.width : source.height);
				int num2 = num | (num >> 1);
				int num3 = num2 | (num2 >> 2);
				int num4 = num3 | (num3 >> 4);
				int num5 = num4 | (num4 >> 8);
				int num6 = num5 | (num5 >> 16);
				int num7 = num6 - (num6 >> 1);
				renderTexture = RenderTexture.GetTemporary(num7, num7, 0, m_AdaptiveRtFormat);
				Graphics.Blit(source, renderTexture);
				int num8 = (int)Mathf.Log(renderTexture.width, 2f);
				int num9 = 2;
				array = new RenderTexture[num8];
				for (int i = 0; i < num8; i++)
				{
					array[i] = RenderTexture.GetTemporary(renderTexture.width / num9, renderTexture.width / num9, 0, m_AdaptiveRtFormat);
					num9 <<= 1;
				}
				RenderTexture source2 = array[num8 - 1];
				Graphics.Blit(renderTexture, array[0], material, 1);
				for (int j = 0; j < num8 - 1; j++)
				{
					Graphics.Blit(array[j], array[j + 1]);
					source2 = array[j + 1];
				}
				m_SmallAdaptiveRt.MarkRestoreExpected();
				material.SetFloat("_AdaptationSpeed", Mathf.Max(eyeAdaptation.speed, 0.001f));
				Graphics.Blit(source2, m_SmallAdaptiveRt, material, flag ? 3 : 2);
				material.SetFloat("_MiddleGrey", eyeAdaptation.middleGrey);
				material.SetFloat("_AdaptationMin", Mathf.Pow(2f, eyeAdaptation.min));
				material.SetFloat("_AdaptationMax", Mathf.Pow(2f, eyeAdaptation.max));
				material.SetTexture("_LumTex", m_SmallAdaptiveRt);
				material.EnableKeyword("ENABLE_EYE_ADAPTATION");
			}
			int num10 = 4;
			if (tonemapping.enabled)
			{
				if (tonemapping.tonemapper == Tonemapper.Curve)
				{
					if (m_TonemapperDirty)
					{
						float num11 = 1f;
						if (tonemapping.curve.length > 0)
						{
							num11 = tonemapping.curve[tonemapping.curve.length - 1].time;
							for (float num12 = 0f; num12 <= 1f; num12 += 0.003921569f)
							{
								float num13 = tonemapping.curve.Evaluate(num12 * num11);
								tonemapperCurve.SetPixel(Mathf.FloorToInt(num12 * 255f), 0, new Color(num13, num13, num13));
							}
							tonemapperCurve.Apply();
						}
						m_TonemapperCurveRange = 1f / num11;
						m_TonemapperDirty = false;
					}
					material.SetFloat("_ToneCurveRange", m_TonemapperCurveRange);
					material.SetTexture("_ToneCurve", tonemapperCurve);
				}
				material.SetFloat("_Exposure", tonemapping.exposure);
				num10 = (int)(num10 + (tonemapping.tonemapper + 1));
			}
			if (lut.enabled)
			{
				Texture texture2 = lut.texture;
				if (lut.texture == null || !CheckUserLut())
				{
					texture2 = identityLut;
				}
				texture = texture2;
				w = lut.contribution;
				material.EnableKeyword("ENABLE_COLOR_GRADING");
			}
			if (colorGrading.enabled)
			{
				if (m_Dirty || !m_InternalLut.IsCreated())
				{
					if (texture == null)
					{
						material.SetVector("_UserLutParams", new Vector4(1f / (float)identityLut.width, 1f / (float)identityLut.height, (float)identityLut.height - 1f, 1f));
						material.SetTexture("_UserLutTex", identityLut);
					}
					else
					{
						material.SetVector("_UserLutParams", new Vector4(1f / (float)texture.width, 1f / (float)texture.height, (float)texture.height - 1f, lut.contribution));
						material.SetTexture("_UserLutTex", texture);
					}
					Color lift;
					Color gamma;
					Color gain;
					GenerateLiftGammaGain(out lift, out gamma, out gain);
					GenCurveTexture();
					material.SetVector("_WhiteBalance", GetWhiteBalance());
					material.SetVector("_Lift", lift);
					material.SetVector("_Gamma", gamma);
					material.SetVector("_Gain", gain);
					material.SetVector("_ContrastGainGamma", new Vector3(colorGrading.basics.contrast, colorGrading.basics.gain, 1f / colorGrading.basics.gamma));
					material.SetFloat("_Vibrance", colorGrading.basics.vibrance);
					material.SetVector("_HSV", new Vector4(colorGrading.basics.hue, colorGrading.basics.saturation, colorGrading.basics.value));
					material.SetVector("_ChannelMixerRed", colorGrading.channelMixer.channels[0]);
					material.SetVector("_ChannelMixerGreen", colorGrading.channelMixer.channels[1]);
					material.SetVector("_ChannelMixerBlue", colorGrading.channelMixer.channels[2]);
					material.SetTexture("_CurveTex", curveTexture);
					internalLutRt.MarkRestoreExpected();
					Graphics.Blit(identityLut, internalLutRt, material, 0);
					m_Dirty = false;
				}
				texture = internalLutRt;
				w = 1f;
				material.EnableKeyword("ENABLE_COLOR_GRADING");
				if (colorGrading.useDithering)
				{
					material.EnableKeyword("ENABLE_DITHERING");
				}
			}
			if (texture != null)
			{
				material.SetTexture("_LutTex", texture);
				material.SetVector("_LutParams", new Vector4(1f / (float)texture.width, 1f / (float)texture.height, (float)texture.height - 1f, w));
			}
			Graphics.Blit(source, destination, material, num10);
			if (eyeAdaptation.enabled)
			{
				for (int k = 0; k < array.Length; k++)
				{
					RenderTexture.ReleaseTemporary(array[k]);
				}
				RenderTexture.ReleaseTemporary(renderTexture);
			}
		}

		public Texture2D BakeLUT()
		{
			Texture2D texture2D = new Texture2D(internalLutRt.width, internalLutRt.height, TextureFormat.RGB24, false, true);
			RenderTexture.active = internalLutRt;
			texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			RenderTexture.active = null;
			return texture2D;
		}
	}
}
