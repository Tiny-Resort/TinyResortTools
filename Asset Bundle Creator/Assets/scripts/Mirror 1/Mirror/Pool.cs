using System;
using System.Collections.Generic;

namespace Mirror
{
	public class Pool<T>
	{
		private readonly Stack<T> objects = new Stack<T>();

		private readonly Func<T> objectGenerator;

		public int Count
		{
			get
			{
				return objects.Count;
			}
		}

		public Pool(Func<T> objectGenerator, int initialCapacity)
		{
			this.objectGenerator = objectGenerator;
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
			objects.Push(item);
		}
	}
}
