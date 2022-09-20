using System.Collections.Generic;
using UnityEngine;

namespace ES3Internal
{
	public class ES3Prefab : MonoBehaviour
	{
		public long prefabId = GetNewRefID();

		public ES3RefIdDictionary localRefs = new ES3RefIdDictionary();

		public void Awake()
		{
			ES3ReferenceMgrBase current = ES3ReferenceMgrBase.Current;
			if (current == null)
			{
				return;
			}
			foreach (KeyValuePair<Object, long> localRef in localRefs)
			{
				if (localRef.Key != null)
				{
					current.Add(localRef.Key);
				}
			}
		}

		public long Get(Object obj)
		{
			long value;
			if (localRefs.TryGetValue(obj, out value))
			{
				return value;
			}
			return -1L;
		}

		public long Add(Object obj)
		{
			long value;
			if (localRefs.TryGetValue(obj, out value))
			{
				return value;
			}
			if (!ES3ReferenceMgrBase.CanBeSaved(obj))
			{
				return -1L;
			}
			value = GetNewRefID();
			localRefs.Add(obj, value);
			return value;
		}

		public Dictionary<string, string> GetReferences()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			ES3ReferenceMgrBase current = ES3ReferenceMgrBase.Current;
			if (current == null)
			{
				return dictionary;
			}
			foreach (KeyValuePair<Object, long> localRef in localRefs)
			{
				dictionary.Add(value: current.Add(localRef.Key).ToString(), key: localRef.Value.ToString());
			}
			return dictionary;
		}

		public void ApplyReferences(Dictionary<long, long> localToGlobal)
		{
			if (ES3ReferenceMgrBase.Current == null)
			{
				return;
			}
			foreach (KeyValuePair<Object, long> localRef in localRefs)
			{
				long value;
				if (localToGlobal.TryGetValue(localRef.Value, out value))
				{
					ES3ReferenceMgrBase.Current.Add(localRef.Key, value);
				}
			}
		}

		public static long GetNewRefID()
		{
			return ES3ReferenceMgrBase.GetNewRefID();
		}
	}
}
