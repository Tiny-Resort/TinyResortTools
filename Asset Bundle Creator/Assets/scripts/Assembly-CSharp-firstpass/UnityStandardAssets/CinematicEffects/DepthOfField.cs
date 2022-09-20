using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Cinematic/Depth Of Field")]
	[RequireComponent(typeof(Camera))]
	public class DepthOfField : MonoBehaviour
	{
		private enum Passes
		{
			BlurAlphaWeighted = 0,
			BoxBlur = 1,
			DilateFgCocFromColor = 2,
			DilateFgCoc = 3,
			CaptureCoc = 4,
			CaptureCocExplicit = 5,
			VisualizeCoc = 6,
			VisualizeCocExplicit = 7,
			CocPrefilter = 8,
			CircleBlur = 9,
			CircleBlurWithDilatedFg = 10,
			CircleBlurLowQuality = 11,
			CircleBlowLowQualityWithDilatedFg = 12,
			Merge = 13,
			MergeExplicit = 14,
			MergeBicubic = 15,
			MergeExplicitBicubic = 16,
			ShapeLowQuality = 17,
			ShapeLowQualityDilateFg = 18,
			ShapeLowQualityMerge = 19,
			ShapeLowQualityMergeDilateFg = 20,
			ShapeMediumQuality = 21,
			ShapeMediumQualityDilateFg = 22,
			ShapeMediumQualityMerge = 23,
			ShapeMediumQualityMergeDilateFg = 24,
			ShapeHighQuality = 25,
			ShapeHighQualityDilateFg = 26,
			ShapeHighQualityMerge = 27,
			ShapeHighQualityMergeDilateFg = 28
		}

		private enum MedianPasses
		{
			Median3 = 0,
			Median3X3 = 1
		}

		private enum BokehTexturesPasses
		{
			Apply = 0,
			Collect = 1
		}

		public enum TweakMode
		{
			Basic = 0,
			Advanced = 1,
			Explicit = 2
		}

		public enum ApertureShape
		{
			Circular = 0,
			Hexagonal = 1,
			Octogonal = 2
		}

		public enum QualityPreset
		{
			Simple = 0,
			Low = 1,
			Medium = 2,
			High = 3,
			VeryHigh = 4,
			Ultra = 5,
			Custom = 6
		}

		public enum FilterQuality
		{
			None = 0,
			Normal = 1,
			High = 2
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class TopLevelSettings : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class SettingsGroup : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class AllTweakModes : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class Basic : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class Advanced : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class Explicit : Attribute
		{
		}

		[Serializable]
		public struct GlobalSettings
		{
			[Tooltip("Allows to view where the blur will be applied. Yellow for near blur, blue for far blur.")]
			public bool visualizeBluriness;

			[Tooltip("Setup mode. Use \"Advanced\" if you need more control on blur settings and/or want to use a bokeh texture. \"Explicit\" is the same as \"Advanced\" but makes use of \"Near Plane\" and \"Far Plane\" values instead of \"F-Stop\".")]
			public TweakMode tweakMode;

			[Tooltip("Quality presets. Use \"Custom\" for more advanced settings.")]
			public QualityPreset quality;

			[Space]
			[Tooltip("\"Circular\" is the fastest, followed by \"Hexagonal\" and \"Octogonal\".")]
			public ApertureShape apertureShape;

			[Range(0f, 179f)]
			[Tooltip("Rotates the aperture when working with \"Hexagonal\" and \"Ortogonal\".")]
			public float apertureOrientation;

			public static GlobalSettings defaultSettings
			{
				get
				{
					GlobalSettings result = default(GlobalSettings);
					result.visualizeBluriness = false;
					result.tweakMode = TweakMode.Basic;
					result.quality = QualityPreset.High;
					result.apertureShape = ApertureShape.Circular;
					result.apertureOrientation = 0f;
					return result;
				}
			}
		}

		[Serializable]
		public struct QualitySettings
		{
			[Tooltip("Enable this to get smooth bokeh.")]
			public bool prefilterBlur;

			[Tooltip("Applies a median filter for even smoother bokeh.")]
			public FilterQuality medianFilter;

			[Tooltip("Dilates near blur to make it smoother.")]
			public bool dilateNearBlur;

			[Tooltip("Uses high quality upsampling for the blur passes.")]
			public bool highQualityUpsampling;

			public static QualitySettings[] presetQualitySettings = new QualitySettings[6]
			{
				new QualitySettings
				{
					prefilterBlur = false,
					medianFilter = FilterQuality.None,
					dilateNearBlur = false,
					highQualityUpsampling = false
				},
				new QualitySettings
				{
					prefilterBlur = true,
					medianFilter = FilterQuality.None,
					dilateNearBlur = false,
					highQualityUpsampling = false
				},
				new QualitySettings
				{
					prefilterBlur = true,
					medianFilter = FilterQuality.Normal,
					dilateNearBlur = false,
					highQualityUpsampling = false
				},
				new QualitySettings
				{
					prefilterBlur = true,
					medianFilter = FilterQuality.Normal,
					dilateNearBlur = true,
					highQualityUpsampling = false
				},
				new QualitySettings
				{
					prefilterBlur = true,
					medianFilter = FilterQuality.High,
					dilateNearBlur = true,
					highQualityUpsampling = false
				},
				new QualitySettings
				{
					prefilterBlur = true,
					medianFilter = FilterQuality.High,
					dilateNearBlur = true,
					highQualityUpsampling = true
				}
			};
		}

		[Serializable]
		public struct FocusSettings
		{
			[Basic]
			[Advanced]
			[Explicit]
			[Tooltip("Auto-focus on a selected transform.")]
			public Transform transform;

			[Basic]
			[Advanced]
			[Explicit]
			[Range(0f, 1f)]
			[Tooltip("Focus distance.")]
			public float plane;

			[Explicit]
			[Range(0f, 1f)]
			[Tooltip("Near focus distance.")]
			public float nearPlane;

			[Explicit]
			[Range(0f, 1f)]
			[Tooltip("Far focus distance.")]
			public float farPlane;

			[Basic]
			[Advanced]
			[Range(0f, 32f)]
			[Tooltip("Simulates focal ratio. Lower values will result in a narrow depth of field.")]
			public float fStops;

			[Basic]
			[Advanced]
			[Explicit]
			[Range(0f, 1f)]
			[Tooltip("Focus range/spread. Use this to fine-tune the F-Stop range.")]
			public float rangeAdjustment;

			public static FocusSettings defaultSettings
			{
				get
				{
					FocusSettings result = default(FocusSettings);
					result.transform = null;
					result.plane = 0.225f;
					result.nearPlane = 0f;
					result.farPlane = 1f;
					result.fStops = 5f;
					result.rangeAdjustment = 0.9f;
					return result;
				}
			}
		}

		[Serializable]
		public struct BokehTextureSettings
		{
			[Advanced]
			[Explicit]
			[Tooltip("Adding a texture to this field will enable the use of \"Bokeh Textures\". Use with care. This feature is only available on Shader Model 5 compatible-hardware and performance scale with the amount of bokeh.")]
			public Texture2D texture;

			[Advanced]
			[Explicit]
			[Range(0.01f, 5f)]
			[Tooltip("Maximum size of bokeh textures on screen.")]
			public float scale;

			[Advanced]
			[Explicit]
			[Range(0.01f, 100f)]
			[Tooltip("Bokeh brightness.")]
			public float intensity;

			[Advanced]
			[Explicit]
			[Range(0.01f, 50f)]
			[Tooltip("Controls the amount of bokeh textures. Lower values mean more bokeh splats.")]
			public float threshold;

			[Advanced]
			[Explicit]
			[Range(0.01f, 1f)]
			[Tooltip("Controls the spawn conditions. Lower values mean more visible bokeh.")]
			public float spawnHeuristic;

			public static BokehTextureSettings defaultSettings
			{
				get
				{
					BokehTextureSettings result = default(BokehTextureSettings);
					result.texture = null;
					result.scale = 1f;
					result.intensity = 50f;
					result.threshold = 2f;
					result.spawnHeuristic = 0.15f;
					return result;
				}
			}
		}

		[Serializable]
		public struct BlurSettings
		{
			[Basic]
			[Advanced]
			[Explicit]
			[Range(0f, 35f)]
			[Tooltip("Maximum blur radius for the near plane.")]
			public float nearRadius;

			[Basic]
			[Advanced]
			[Explicit]
			[Range(0f, 35f)]
			[Tooltip("Maximum blur radius for the far plane.")]
			public float farRadius;

			[Advanced]
			[Explicit]
			[Range(0.5f, 4f)]
			[Tooltip("Blur luminosity booster threshold for the near and far boost amounts.")]
			public float boostPoint;

			[Advanced]
			[Explicit]
			[Range(0f, 1f)]
			[Tooltip("Boosts luminosity in the near blur.")]
			public float nearBoostAmount;

			[Advanced]
			[Explicit]
			[Range(0f, 1f)]
			[Tooltip("Boosts luminosity in the far blur.")]
			public float farBoostAmount;

			public static BlurSettings defaultSettings
			{
				get
				{
					BlurSettings result = default(BlurSettings);
					result.nearRadius = 20f;
					result.farRadius = 20f;
					result.boostPoint = 0.75f;
					result.nearBoostAmount = 0f;
					result.farBoostAmount = 0f;
					return result;
				}
			}
		}

		private const float kMaxBlur = 35f;

		[TopLevelSettings]
		public GlobalSettings settings = GlobalSettings.defaultSettings;

		[SettingsGroup]
		[AllTweakModes]
		public QualitySettings quality = QualitySettings.presetQualitySettings[3];

		[SettingsGroup]
		public FocusSettings focus = FocusSettings.defaultSettings;

		[SettingsGroup]
		public BokehTextureSettings bokehTexture = BokehTextureSettings.defaultSettings;

		[SettingsGroup]
		public BlurSettings blur = BlurSettings.defaultSettings;

		[SerializeField]
		private Shader m_FilmicDepthOfFieldShader;

		[SerializeField]
		private Shader m_MedianFilterShader;

		[SerializeField]
		private Shader m_TextureBokehShader;

		private RenderTextureUtility m_RTU = new RenderTextureUtility();

		private Material m_FilmicDepthOfFieldMaterial;

		private Material m_MedianFilterMaterial;

		private Material m_TextureBokehMaterial;

		private ComputeBuffer m_ComputeBufferDrawArgs;

		private ComputeBuffer m_ComputeBufferPoints;

		private QualitySettings m_CurrentQualitySettings;

		private float m_LastApertureOrientation;

		private Vector4 m_OctogonalBokehDirection1;

		private Vector4 m_OctogonalBokehDirection2;

		private Vector4 m_OctogonalBokehDirection3;

		private Vector4 m_OctogonalBokehDirection4;

		private Vector4 m_HexagonalBokehDirection1;

		private Vector4 m_HexagonalBokehDirection2;

		private Vector4 m_HexagonalBokehDirection3;

		public Shader filmicDepthOfFieldShader
		{
			get
			{
				if (m_FilmicDepthOfFieldShader == null)
				{
					m_FilmicDepthOfFieldShader = Shader.Find("Hidden/DepthOfField/DepthOfField");
				}
				return m_FilmicDepthOfFieldShader;
			}
		}

		public Shader medianFilterShader
		{
			get
			{
				if (m_MedianFilterShader == null)
				{
					m_MedianFilterShader = Shader.Find("Hidden/DepthOfField/MedianFilter");
				}
				return m_MedianFilterShader;
			}
		}

		public Shader textureBokehShader
		{
			get
			{
				if (m_TextureBokehShader == null)
				{
					m_TextureBokehShader = Shader.Find("Hidden/DepthOfField/BokehSplatting");
				}
				return m_TextureBokehShader;
			}
		}

		public Material filmicDepthOfFieldMaterial
		{
			get
			{
				if (m_FilmicDepthOfFieldMaterial == null)
				{
					m_FilmicDepthOfFieldMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(filmicDepthOfFieldShader);
				}
				return m_FilmicDepthOfFieldMaterial;
			}
		}

		public Material medianFilterMaterial
		{
			get
			{
				if (m_MedianFilterMaterial == null)
				{
					m_MedianFilterMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(medianFilterShader);
				}
				return m_MedianFilterMaterial;
			}
		}

		public Material textureBokehMaterial
		{
			get
			{
				if (m_TextureBokehMaterial == null)
				{
					m_TextureBokehMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(textureBokehShader);
				}
				return m_TextureBokehMaterial;
			}
		}

		public ComputeBuffer computeBufferDrawArgs
		{
			get
			{
				if (m_ComputeBufferDrawArgs == null)
				{
					m_ComputeBufferDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
					m_ComputeBufferDrawArgs.SetData(new int[4] { 0, 1, 0, 0 });
				}
				return m_ComputeBufferDrawArgs;
			}
		}

		public ComputeBuffer computeBufferPoints
		{
			get
			{
				if (m_ComputeBufferPoints == null)
				{
					m_ComputeBufferPoints = new ComputeBuffer(90000, 28, ComputeBufferType.Append);
				}
				return m_ComputeBufferPoints;
			}
		}

		private bool shouldPerformBokeh
		{
			get
			{
				if (ImageEffectHelper.supportsDX11 && bokehTexture.texture != null && (bool)textureBokehMaterial)
				{
					return settings.tweakMode != TweakMode.Basic;
				}
				return false;
			}
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(filmicDepthOfFieldShader, true, true, this) || !ImageEffectHelper.IsSupported(medianFilterShader, true, true, this))
			{
				base.enabled = false;
				return;
			}
			if (ImageEffectHelper.supportsDX11 && !ImageEffectHelper.IsSupported(textureBokehShader, true, true, this))
			{
				base.enabled = false;
				return;
			}
			ComputeBlurDirections(true);
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}

		private void OnDisable()
		{
			ReleaseComputeResources();
			if (m_FilmicDepthOfFieldMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(m_FilmicDepthOfFieldMaterial);
			}
			if (m_TextureBokehMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(m_TextureBokehMaterial);
			}
			if (m_MedianFilterMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(m_MedianFilterMaterial);
			}
			m_FilmicDepthOfFieldMaterial = null;
			m_TextureBokehMaterial = null;
			m_MedianFilterMaterial = null;
			m_RTU.ReleaseAllTemporaryRenderTextures();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (medianFilterMaterial == null || filmicDepthOfFieldMaterial == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (settings.visualizeBluriness)
			{
				Vector4 blurParams;
				Vector4 blurCoe;
				ComputeCocParameters(out blurParams, out blurCoe);
				filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurParams);
				filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurCoe);
				Graphics.Blit(null, destination, filmicDepthOfFieldMaterial, (settings.tweakMode == TweakMode.Explicit) ? 7 : 6);
			}
			else
			{
				DoDepthOfField(source, destination);
			}
			m_RTU.ReleaseAllTemporaryRenderTextures();
		}

		private void DoDepthOfField(RenderTexture source, RenderTexture destination)
		{
			m_CurrentQualitySettings = quality;
			if (settings.quality != QualityPreset.Custom)
			{
				m_CurrentQualitySettings = QualitySettings.presetQualitySettings[(int)settings.quality];
			}
			float num = (float)source.height / 720f;
			float num2 = num;
			float num3 = Mathf.Max(blur.nearRadius, blur.farRadius) * num2 * 0.75f;
			float num4 = blur.nearRadius * num;
			float num5 = blur.farRadius * num;
			float num6 = Mathf.Max(num4, num5);
			switch (settings.apertureShape)
			{
			case ApertureShape.Hexagonal:
				num6 *= 1.2f;
				break;
			case ApertureShape.Octogonal:
				num6 *= 1.15f;
				break;
			}
			if (num6 < 0.5f)
			{
				Graphics.Blit(source, destination);
				return;
			}
			int width = source.width / 2;
			int height = source.height / 2;
			Vector4 value = new Vector4(num4 * 0.5f, num5 * 0.5f, 0f, 0f);
			RenderTexture temporaryRenderTexture = m_RTU.GetTemporaryRenderTexture(width, height);
			RenderTexture temporaryRenderTexture2 = m_RTU.GetTemporaryRenderTexture(width, height);
			Vector4 blurParams;
			Vector4 blurCoe;
			ComputeCocParameters(out blurParams, out blurCoe);
			filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurParams);
			filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurCoe);
			filmicDepthOfFieldMaterial.SetVector("_BoostParams", new Vector4(num4 * blur.nearBoostAmount * -0.5f, num5 * blur.farBoostAmount * 0.5f, blur.boostPoint, 0f));
			Graphics.Blit(source, temporaryRenderTexture2, filmicDepthOfFieldMaterial, (settings.tweakMode == TweakMode.Explicit) ? 5 : 4);
			RenderTexture src = temporaryRenderTexture2;
			RenderTexture dst = temporaryRenderTexture;
			if (shouldPerformBokeh)
			{
				RenderTexture temporaryRenderTexture3 = m_RTU.GetTemporaryRenderTexture(width, height);
				Graphics.Blit(src, temporaryRenderTexture3, filmicDepthOfFieldMaterial, 1);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(0f, 1.5f, 0f, 1.5f));
				Graphics.Blit(temporaryRenderTexture3, dst, filmicDepthOfFieldMaterial, 0);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(1.5f, 0f, 0f, 1.5f));
				Graphics.Blit(dst, temporaryRenderTexture3, filmicDepthOfFieldMaterial, 0);
				textureBokehMaterial.SetTexture("_BlurredColor", temporaryRenderTexture3);
				textureBokehMaterial.SetFloat("_SpawnHeuristic", bokehTexture.spawnHeuristic);
				textureBokehMaterial.SetVector("_BokehParams", new Vector4(bokehTexture.scale * num2, bokehTexture.intensity, bokehTexture.threshold, num3));
				Graphics.SetRandomWriteTarget(1, computeBufferPoints);
				Graphics.Blit(src, dst, textureBokehMaterial, 1);
				Graphics.ClearRandomWriteTargets();
				SwapRenderTexture(ref src, ref dst);
				m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture3);
			}
			filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurParams);
			filmicDepthOfFieldMaterial.SetVector("_BlurCoe", value);
			filmicDepthOfFieldMaterial.SetVector("_BoostParams", new Vector4(num4 * blur.nearBoostAmount * -0.5f, num5 * blur.farBoostAmount * 0.5f, blur.boostPoint, 0f));
			RenderTexture renderTexture = null;
			if (m_CurrentQualitySettings.dilateNearBlur)
			{
				RenderTexture temporaryRenderTexture4 = m_RTU.GetTemporaryRenderTexture(width, height, 0, RenderTextureFormat.RGHalf);
				renderTexture = m_RTU.GetTemporaryRenderTexture(width, height, 0, RenderTextureFormat.RGHalf);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(0f, num4 * 0.75f, 0f, 0f));
				Graphics.Blit(src, temporaryRenderTexture4, filmicDepthOfFieldMaterial, 2);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(num4 * 0.75f, 0f, 0f, 0f));
				Graphics.Blit(temporaryRenderTexture4, renderTexture, filmicDepthOfFieldMaterial, 3);
				m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture4);
			}
			if (m_CurrentQualitySettings.prefilterBlur)
			{
				Graphics.Blit(src, dst, filmicDepthOfFieldMaterial, 8);
				SwapRenderTexture(ref src, ref dst);
			}
			switch (settings.apertureShape)
			{
			case ApertureShape.Circular:
				DoCircularBlur(renderTexture, ref src, ref dst, num6);
				break;
			case ApertureShape.Hexagonal:
				DoHexagonalBlur(renderTexture, ref src, ref dst, num6);
				break;
			case ApertureShape.Octogonal:
				DoOctogonalBlur(renderTexture, ref src, ref dst, num6);
				break;
			}
			switch (m_CurrentQualitySettings.medianFilter)
			{
			case FilterQuality.Normal:
				medianFilterMaterial.SetVector("_Offsets", new Vector4(1f, 0f, 0f, 0f));
				Graphics.Blit(src, dst, medianFilterMaterial, 0);
				SwapRenderTexture(ref src, ref dst);
				medianFilterMaterial.SetVector("_Offsets", new Vector4(0f, 1f, 0f, 0f));
				Graphics.Blit(src, dst, medianFilterMaterial, 0);
				SwapRenderTexture(ref src, ref dst);
				break;
			case FilterQuality.High:
				Graphics.Blit(src, dst, medianFilterMaterial, 1);
				SwapRenderTexture(ref src, ref dst);
				break;
			}
			filmicDepthOfFieldMaterial.SetVector("_BlurCoe", value);
			filmicDepthOfFieldMaterial.SetVector("_Convolved_TexelSize", new Vector4(src.width, src.height, 1f / (float)src.width, 1f / (float)src.height));
			filmicDepthOfFieldMaterial.SetTexture("_SecondTex", src);
			int pass = ((settings.tweakMode == TweakMode.Explicit) ? 14 : 13);
			if (m_CurrentQualitySettings.highQualityUpsampling)
			{
				pass = ((settings.tweakMode == TweakMode.Explicit) ? 16 : 15);
			}
			if (shouldPerformBokeh)
			{
				RenderTexture temporaryRenderTexture5 = m_RTU.GetTemporaryRenderTexture(source.height, source.width, 0, source.format);
				Graphics.Blit(source, temporaryRenderTexture5, filmicDepthOfFieldMaterial, pass);
				Graphics.SetRenderTarget(temporaryRenderTexture5);
				ComputeBuffer.CopyCount(computeBufferPoints, computeBufferDrawArgs, 0);
				textureBokehMaterial.SetBuffer("pointBuffer", computeBufferPoints);
				textureBokehMaterial.SetTexture("_MainTex", bokehTexture.texture);
				textureBokehMaterial.SetVector("_Screen", new Vector3(1f / (1f * (float)source.width), 1f / (1f * (float)source.height), num3));
				textureBokehMaterial.SetPass(0);
				Graphics.DrawProceduralIndirectNow(MeshTopology.Points, computeBufferDrawArgs);
				Graphics.Blit(temporaryRenderTexture5, destination);
			}
			else
			{
				Graphics.Blit(source, destination, filmicDepthOfFieldMaterial, pass);
			}
		}

		private void DoHexagonalBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
		{
			ComputeBlurDirections(false);
			int blurPass;
			int blurAndMergePass;
			GetDirectionalBlurPassesFromRadius(blurredFgCoc, maxRadius, out blurPass, out blurAndMergePass);
			filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
			RenderTexture temporaryRenderTexture = m_RTU.GetTemporaryRenderTexture(src.width, src.height, 0, src.format);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection1);
			Graphics.Blit(src, temporaryRenderTexture, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection2);
			Graphics.Blit(temporaryRenderTexture, src, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection3);
			filmicDepthOfFieldMaterial.SetTexture("_ThirdTex", src);
			Graphics.Blit(temporaryRenderTexture, dst, filmicDepthOfFieldMaterial, blurAndMergePass);
			m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture);
			SwapRenderTexture(ref src, ref dst);
		}

		private void DoOctogonalBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
		{
			ComputeBlurDirections(false);
			int blurPass;
			int blurAndMergePass;
			GetDirectionalBlurPassesFromRadius(blurredFgCoc, maxRadius, out blurPass, out blurAndMergePass);
			filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
			RenderTexture temporaryRenderTexture = m_RTU.GetTemporaryRenderTexture(src.width, src.height, 0, src.format);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection1);
			Graphics.Blit(src, temporaryRenderTexture, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection2);
			Graphics.Blit(temporaryRenderTexture, dst, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection3);
			Graphics.Blit(src, temporaryRenderTexture, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection4);
			filmicDepthOfFieldMaterial.SetTexture("_ThirdTex", dst);
			Graphics.Blit(temporaryRenderTexture, src, filmicDepthOfFieldMaterial, blurAndMergePass);
			m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture);
		}

		private void DoCircularBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
		{
			int pass;
			if (blurredFgCoc != null)
			{
				filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
				pass = ((maxRadius > 10f) ? 10 : 12);
			}
			else
			{
				pass = ((maxRadius > 10f) ? 9 : 11);
			}
			Graphics.Blit(src, dst, filmicDepthOfFieldMaterial, pass);
			SwapRenderTexture(ref src, ref dst);
		}

		private void ComputeCocParameters(out Vector4 blurParams, out Vector4 blurCoe)
		{
			Camera component = GetComponent<Camera>();
			float num = (focus.transform ? (component.WorldToViewportPoint(focus.transform.position).z / component.farClipPlane) : (focus.plane * focus.plane * focus.plane * focus.plane));
			if (settings.tweakMode == TweakMode.Basic || settings.tweakMode == TweakMode.Advanced)
			{
				float w = focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment;
				float num2 = 4f / Mathf.Tan(0.5f * component.fieldOfView * ((float)Math.PI / 180f));
				float x = num2 / focus.fStops;
				blurCoe = new Vector4(0f, 0f, 1f, 1f);
				blurParams = new Vector4(x, num2, num, w);
				return;
			}
			float num3 = focus.nearPlane * focus.nearPlane * focus.nearPlane * focus.nearPlane;
			float num4 = focus.farPlane * focus.farPlane * focus.farPlane * focus.farPlane;
			float num5 = focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment;
			float num6 = num5;
			if (num <= num3)
			{
				num = num3 + 1E-07f;
			}
			if (num >= num4)
			{
				num = num4 - 1E-07f;
			}
			if (num - num5 <= num3)
			{
				num5 = num - num3 - 1E-07f;
			}
			if (num + num6 >= num4)
			{
				num6 = num4 - num - 1E-07f;
			}
			float num7 = 1f / (num3 - num + num5);
			float num8 = 1f / (num4 - num - num6);
			float num9 = 1f - num7 * num3;
			float num10 = 1f - num8 * num4;
			blurParams = new Vector4(-1f * num7, -1f * num9, 1f * num8, 1f * num10);
			blurCoe = new Vector4(0f, 0f, (num10 - num9) / (num7 - num8), 0f);
		}

		private void ReleaseComputeResources()
		{
			if (m_ComputeBufferDrawArgs != null)
			{
				m_ComputeBufferDrawArgs.Release();
			}
			if (m_ComputeBufferPoints != null)
			{
				m_ComputeBufferPoints.Release();
			}
			m_ComputeBufferDrawArgs = null;
			m_ComputeBufferPoints = null;
		}

		private void ComputeBlurDirections(bool force)
		{
			if (force || !(Math.Abs(m_LastApertureOrientation - settings.apertureOrientation) < float.Epsilon))
			{
				m_LastApertureOrientation = settings.apertureOrientation;
				float num = settings.apertureOrientation * ((float)Math.PI / 180f);
				float cosinus = Mathf.Cos(num);
				float sinus = Mathf.Sin(num);
				m_OctogonalBokehDirection1 = new Vector4(0.5f, 0f, 0f, 0f);
				m_OctogonalBokehDirection2 = new Vector4(0f, 0.5f, 1f, 0f);
				m_OctogonalBokehDirection3 = new Vector4(-0.353553f, 0.353553f, 1f, 0f);
				m_OctogonalBokehDirection4 = new Vector4(0.353553f, 0.353553f, 1f, 0f);
				m_HexagonalBokehDirection1 = new Vector4(0.5f, 0f, 0f, 0f);
				m_HexagonalBokehDirection2 = new Vector4(0.25f, 0.433013f, 1f, 0f);
				m_HexagonalBokehDirection3 = new Vector4(0.25f, -0.433013f, 1f, 0f);
				if (num > float.Epsilon)
				{
					Rotate2D(ref m_OctogonalBokehDirection1, cosinus, sinus);
					Rotate2D(ref m_OctogonalBokehDirection2, cosinus, sinus);
					Rotate2D(ref m_OctogonalBokehDirection3, cosinus, sinus);
					Rotate2D(ref m_OctogonalBokehDirection4, cosinus, sinus);
					Rotate2D(ref m_HexagonalBokehDirection1, cosinus, sinus);
					Rotate2D(ref m_HexagonalBokehDirection2, cosinus, sinus);
					Rotate2D(ref m_HexagonalBokehDirection3, cosinus, sinus);
				}
			}
		}

		private static void Rotate2D(ref Vector4 direction, float cosinus, float sinus)
		{
			Vector4 vector = direction;
			direction.x = vector.x * cosinus - vector.y * sinus;
			direction.y = vector.x * sinus + vector.y * cosinus;
		}

		private static void SwapRenderTexture(ref RenderTexture src, ref RenderTexture dst)
		{
			RenderTexture renderTexture = dst;
			dst = src;
			src = renderTexture;
		}

		private static void GetDirectionalBlurPassesFromRadius(RenderTexture blurredFgCoc, float maxRadius, out int blurPass, out int blurAndMergePass)
		{
			if (blurredFgCoc == null)
			{
				if (maxRadius > 10f)
				{
					blurPass = 25;
					blurAndMergePass = 27;
				}
				else if (maxRadius > 5f)
				{
					blurPass = 21;
					blurAndMergePass = 23;
				}
				else
				{
					blurPass = 17;
					blurAndMergePass = 19;
				}
			}
			else if (maxRadius > 10f)
			{
				blurPass = 26;
				blurAndMergePass = 28;
			}
			else if (maxRadius > 5f)
			{
				blurPass = 22;
				blurAndMergePass = 24;
			}
			else
			{
				blurPass = 18;
				blurAndMergePass = 20;
			}
		}
	}
}
