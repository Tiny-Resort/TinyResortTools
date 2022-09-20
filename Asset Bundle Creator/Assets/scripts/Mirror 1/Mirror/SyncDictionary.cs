using System.Collections.Generic;

namespace Mirror
{
	public class SyncDictionary<TKey, TValue> : SyncIDictionary<TKey, TValue>
	{
		public new Dictionary<TKey, TValue>.ValueCollection Values
		{
			get
			{
				return ((Dictionary<TKey, TValue>)objects).Values;
			}
		}

		public new Dictionary<TKey, TValue>.KeyCollection Keys
		{
			get
			{
				return ((Dictionary<TKey, TValue>)objects).Keys;
			}
		}

		public SyncDictionary()
			: base((IDictionary<TKey, TValue>)new Dictionary<TKey, TValue>())
		{
		}

		public SyncDictionary(IEqualityComparer<TKey> eq)
			: base((IDictionary<TKey, TValue>)new Dictionary<TKey, TValue>(eq))
		{
		}

		public new Dictionary<TKey, TValue>.Enumerator GetEnumerator()
		{
			return ((Dictionary<TKey, TValue>)objects).GetEnumerator();
		}
	}
}
