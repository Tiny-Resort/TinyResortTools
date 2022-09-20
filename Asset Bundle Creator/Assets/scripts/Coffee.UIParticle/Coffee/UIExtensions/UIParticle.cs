using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[ExecuteInEditMode]
	public class UIParticle : MaskableGraphic
	{
		[Serializable]
		public class AnimatableProperty : ISerializationCallbackReceiver
		{
			public enum ShaderPropertyType
			{
				Color = 0,
				Vector = 1,
				Float = 2,
				Range = 3,
				Texture = 4
			}

			[SerializeField]
			private string m_Name = "";

			[SerializeField]
			private ShaderPropertyType m_Type = ShaderPropertyType.Vector;

			public int id { get; private set; }

			public ShaderPropertyType type
			{
				get
				{
					return m_Type;
				}
			}

			public void OnBeforeSerialize()
			{
			}

			public void OnAfterDeserialize()
			{
				id = Shader.PropertyToID(m_Name);
			}
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Predicate<UIParticle> _003C_003E9__48_0;

			internal bool _003CSetParent_003Eb__48_0(UIParticle x)
			{
				return x == null;
			}
		}

		private static readonly int s_IdMainTex = Shader.PropertyToID("_MainTex");

		private static readonly List<Vector3> s_Vertices = new List<Vector3>();

		private static readonly List<Color32> s_Colors = new List<Color32>();

		private static readonly List<UIParticle> s_TempRelatables = new List<UIParticle>();

		private static readonly List<UIParticle> s_ActiveParticles = new List<UIParticle>();

		[Tooltip("The ParticleSystem rendered by CanvasRenderer")]
		[SerializeField]
		private ParticleSystem m_ParticleSystem;

		[Tooltip("The UIParticle to render trail effect")]
		[SerializeField]
		private UIParticle m_TrailParticle;

		[HideInInspector]
		[SerializeField]
		private bool m_IsTrail;

		[Tooltip("Particle effect scale")]
		[SerializeField]
		private float m_Scale = 1f;

		[Tooltip("Ignore parent scale")]
		[SerializeField]
		private bool m_IgnoreParent;

		[Tooltip("Animatable material properties. AnimationでParticleSystemのマテリアルプロパティを変更する場合、有効にしてください。")]
		[SerializeField]
		private AnimatableProperty[] m_AnimatableProperties = new AnimatableProperty[0];

		private static MaterialPropertyBlock s_Mpb;

		private Mesh _mesh;

		private ParticleSystemRenderer _renderer;

		private UIParticle _parent;

		private List<UIParticle> _children = new List<UIParticle>();

		private Matrix4x4 scaleaMatrix;

		private Vector3 _oldPos;

		private static readonly Vector3 minimumVec3 = new Vector3(1E-07f, 1E-07f, 1E-07f);

		private static ParticleSystem.Particle[] s_Particles = new ParticleSystem.Particle[4096];

		public override Texture mainTexture
		{
			get
			{
				Texture texture = null;
				if (!m_IsTrail && (bool)cachedParticleSystem)
				{
					ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = cachedParticleSystem.textureSheetAnimation;
					if (textureSheetAnimation.enabled && textureSheetAnimation.mode == ParticleSystemAnimationMode.Sprites && 0 < textureSheetAnimation.spriteCount)
					{
						Sprite sprite = textureSheetAnimation.GetSprite(0);
						textureSheetAnimation.uvChannelMask = (((bool)sprite && sprite.packed) ? ((UVChannelFlags)(-1)) : ((UVChannelFlags)0));
						texture = (sprite ? sprite.texture : null);
					}
				}
				if (!texture && (bool)_renderer)
				{
					Material material = this.material;
					if ((bool)material && material.HasProperty(s_IdMainTex))
					{
						texture = material.mainTexture;
					}
				}
				return texture ?? Graphic.s_WhiteTexture;
			}
		}

		public override Material material
		{
			get
			{
				if (!_renderer)
				{
					return null;
				}
				if (!m_IsTrail)
				{
					return _renderer.sharedMaterial;
				}
				return _renderer.trailMaterial;
			}
			set
			{
				if ((bool)_renderer)
				{
					if (m_IsTrail && _renderer.trailMaterial != value)
					{
						_renderer.trailMaterial = value;
						SetMaterialDirty();
					}
					else if (!m_IsTrail && _renderer.sharedMaterial != value)
					{
						_renderer.sharedMaterial = value;
						SetMaterialDirty();
					}
				}
			}
		}

		public float scale
		{
			get
			{
				if (!_parent)
				{
					return m_Scale;
				}
				return _parent.scale;
			}
			set
			{
				m_Scale = value;
			}
		}

		public bool ignoreParent
		{
			get
			{
				return m_IgnoreParent;
			}
			set
			{
				if (m_IgnoreParent != value)
				{
					m_IgnoreParent = value;
					OnTransformParentChanged();
				}
			}
		}

		public bool isRoot
		{
			get
			{
				return !_parent;
			}
		}

		public override bool raycastTarget
		{
			get
			{
				return false;
			}
			set
			{
				base.raycastTarget = value;
			}
		}

		public ParticleSystem cachedParticleSystem
		{
			get
			{
				if (!m_ParticleSystem)
				{
					return m_ParticleSystem = GetComponent<ParticleSystem>();
				}
				return m_ParticleSystem;
			}
		}

		public override Material GetModifiedMaterial(Material baseMaterial)
		{
			Material material = null;
			material = ((!_renderer) ? baseMaterial : ((m_AnimatableProperties.Length != 0) ? new Material(this.material) : _renderer.sharedMaterial));
			return base.GetModifiedMaterial(material);
		}

		protected override void OnEnable()
		{
			if (s_ActiveParticles.Count == 0)
			{
				Canvas.willRenderCanvases += UpdateMeshes;
				s_Mpb = new MaterialPropertyBlock();
			}
			s_ActiveParticles.Add(this);
			GetComponentsInChildren(false, s_TempRelatables);
			int num = s_TempRelatables.Count - 1;
			while (0 <= num)
			{
				s_TempRelatables[num].OnTransformParentChanged();
				num--;
			}
			s_TempRelatables.Clear();
			_renderer = (cachedParticleSystem ? cachedParticleSystem.GetComponent<ParticleSystemRenderer>() : null);
			if ((bool)_renderer && Application.isPlaying)
			{
				_renderer.enabled = false;
			}
			_mesh = new Mesh();
			_mesh.MarkDynamic();
			CheckTrail();
			if ((bool)cachedParticleSystem)
			{
				_oldPos = ((cachedParticleSystem.main.scalingMode == ParticleSystemScalingMode.Local) ? base.rectTransform.localPosition : base.rectTransform.position);
			}
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			s_ActiveParticles.Remove(this);
			if (s_ActiveParticles.Count == 0)
			{
				Canvas.willRenderCanvases -= UpdateMeshes;
			}
			int num = _children.Count - 1;
			while (0 <= num)
			{
				_children[num].SetParent(_parent);
				num--;
			}
			_children.Clear();
			SetParent(null);
			UnityEngine.Object.DestroyImmediate(_mesh);
			_mesh = null;
			CheckTrail();
			base.OnDisable();
		}

		protected override void UpdateGeometry()
		{
		}

		protected override void OnTransformParentChanged()
		{
			UIParticle uIParticle = null;
			if (base.isActiveAndEnabled && !m_IgnoreParent)
			{
				Transform parent = base.transform.parent;
				int num = 0;
				while ((bool)parent && (!uIParticle || !uIParticle.enabled))
				{
					uIParticle = parent.GetComponent<UIParticle>();
					parent = parent.parent;
					num++;
					if (num >= 40)
					{
						break;
					}
				}
			}
			SetParent(uIParticle);
			base.OnTransformParentChanged();
		}

		protected override void OnDidApplyAnimationProperties()
		{
		}

		private static void UpdateMeshes()
		{
			for (int i = 0; i < s_ActiveParticles.Count; i++)
			{
				if ((bool)s_ActiveParticles[i])
				{
					s_ActiveParticles[i].UpdateMesh();
				}
			}
		}

		private void UpdateMesh()
		{
			try
			{
				CheckTrail();
				if (!m_ParticleSystem || !base.canvas)
				{
					return;
				}
				Vector3 localPosition = base.rectTransform.localPosition;
				if (Mathf.Abs(localPosition.z) < 0.01f)
				{
					localPosition.z = 0.01f;
					base.rectTransform.localPosition = localPosition;
				}
				Canvas rootCanvas = base.canvas.rootCanvas;
				if (Application.isPlaying)
				{
					_renderer.enabled = false;
				}
				if ((_renderer.renderMode == ParticleSystemRenderMode.Mesh && !_renderer.mesh) || _renderer.renderMode == ParticleSystemRenderMode.None)
				{
					return;
				}
				ParticleSystem.MainModule main = m_ParticleSystem.main;
				scaleaMatrix = ((main.scalingMode == ParticleSystemScalingMode.Hierarchy) ? Matrix4x4.Scale(scale * Vector3.one) : Matrix4x4.Scale(scale * rootCanvas.transform.localScale));
				Matrix4x4 matrix4x = default(Matrix4x4);
				switch (main.simulationSpace)
				{
				case ParticleSystemSimulationSpace.Local:
					matrix4x = scaleaMatrix * Matrix4x4.Rotate(base.rectTransform.rotation).inverse * Matrix4x4.Scale(base.rectTransform.lossyScale + minimumVec3).inverse;
					break;
				case ParticleSystemSimulationSpace.World:
				{
					matrix4x = scaleaMatrix * base.rectTransform.worldToLocalMatrix;
					bool flag = main.scalingMode == ParticleSystemScalingMode.Local;
					Vector3 position = base.rectTransform.position;
					Vector3 vector = position - _oldPos;
					_oldPos = position;
					if (!Mathf.Approximately(scale, 0f) && 0f < vector.sqrMagnitude)
					{
						if (flag)
						{
							Vector3 vector2 = rootCanvas.transform.localScale * scale;
							vector.x *= 1f - 1f / vector2.x;
							vector.y *= 1f - 1f / vector2.y;
							vector.z *= 1f - 1f / vector2.z;
						}
						else
						{
							vector *= 1f - 1f / scale;
						}
						int particleCount = m_ParticleSystem.particleCount;
						if (s_Particles.Length < particleCount)
						{
							s_Particles = new ParticleSystem.Particle[s_Particles.Length * 2];
						}
						m_ParticleSystem.GetParticles(s_Particles);
						for (int i = 0; i < particleCount; i++)
						{
							ParticleSystem.Particle particle = s_Particles[i];
							particle.position += vector;
							s_Particles[i] = particle;
						}
						m_ParticleSystem.SetParticles(s_Particles, particleCount);
					}
					break;
				}
				}
				_mesh.Clear();
				if (0 < m_ParticleSystem.particleCount)
				{
					Camera camera = ((rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? UIParticleOverlayCamera.GetCameraForOvrelay(rootCanvas) : (base.canvas.worldCamera ?? Camera.main));
					if (!camera)
					{
						return;
					}
					if (m_IsTrail)
					{
						_renderer.BakeTrailsMesh(_mesh, camera, true);
					}
					else
					{
						_renderer.BakeMesh(_mesh, camera, true);
					}
					if (QualitySettings.activeColorSpace == ColorSpace.Linear)
					{
						_mesh.GetColors(s_Colors);
						int count = s_Colors.Count;
						for (int j = 0; j < count; j++)
						{
							s_Colors[j] = ((Color)s_Colors[j]).gamma;
						}
						_mesh.SetColors(s_Colors);
					}
					_mesh.GetVertices(s_Vertices);
					int count2 = s_Vertices.Count;
					for (int k = 0; k < count2; k++)
					{
						s_Vertices[k] = matrix4x.MultiplyPoint3x4(s_Vertices[k]);
					}
					_mesh.SetVertices(s_Vertices);
					_mesh.RecalculateBounds();
					s_Vertices.Clear();
					s_Colors.Clear();
				}
				base.canvasRenderer.SetMesh(_mesh);
				base.canvasRenderer.SetTexture(mainTexture);
				UpdateAnimatableMaterialProperties();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private void CheckTrail()
		{
			if (base.isActiveAndEnabled && !m_IsTrail && (bool)m_ParticleSystem && m_ParticleSystem.trails.enabled)
			{
				if (!m_TrailParticle)
				{
					m_TrailParticle = new GameObject("[UIParticle] Trail").AddComponent<UIParticle>();
					Transform obj = m_TrailParticle.transform;
					obj.SetParent(base.transform);
					obj.localPosition = Vector3.zero;
					obj.localRotation = Quaternion.identity;
					obj.localScale = Vector3.one;
					m_TrailParticle._renderer = GetComponent<ParticleSystemRenderer>();
					m_TrailParticle.m_ParticleSystem = GetComponent<ParticleSystem>();
					m_TrailParticle.m_IsTrail = true;
				}
				m_TrailParticle.enabled = true;
			}
			else if ((bool)m_TrailParticle)
			{
				m_TrailParticle.enabled = false;
			}
		}

		private void SetParent(UIParticle newParent)
		{
			if (_parent != newParent && this != newParent)
			{
				if ((bool)_parent && _parent._children.Contains(this))
				{
					_parent._children.Remove(this);
					_parent._children.RemoveAll(_003C_003Ec._003C_003E9__48_0 ?? (_003C_003Ec._003C_003E9__48_0 = _003C_003Ec._003C_003E9._003CSetParent_003Eb__48_0));
				}
				_parent = newParent;
			}
			if ((bool)_parent && !_parent._children.Contains(this))
			{
				_parent._children.Add(this);
			}
		}

		private void UpdateAnimatableMaterialProperties()
		{
			if (m_AnimatableProperties.Length == 0)
			{
				return;
			}
			_renderer.GetPropertyBlock(s_Mpb);
			for (int i = 0; i < base.canvasRenderer.materialCount; i++)
			{
				Material material = base.canvasRenderer.GetMaterial(i);
				AnimatableProperty[] animatableProperties = m_AnimatableProperties;
				foreach (AnimatableProperty animatableProperty in animatableProperties)
				{
					switch (animatableProperty.type)
					{
					case AnimatableProperty.ShaderPropertyType.Color:
						material.SetColor(animatableProperty.id, s_Mpb.GetColor(animatableProperty.id));
						break;
					case AnimatableProperty.ShaderPropertyType.Vector:
						material.SetVector(animatableProperty.id, s_Mpb.GetVector(animatableProperty.id));
						break;
					case AnimatableProperty.ShaderPropertyType.Float:
					case AnimatableProperty.ShaderPropertyType.Range:
						material.SetFloat(animatableProperty.id, s_Mpb.GetFloat(animatableProperty.id));
						break;
					case AnimatableProperty.ShaderPropertyType.Texture:
						material.SetTexture(animatableProperty.id, s_Mpb.GetTexture(animatableProperty.id));
						break;
					}
				}
			}
		}
	}
}
