using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TMPro.Examples
{
	public class VertexZoom : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass10_0
		{
			public List<float> modifiedCharScale;

			public Comparison<int> _003C_003E9__0;

			internal int _003CAnimateVertexColors_003Eb__0(int a, int b)
			{
				return modifiedCharScale[a].CompareTo(modifiedCharScale[b]);
			}
		}

		public float AngleMultiplier = 1f;

		public float SpeedMultiplier = 1f;

		public float CurveScale = 1f;

		private TMP_Text m_TextComponent;

		private bool hasTextChanged;

		private void Awake()
		{
			m_TextComponent = GetComponent<TMP_Text>();
		}

		private void OnEnable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
		}

		private void OnDisable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
		}

		private void Start()
		{
			StartCoroutine(AnimateVertexColors());
		}

		private void ON_TEXT_CHANGED(UnityEngine.Object obj)
		{
			if (obj == m_TextComponent)
			{
				hasTextChanged = true;
			}
		}

		private IEnumerator AnimateVertexColors()
		{
			_003C_003Ec__DisplayClass10_0 _003C_003Ec__DisplayClass10_ = new _003C_003Ec__DisplayClass10_0();
			m_TextComponent.ForceMeshUpdate();
			TMP_TextInfo textInfo = m_TextComponent.textInfo;
			TMP_MeshInfo[] cachedMeshInfoVertexData = textInfo.CopyMeshInfoVertexData();
			_003C_003Ec__DisplayClass10_.modifiedCharScale = new List<float>();
			List<int> scaleSortingOrder = new List<int>();
			hasTextChanged = true;
			while (true)
			{
				if (hasTextChanged)
				{
					cachedMeshInfoVertexData = textInfo.CopyMeshInfoVertexData();
					hasTextChanged = false;
				}
				int characterCount = textInfo.characterCount;
				if (characterCount == 0)
				{
					yield return new WaitForSeconds(0.25f);
					continue;
				}
				_003C_003Ec__DisplayClass10_.modifiedCharScale.Clear();
				scaleSortingOrder.Clear();
				for (int i = 0; i < characterCount; i++)
				{
					if (textInfo.characterInfo[i].isVisible)
					{
						int materialReferenceIndex = textInfo.characterInfo[i].materialReferenceIndex;
						int vertexIndex = textInfo.characterInfo[i].vertexIndex;
						Vector3[] vertices = cachedMeshInfoVertexData[materialReferenceIndex].vertices;
						Vector3 vector = (Vector2)((vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2f);
						Vector3[] vertices2 = textInfo.meshInfo[materialReferenceIndex].vertices;
						vertices2[vertexIndex] = vertices[vertexIndex] - vector;
						vertices2[vertexIndex + 1] = vertices[vertexIndex + 1] - vector;
						vertices2[vertexIndex + 2] = vertices[vertexIndex + 2] - vector;
						vertices2[vertexIndex + 3] = vertices[vertexIndex + 3] - vector;
						float num = UnityEngine.Random.Range(1f, 1.5f);
						_003C_003Ec__DisplayClass10_.modifiedCharScale.Add(num);
						scaleSortingOrder.Add(_003C_003Ec__DisplayClass10_.modifiedCharScale.Count - 1);
						Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0f, 0f, 0f), Quaternion.identity, Vector3.one * num);
						vertices2[vertexIndex] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex]);
						vertices2[vertexIndex + 1] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex + 1]);
						vertices2[vertexIndex + 2] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex + 2]);
						vertices2[vertexIndex + 3] = matrix4x.MultiplyPoint3x4(vertices2[vertexIndex + 3]);
						vertices2[vertexIndex] += vector;
						vertices2[vertexIndex + 1] += vector;
						vertices2[vertexIndex + 2] += vector;
						vertices2[vertexIndex + 3] += vector;
						Vector2[] uvs = cachedMeshInfoVertexData[materialReferenceIndex].uvs0;
						Vector2[] uvs2 = textInfo.meshInfo[materialReferenceIndex].uvs0;
						uvs2[vertexIndex] = uvs[vertexIndex];
						uvs2[vertexIndex + 1] = uvs[vertexIndex + 1];
						uvs2[vertexIndex + 2] = uvs[vertexIndex + 2];
						uvs2[vertexIndex + 3] = uvs[vertexIndex + 3];
						Color32[] colors = cachedMeshInfoVertexData[materialReferenceIndex].colors32;
						Color32[] colors2 = textInfo.meshInfo[materialReferenceIndex].colors32;
						colors2[vertexIndex] = colors[vertexIndex];
						colors2[vertexIndex + 1] = colors[vertexIndex + 1];
						colors2[vertexIndex + 2] = colors[vertexIndex + 2];
						colors2[vertexIndex + 3] = colors[vertexIndex + 3];
					}
				}
				for (int j = 0; j < textInfo.meshInfo.Length; j++)
				{
					scaleSortingOrder.Sort(_003C_003Ec__DisplayClass10_._003C_003E9__0 ?? (_003C_003Ec__DisplayClass10_._003C_003E9__0 = _003C_003Ec__DisplayClass10_._003CAnimateVertexColors_003Eb__0));
					textInfo.meshInfo[j].SortGeometry(scaleSortingOrder);
					textInfo.meshInfo[j].mesh.vertices = textInfo.meshInfo[j].vertices;
					textInfo.meshInfo[j].mesh.uv = textInfo.meshInfo[j].uvs0;
					textInfo.meshInfo[j].mesh.colors32 = textInfo.meshInfo[j].colors32;
					m_TextComponent.UpdateGeometry(textInfo.meshInfo[j].mesh, j);
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}
