using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Noise/Noise And Grain (Filmic)")]
	public class NoiseAndGrain : PostEffectsBase
	{
		public float intensityMultiplier = 0.25f;

		public float generalIntensity = 0.5f;

		public float blackIntensity = 1f;

		public float whiteIntensity = 1f;

		public float midGrey = 0.2f;

		public bool dx11Grain;

		public float softness;

		public bool monochrome;

		public Vector3 intensities = new Vector3(1f, 1f, 1f);

		public Vector3 tiling = new Vector3(64f, 64f, 64f);

		public float monochromeTiling = 64f;

		public FilterMode filterMode = FilterMode.Bilinear;

		public Texture2D noiseTexture;

		public Shader noiseShader;

		private Material noiseMaterial;

		public Shader dx11NoiseShader;

		private Material dx11NoiseMaterial;

		private static float TILE_AMOUNT = 64f;

		private Mesh mesh;

		private void Awake()
		{
			mesh = new Mesh();
		}

		public override bool CheckResources()
		{
			CheckSupport(false);
			noiseMaterial = CheckShaderAndCreateMaterial(noiseShader, noiseMaterial);
			if (dx11Grain && supportDX11)
			{
				dx11NoiseMaterial = CheckShaderAndCreateMaterial(dx11NoiseShader, dx11NoiseMaterial);
			}
			if (!isSupported)
			{
				ReportAutoDisable();
			}
			return isSupported;
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!CheckResources() || null == noiseTexture)
			{
				Graphics.Blit(source, destination);
				if (null == noiseTexture)
				{
					Debug.LogWarning("Noise & Grain effect failing as noise texture is not assigned. please assign.", base.transform);
				}
				return;
			}
			softness = Mathf.Clamp(softness, 0f, 0.99f);
			if (dx11Grain && supportDX11)
			{
				dx11NoiseMaterial.SetFloat("_DX11NoiseTime", Time.frameCount);
				dx11NoiseMaterial.SetTexture("_NoiseTex", noiseTexture);
				dx11NoiseMaterial.SetVector("_NoisePerChannel", monochrome ? Vector3.one : intensities);
				dx11NoiseMaterial.SetVector("_MidGrey", new Vector3(midGrey, 1f / (1f - midGrey), -1f / midGrey));
				dx11NoiseMaterial.SetVector("_NoiseAmount", new Vector3(generalIntensity, blackIntensity, whiteIntensity) * intensityMultiplier);
				if (softness > Mathf.Epsilon)
				{
					RenderTexture temporary = RenderTexture.GetTemporary((int)((float)source.width * (1f - softness)), (int)((float)source.height * (1f - softness)));
					DrawNoiseQuadGrid(source, temporary, dx11NoiseMaterial, noiseTexture, mesh, monochrome ? 3 : 2);
					dx11NoiseMaterial.SetTexture("_NoiseTex", temporary);
					Graphics.Blit(source, destination, dx11NoiseMaterial, 4);
					RenderTexture.ReleaseTemporary(temporary);
				}
				else
				{
					DrawNoiseQuadGrid(source, destination, dx11NoiseMaterial, noiseTexture, mesh, monochrome ? 1 : 0);
				}
				return;
			}
			if ((bool)noiseTexture)
			{
				noiseTexture.wrapMode = TextureWrapMode.Repeat;
				noiseTexture.filterMode = filterMode;
			}
			noiseMaterial.SetTexture("_NoiseTex", noiseTexture);
			noiseMaterial.SetVector("_NoisePerChannel", monochrome ? Vector3.one : intensities);
			noiseMaterial.SetVector("_NoiseTilingPerChannel", monochrome ? (Vector3.one * monochromeTiling) : tiling);
			noiseMaterial.SetVector("_MidGrey", new Vector3(midGrey, 1f / (1f - midGrey), -1f / midGrey));
			noiseMaterial.SetVector("_NoiseAmount", new Vector3(generalIntensity, blackIntensity, whiteIntensity) * intensityMultiplier);
			if (softness > Mathf.Epsilon)
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary((int)((float)source.width * (1f - softness)), (int)((float)source.height * (1f - softness)));
				DrawNoiseQuadGrid(source, temporary2, noiseMaterial, noiseTexture, mesh, 2);
				noiseMaterial.SetTexture("_NoiseTex", temporary2);
				Graphics.Blit(source, destination, noiseMaterial, 1);
				RenderTexture.ReleaseTemporary(temporary2);
			}
			else
			{
				DrawNoiseQuadGrid(source, destination, noiseMaterial, noiseTexture, mesh, 0);
			}
		}

		private static void DrawNoiseQuadGrid(RenderTexture source, RenderTexture dest, Material fxMaterial, Texture2D noise, Mesh mesh, int passNr)
		{
			RenderTexture.active = dest;
			fxMaterial.SetTexture("_MainTex", source);
			GL.PushMatrix();
			GL.LoadOrtho();
			fxMaterial.SetPass(passNr);
			BuildMesh(mesh, source, noise);
			Transform obj = Camera.main.transform;
			Vector3 position = obj.position;
			Quaternion rotation = obj.rotation;
			obj.position = Vector3.zero;
			obj.rotation = Quaternion.identity;
			Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
			obj.position = position;
			obj.rotation = rotation;
			GL.PopMatrix();
		}

		private static void BuildMesh(Mesh mesh, RenderTexture source, Texture2D noise)
		{
			float noiseSize = (float)noise.width * 1f;
			float num = 1f * (float)source.width / TILE_AMOUNT;
			float num2 = 1f * (float)source.width / (1f * (float)source.height);
			float num3 = 1f / num;
			float num4 = num3 * num2;
			int num5 = (int)Mathf.Ceil(num);
			int num6 = (int)Mathf.Ceil(1f / num4);
			if (mesh.vertices.Length != num5 * num6 * 4)
			{
				Vector3[] array = new Vector3[num5 * num6 * 4];
				Vector2[] array2 = new Vector2[num5 * num6 * 4];
				int[] array3 = new int[num5 * num6 * 6];
				int num7 = 0;
				int num8 = 0;
				for (float num9 = 0f; num9 < 1f; num9 += num3)
				{
					for (float num10 = 0f; num10 < 1f; num10 += num4)
					{
						array[num7] = new Vector3(num9, num10, 0.1f);
						array[num7 + 1] = new Vector3(num9 + num3, num10, 0.1f);
						array[num7 + 2] = new Vector3(num9 + num3, num10 + num4, 0.1f);
						array[num7 + 3] = new Vector3(num9, num10 + num4, 0.1f);
						array2[num7] = new Vector2(0f, 0f);
						array2[num7 + 1] = new Vector2(1f, 0f);
						array2[num7 + 2] = new Vector2(1f, 1f);
						array2[num7 + 3] = new Vector2(0f, 1f);
						array3[num8] = num7;
						array3[num8 + 1] = num7 + 1;
						array3[num8 + 2] = num7 + 2;
						array3[num8 + 3] = num7;
						array3[num8 + 4] = num7 + 2;
						array3[num8 + 5] = num7 + 3;
						num7 += 4;
						num8 += 6;
					}
				}
				mesh.vertices = array;
				mesh.uv2 = array2;
				mesh.triangles = array3;
			}
			BuildMeshUV0(mesh, num5, num6, noiseSize, noise.width);
		}

		private static void BuildMeshUV0(Mesh mesh, int width, int height, float noiseSize, int noiseWidth)
		{
			float num = noiseSize / ((float)noiseWidth * 1f);
			float num2 = 1f / noiseSize;
			Vector2[] array = new Vector2[width * height * 4];
			int num3 = 0;
			for (int i = 0; i < width * height; i++)
			{
				float f = Random.Range(0f, noiseSize);
				float f2 = Random.Range(0f, noiseSize);
				f = Mathf.Floor(f) * num2;
				f2 = Mathf.Floor(f2) * num2;
				array[num3] = new Vector2(f, f2);
				array[num3 + 1] = new Vector2(f + num * num2, f2);
				array[num3 + 2] = new Vector2(f + num * num2, f2 + num * num2);
				array[num3 + 3] = new Vector2(f, f2 + num * num2);
				num3 += 4;
			}
			mesh.uv = array;
		}
	}
}
