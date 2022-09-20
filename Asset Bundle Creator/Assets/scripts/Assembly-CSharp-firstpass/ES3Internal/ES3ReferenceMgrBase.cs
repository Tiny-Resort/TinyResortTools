using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ES3Internal
{
	[Serializable]
	[DisallowMultipleComponent]
	public abstract class ES3ReferenceMgrBase : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass30_0
		{
			public UnityEngine.Object obj;

			public Func<KeyValuePair<long, UnityEngine.Object>, bool> _003C_003E9__0;

			internal bool _003CRemove_003Eb__0(KeyValuePair<long, UnityEngine.Object> kvp)
			{
				return kvp.Value == obj;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass31_0
		{
			public long referenceID;

			public Func<KeyValuePair<UnityEngine.Object, long>, bool> _003C_003E9__0;

			internal bool _003CRemove_003Eb__0(KeyValuePair<UnityEngine.Object, long> kvp)
			{
				return kvp.Value == referenceID;
			}
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Func<KeyValuePair<long, UnityEngine.Object>, bool> _003C_003E9__32_0;

			public static Func<KeyValuePair<long, UnityEngine.Object>, long> _003C_003E9__32_1;

			internal bool _003CRemoveNullOrInvalidValues_003Eb__32_0(KeyValuePair<long, UnityEngine.Object> pair)
			{
				if (!(pair.Value == null))
				{
					return !CanBeSaved(pair.Value);
				}
				return true;
			}

			internal long _003CRemoveNullOrInvalidValues_003Eb__32_1(KeyValuePair<long, UnityEngine.Object> pair)
			{
				return pair.Key;
			}
		}

		internal object _lock = new object();

		public const string referencePropertyName = "_ES3Ref";

		private static ES3ReferenceMgrBase _current = null;

		private static HashSet<ES3ReferenceMgrBase> mgrs = new HashSet<ES3ReferenceMgrBase>();

		private static System.Random rng;

		[HideInInspector]
		public bool openPrefabs;

		public List<ES3Prefab> prefabs = new List<ES3Prefab>();

		[SerializeField]
		public ES3IdRefDictionary idRef = new ES3IdRefDictionary();

		private ES3RefIdDictionary _refId;

		public static ES3ReferenceMgrBase Current
		{
			get
			{
				if (_current == null)
				{
					ES3ReferenceMgrBase managerFromScene = GetManagerFromScene(SceneManager.GetActiveScene());
					if (managerFromScene != null)
					{
						mgrs.Add(_current = managerFromScene);
					}
				}
				return _current;
			}
		}

		public bool IsInitialised
		{
			get
			{
				return idRef.Count > 0;
			}
		}

		public ES3RefIdDictionary refId
		{
			get
			{
				if (_refId == null)
				{
					_refId = new ES3RefIdDictionary();
					foreach (KeyValuePair<long, UnityEngine.Object> item in idRef)
					{
						if (item.Value != null)
						{
							_refId[item.Value] = item.Key;
						}
					}
				}
				return _refId;
			}
			set
			{
				_refId = value;
			}
		}

		public ES3GlobalReferences GlobalReferences
		{
			get
			{
				return ES3GlobalReferences.Instance;
			}
		}

		public static ES3ReferenceMgrBase GetManagerFromScene(Scene scene)
		{
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			ES3ReferenceMgr eS3ReferenceMgr = null;
			GameObject[] array = rootGameObjects;
			foreach (GameObject gameObject in array)
			{
				if (gameObject.name == "Easy Save 3 Manager")
				{
					eS3ReferenceMgr = gameObject.GetComponent<ES3ReferenceMgr>();
					break;
				}
			}
			if (eS3ReferenceMgr == null)
			{
				array = rootGameObjects;
				for (int i = 0; i < array.Length; i++)
				{
					if ((eS3ReferenceMgr = array[i].GetComponentInChildren<ES3ReferenceMgr>()) != null)
					{
						break;
					}
				}
			}
			return eS3ReferenceMgr;
		}

		private void Awake()
		{
			if (_current != null && _current != this)
			{
				ES3ReferenceMgrBase current = _current;
				if (Current != null)
				{
					current.Merge(this);
					UnityEngine.Object.Destroy(this);
					_current = current;
				}
			}
			else
			{
				_current = this;
			}
			mgrs.Add(this);
		}

		private void OnDestroy()
		{
			mgrs.Remove(this);
		}

		public void Merge(ES3ReferenceMgrBase otherMgr)
		{
			foreach (KeyValuePair<long, UnityEngine.Object> item in otherMgr.idRef)
			{
				Add(item.Value, item.Key);
			}
		}

		public long Get(UnityEngine.Object obj)
		{
			foreach (ES3ReferenceMgrBase mgr in mgrs)
			{
				if (!(mgr == null))
				{
					if (obj == null)
					{
						return -1L;
					}
					long value;
					if (!mgr.refId.TryGetValue(obj, out value))
					{
						return -1L;
					}
					return value;
				}
			}
			return -1L;
		}

		internal UnityEngine.Object Get(long id, Type type, bool suppressWarnings = false)
		{
			foreach (ES3ReferenceMgrBase mgr in mgrs)
			{
				if (mgr == null)
				{
					continue;
				}
				if (id == -1)
				{
					return null;
				}
				UnityEngine.Object value;
				if (!mgr.idRef.TryGetValue(id, out value))
				{
					if (GlobalReferences != null)
					{
						UnityEngine.Object @object = GlobalReferences.Get(id);
						if (@object != null)
						{
							return @object;
						}
					}
					if (type != null)
					{
						string[] obj = new string[5] { "Reference for ", null, null, null, null };
						obj[1] = (((object)type != null) ? type.ToString() : null);
						obj[2] = " with ID ";
						obj[3] = id.ToString();
						obj[4] = " could not be found in Easy Save's reference manager. If you are loading objects dynamically (i.e. objects created at runtime), this warning is expected and can be ignored.";
						ES3Debug.LogWarning(string.Concat(obj), this);
					}
					else
					{
						ES3Debug.LogWarning("Reference with ID " + id + " could not be found in Easy Save's reference manager. If you are loading objects dynamically (i.e. objects created at runtime), this warning is expected and can be ignored.", this);
					}
					return null;
				}
				if (value == null)
				{
					return null;
				}
				return value;
			}
			return null;
		}

		public UnityEngine.Object Get(long id, bool suppressWarnings = false)
		{
			return Get(id, null, suppressWarnings);
		}

		public ES3Prefab GetPrefab(long id, bool suppressWarnings = false)
		{
			foreach (ES3ReferenceMgrBase mgr in mgrs)
			{
				if (mgr == null)
				{
					continue;
				}
				foreach (ES3Prefab prefab in mgr.prefabs)
				{
					if (prefabs != null && prefab.prefabId == id)
					{
						return prefab;
					}
				}
			}
			if (!suppressWarnings)
			{
				ES3Debug.LogWarning("Prefab with ID " + id + " could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene.", this);
			}
			return null;
		}

		public long GetPrefab(ES3Prefab prefabToFind, bool suppressWarnings = false)
		{
			foreach (ES3ReferenceMgrBase mgr in mgrs)
			{
				if (mgr == null)
				{
					continue;
				}
				foreach (ES3Prefab prefab in prefabs)
				{
					if (prefab == prefabToFind)
					{
						return prefab.prefabId;
					}
				}
			}
			if (!suppressWarnings)
			{
				ES3Debug.LogWarning("Prefab with name " + prefabToFind.name + " could not be found in Easy Save's reference manager. Try pressing the Refresh References button on the ES3ReferenceMgr Component of the Easy Save 3 Manager in your scene.", prefabToFind);
			}
			return -1L;
		}

		public long Add(UnityEngine.Object obj)
		{
			long value;
			if (refId.TryGetValue(obj, out value))
			{
				return value;
			}
			if (GlobalReferences != null)
			{
				value = GlobalReferences.GetOrAdd(obj);
				if (value != -1)
				{
					Add(obj, value);
					return value;
				}
			}
			lock (_lock)
			{
				value = GetNewRefID();
				return Add(obj, value);
			}
		}

		public long Add(UnityEngine.Object obj, long id)
		{
			if (!CanBeSaved(obj))
			{
				return -1L;
			}
			if (id == -1)
			{
				id = GetNewRefID();
			}
			lock (_lock)
			{
				idRef[id] = obj;
				if (obj != null)
				{
					refId[obj] = id;
					return id;
				}
				return id;
			}
		}

		public bool AddPrefab(ES3Prefab prefab)
		{
			if (!prefabs.Contains(prefab))
			{
				prefabs.Add(prefab);
				return true;
			}
			return false;
		}

		public void Remove(UnityEngine.Object obj)
		{
			_003C_003Ec__DisplayClass30_0 _003C_003Ec__DisplayClass30_ = new _003C_003Ec__DisplayClass30_0();
			_003C_003Ec__DisplayClass30_.obj = obj;
			foreach (ES3ReferenceMgrBase mgr in mgrs)
			{
				if (mgr == null)
				{
					continue;
				}
				lock (mgr._lock)
				{
					mgr.refId.Remove(_003C_003Ec__DisplayClass30_.obj);
					foreach (KeyValuePair<long, UnityEngine.Object> item in mgr.idRef.Where(_003C_003Ec__DisplayClass30_._003C_003E9__0 ?? (_003C_003Ec__DisplayClass30_._003C_003E9__0 = _003C_003Ec__DisplayClass30_._003CRemove_003Eb__0)).ToList())
					{
						mgr.idRef.Remove(item.Key);
					}
				}
			}
		}

		public void Remove(long referenceID)
		{
			_003C_003Ec__DisplayClass31_0 _003C_003Ec__DisplayClass31_ = new _003C_003Ec__DisplayClass31_0();
			_003C_003Ec__DisplayClass31_.referenceID = referenceID;
			foreach (ES3ReferenceMgrBase mgr in mgrs)
			{
				if (mgr == null)
				{
					continue;
				}
				lock (mgr._lock)
				{
					mgr.idRef.Remove(_003C_003Ec__DisplayClass31_.referenceID);
					foreach (KeyValuePair<UnityEngine.Object, long> item in mgr.refId.Where(_003C_003Ec__DisplayClass31_._003C_003E9__0 ?? (_003C_003Ec__DisplayClass31_._003C_003E9__0 = _003C_003Ec__DisplayClass31_._003CRemove_003Eb__0)).ToList())
					{
						mgr.refId.Remove(item.Key);
					}
				}
			}
		}

		public void RemoveNullOrInvalidValues()
		{
			foreach (long item in idRef.Where(_003C_003Ec._003C_003E9__32_0 ?? (_003C_003Ec._003C_003E9__32_0 = _003C_003Ec._003C_003E9._003CRemoveNullOrInvalidValues_003Eb__32_0)).Select(_003C_003Ec._003C_003E9__32_1 ?? (_003C_003Ec._003C_003E9__32_1 = _003C_003Ec._003C_003E9._003CRemoveNullOrInvalidValues_003Eb__32_1)).ToList())
			{
				idRef.Remove(item);
			}
			if (GlobalReferences != null)
			{
				GlobalReferences.RemoveInvalidKeys();
			}
		}

		public void Clear()
		{
			lock (_lock)
			{
				refId.Clear();
				idRef.Clear();
			}
		}

		public bool Contains(UnityEngine.Object obj)
		{
			return refId.ContainsKey(obj);
		}

		public bool Contains(long referenceID)
		{
			return idRef.ContainsKey(referenceID);
		}

		public void ChangeId(long oldId, long newId)
		{
			foreach (ES3ReferenceMgrBase mgr in mgrs)
			{
				if (!(mgr == null))
				{
					mgr.idRef.ChangeKey(oldId, newId);
					mgr.refId = null;
				}
			}
		}

		internal static long GetNewRefID()
		{
			if (rng == null)
			{
				rng = new System.Random();
			}
			byte[] array = new byte[8];
			rng.NextBytes(array);
			return Math.Abs(BitConverter.ToInt64(array, 0) % long.MaxValue);
		}

		internal static bool CanBeSaved(UnityEngine.Object obj)
		{
			return true;
		}
	}
}
