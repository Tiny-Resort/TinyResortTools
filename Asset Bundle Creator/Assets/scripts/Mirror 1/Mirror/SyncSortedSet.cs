using System.Collections.Generic;

namespace Mirror
{
	public class SyncSortedSet<T> : SyncSet<T>
	{
		public SyncSortedSet()
			: this((IComparer<T>)Comparer<T>.Default)
		{
		}

		public SyncSortedSet(IComparer<T> comparer)
			: base((ISet<T>)new SortedSet<T>(comparer ?? Comparer<T>.Default))
		{
		}

		public new SortedSet<T>.Enumerator GetEnumerator()
		{
			return ((SortedSet<T>)objects).GetEnumerator();
		}
	}
}
