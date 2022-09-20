using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Cinematic/Ambient Occlusion")]
	public class AmbientOcclusion : MonoBehaviour
	{
		private struct PropertyObserver
		{
			private int _blurIterations;

			private bool _downsampling;

			private bool _ambientOnly;

			private int _pixelWidth;

			private int _pixelHeight;

			public bool CheckNeedsReset(Settings setting, Camera camera)
			{
				if (_blurIterations == setting.blurIterations && _downsampling == setting.downsampling && _ambientOnly == setting.ambientOnly && _pixelWidth == camera.pixelWidth)
				{
					return _pixelHeight != camera.pixelHeight;
				}
				return true;
			}

			public void Update(Settings setting, Camera camera)
			{
				_blurIterations = setting.blurIterations;
				_downsampling = setting.downsampling;
				_ambientOnly = setting.ambientOnly;
				_pixelWidth = camera.pixelWidth;
				_pixelHeight = camera.pixelHeight;
			}
		}

		public enum SampleCount
		{
			Lowest = 0,
			Low = 1,
			Medium = 2,
			High = 3,
			Variable = 4
		}

		[Serializable]
		public class Settings
		{
			[SerializeField]
			[Range(0f, 4f)]
			[Tooltip("Degree of darkness produced by the effect.")]
			public float intensity;

			[SerializeField]
			[Tooltip("Radius of sample points, which affects extent of darkened areas.")]
			public float radius;

			[SerializeField]
			[Tooltip("Number of sample points, which affects quality and performance.")]
			public SampleCount sampleCount;

			[SerializeField]
			[Tooltip("Determines the sample count when SampleCount.Variable is used.")]
			public int sampleCountValue;

			[SerializeField]
			[Range(0f, 4f)]
			[Tooltip("Number of iterations of the blur filter.")]
			public int blurIterations;

			[SerializeField]
			[Tooltip("Halves the resolution of the effect to increase performance.")]
			public bool downsampling;

			[SerializeField]
			[Tooltip("If checked, the effect only affects ambient lighting.")]
			public bool ambientOnly;

			[SerializeField]
			public bool debug;

			public static Settings defaultSettings
			{
				get
				{
					return new Settings
					{
						intensity = 1f,
						radius = 0.3f,
						sampleCount = SampleCount.Medium,
						sampleCountValue = 24,
						blurIterations = 2,
						downsampling = false,
						ambientOnly = false
					};
				}
			}
		}

		private Camera targetCamera;

		[SerializeField]
		public Settings settings = Settings.defaultSettings;

		[SerializeField]
		private Shader _aoShader;

		private Material _aoMaterial;

		private CommandBuffer _aoCommands;

		[SerializeField]
		private Mesh _quadMesh;

		public bool isAmbientOnlySupported
		{
			get
			{
				if (targetCamera.allowHDR)
				{
					return isGBufferAvailable;
				}
				return false;
			}
		}

		private float intensity
		{
			get
			{
				return settings.intensity;
			}
		}

		private float radius
		{
			get
			{
				return Mathf.Max(settings.radius, 0.0001f);
			}
		}

		private SampleCount sampleCount
		{
			get
			{
				return settings.sampleCount;
			}
		}

		private int sampleCountValue
		{
			get
			{
				switch (settings.sampleCount)
				{
				case SampleCount.Lowest:
					return 3;
				case SampleCount.Low:
					return 6;
				case SampleCount.Medium:
					return 12;
				case SampleCount.High:
					return 20;
				default:
					return Mathf.Clamp(settings.sampleCountValue, 1, 256);
				}
			}
		}

		private int blurIterations
		{
			get
			{
				return settings.blurIterations;
			}
		}

		private bool downsampling
		{
			get
			{
				return settings.downsampling;
			}
		}

		private bool ambientOnly
		{
			get
			{
				if (settings.ambientOnly)
				{
					return isAmbientOnlySupported;
				}
				return false;
			}
		}

		private Shader aoShader
		{
			get
			{
				if (_aoShader == null)
				{
					_aoShader = Shader.Find("Hidden/Image Effects/Cinematic/AmbientOcclusion");
				}
				return _aoShader;
			}
		}

		private Material aoMaterial
		{
			get
			{
				if (_aoMaterial == null)
				{
					_aoMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(aoShader);
				}
				return _aoMaterial;
			}
		}

		private CommandBuffer aoCommands
		{
			get
			{
				if (_aoCommands == null)
				{
					_aoCommands = new CommandBuffer();
					_aoCommands.name = "AmbientOcclusion";
				}
				return _aoCommands;
			}
		}

		private PropertyObserver propertyObserver { get; set; }

		private bool isGBufferAvailable
		{
			get
			{
				return targetCamera.actualRenderingPath == RenderingPath.DeferredShading;
			}
		}

		private Mesh quadMesh
		{
			get
			{
				return _quadMesh;
			}
		}

		private void Awake()
		{
			targetCamera = GetComponent<Camera>();
		}

		private void BuildAOCommands()
		{
			CommandBuffer commandBuffer = aoCommands;
			int num = targetCamera.pixelWidth;
			int num2 = targetCamera.pixelHeight;
			RenderTextureFormat format = RenderTextureFormat.R8;
			RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;
			if (downsampling)
			{
				num /= 2;
				num2 /= 2;
			}
			Material material = aoMaterial;
			int num3 = Shader.PropertyToID("_OcclusionTexture");
			commandBuffer.GetTemporaryRT(num3, num, num2, 0, FilterMode.Bilinear, format, readWrite);
			commandBuffer.Blit(null, num3, material, 0);
			if (blurIterations > 0)
			{
				int num4 = Shader.PropertyToID("_OcclusionBlurTexture");
				commandBuffer.GetTemporaryRT(num4, num, num2, 0, FilterMode.Bilinear, format, readWrite);
				for (int i = 0; i < blurIterations; i++)
				{
					commandBuffer.SetGlobalVector("_BlurVector", Vector2.right);
					commandBuffer.Blit(num3, num4, material, 1);
					commandBuffer.SetGlobalVector("_BlurVector", Vector2.up);
					commandBuffer.Blit(num4, num3, material, 1);
				}
				commandBuffer.ReleaseTemporaryRT(num4);
			}
			RenderTargetIdentifier[] colors = new RenderTargetIdentifier[2]
			{
				BuiltinRenderTextureType.GBuffer0,
				BuiltinRenderTextureType.CameraTarget
			};
			commandBuffer.SetRenderTarget(colors, BuiltinRenderTextureType.CameraTarget);
			commandBuffer.DrawMesh(quadMesh, Matrix4x4.identity, material, 0, 3);
			commandBuffer.ReleaseTemporaryRT(num3);
		}

		private void ExecuteAOPass(RenderTexture source, RenderTexture destination)
		{
			int num = source.width;
			int num2 = source.height;
			RenderTextureFormat format = RenderTextureFormat.R8;
			RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;
			if (downsampling)
			{
				num /= 2;
				num2 /= 2;
			}
			Material material = aoMaterial;
			RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, format, readWrite);
			Graphics.Blit(null, temporary, material, 0);
			if (blurIterations > 0)
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(num, num2, 0, format, readWrite);
				for (int i = 0; i < blurIterations; i++)
				{
					material.SetVector("_BlurVector", Vector2.right);
					Graphics.Blit(temporary, temporary2, material, 1);
					material.SetVector("_BlurVector", Vector2.up);
					Graphics.Blit(temporary2, temporary, material, 1);
				}
				RenderTexture.ReleaseTemporary(temporary2);
			}
			material.SetTexture("_OcclusionTexture", temporary);
			if (!settings.debug)
			{
				Graphics.Blit(source, destination, material, 2);
			}
			else
			{
				Graphics.Blit(source, destination, material, 4);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}

		private void UpdateMaterialProperties()
		{
			Material material = aoMaterial;
			material.shaderKeywords = null;
			material.SetFloat("_Intensity", intensity);
			material.SetFloat("_Radius", radius);
			material.SetFloat("_TargetScale", downsampling ? 0.5f : 1f);
			if (isGBufferAvailable)
			{
				material.EnableKeyword("_SOURCE_GBUFFER");
			}
			if (sampleCount == SampleCount.Lowest)
			{
				material.EnableKeyword("_SAMPLECOUNT_LOWEST");
			}
			else
			{
				material.SetInt("_SampleCount", sampleCountValue);
			}
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(aoShader, true, false, this))
			{
				base.enabled = false;
				return;
			}
			if (ambientOnly)
			{
				targetCamera.AddCommandBuffer(CameraEvent.BeforeReflections, aoCommands);
			}
			if (!isGBufferAvailable)
			{
				targetCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
			}
		}

		private void OnDisable()
		{
			if (_aoMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(_aoMaterial);
			}
			_aoMaterial = null;
			if (_aoCommands != null)
			{
				targetCamera.RemoveCommandBuffer(CameraEvent.BeforeReflections, _aoCommands);
			}
			_aoCommands = null;
		}

		private void Update()
		{
			if (propertyObserver.CheckNeedsReset(settings, targetCamera))
			{
				OnDisable();
				OnEnable();
				if (ambientOnly)
				{
					aoCommands.Clear();
					BuildAOCommands();
				}
				propertyObserver.Update(settings, targetCamera);
			}
			if (ambientOnly)
			{
				UpdateMaterialProperties();
			}
		}

		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (ambientOnly)
			{
				Graphics.Blit(source, destination);
				return;
			}
			UpdateMaterialProperties();
			ExecuteAOPass(source, destination);
		}
	}
}
