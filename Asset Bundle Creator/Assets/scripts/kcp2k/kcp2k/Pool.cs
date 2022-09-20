using System;
using System.Collections.Generic;

namespace kcp2k
{
	public class Pool<T>
	{
		private readonly Stack<T> objects = new Stack<T>();

		private readonly Func<T> objectGenerator;

		private readonly Action<T> objectResetter;

		public int Count
		{
			get
			{
				return objects.Count;
			}
		}

		public Pool(Func<T> objectGenerator, Action<T> objectResetter, int initialCapacity)
		{
			this.objectGenerator = objectGenerator;
			this.objectResetter = objectResetter;
			for (int i = 0; i < initialCapacity; i++)
			{
				objects.Push(objectGenerator());
			}
		}

		public T Take()
		{
			if (objects.Count <= 0)
			{
				return objectGenerator();
			}
			return objects.Pop();
		}

		public void Return(T item)
		{
			objectResetter(item);
			objects.Push(item);
		}

		public void Clear()
		{
			objects.Clear();
		}
	}
}
