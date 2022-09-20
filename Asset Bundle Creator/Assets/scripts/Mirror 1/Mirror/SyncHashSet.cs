using System.Collections.Generic;

namespace Mirror
{
	public class SyncHashSet<T> : SyncSet<T>
	{
		public SyncHashSet()
			: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
		{
		}

		public SyncHashSet(IEqualityComparer<T> comparer)
			: base((ISet<T>)new HashSet<T>(comparer ?? EqualityComparer<T>.Default))
		{
		}

		public new HashSet<T>.Enumerator GetEnumerator()
		{
			return ((HashSet<T>)objects).GetEnumerator();
		}
	}
}
