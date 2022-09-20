using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirror
{
	public class SyncList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, SyncObject
	{
		public delegate void SyncListChanged(Operation op, int itemIndex, T oldItem, T newItem);

		public enum Operation : byte
		{
			OP_ADD = 0,
			OP_CLEAR = 1,
			OP_INSERT = 2,
			OP_REMOVEAT = 3,
			OP_SET = 4
		}

		private struct Change
		{
			internal Operation operation;

			internal int index;

			internal T item;
		}

		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly SyncList<T> list;

			private int index;

			public T Current
			{
				get;
				private set; }

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public Enumerator(SyncList<T> list)
			{
				this.list = list;
				index = -1;
				Current = default(T);
			}

			public bool MoveNext()
			{
				if (++index >= list.Count)
				{
					return false;
				}
				Current = list[index];
				return true;
			}

			public void Reset()
			{
				index = -1;
			}

			public void Dispose()
			{
			}
		}

		private readonly IList<T> objects;

		private readonly IEqualityComparer<T> comparer;

		private readonly List<Change> changes = new List<Change>();

		private int changesAhead;

		public int Count
		{
			get
			{
				return objects.Count;
			}
		}

		public bool IsReadOnly { get; private set; }

		public bool IsDirty
		{
			get
			{
				return changes.Count > 0;
			}
		}

		public T this[int i]
		{
			get
			{
				return objects[i];
			}
			set
			{
				if (!comparer.Equals(objects[i], value))
				{
					T oldItem = objects[i];
					objects[i] = value;
					AddOperation(Operation.OP_SET, i, oldItem, value);
				}
			}
		}

		public event SyncListChanged Callback;

		public SyncList()
			: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
		{
		}

		public SyncList(IEqualityComparer<T> comparer)
		{
			this.comparer = comparer ?? EqualityComparer<T>.Default;
			objects = new List<T>();
		}

		public SyncList(IList<T> objects, IEqualityComparer<T> comparer = null)
		{
			this.comparer = comparer ?? EqualityComparer<T>.Default;
			this.objects = objects;
		}

		public void Flush()
		{
			changes.Clear();
		}

		public void Reset()
		{
			IsReadOnly = false;
			changes.Clear();
			changesAhead = 0;
			objects.Clear();
		}

		private void AddOperation(Operation op, int itemIndex, T oldItem, T newItem)
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException("Synclists can only be modified at the server");
			}
			Change change = default(Change);
			change.operation = op;
			change.index = itemIndex;
			change.item = newItem;
			Change item = change;
			changes.Add(item);
			SyncListChanged callback = this.Callback;
			if (callback != null)
			{
				callback(op, itemIndex, oldItem, newItem);
			}
		}

		public void OnSerializeAll(NetworkWriter writer)
		{
			writer.WriteUInt((uint)objects.Count);
			for (int i = 0; i < objects.Count; i++)
			{
				T value = objects[i];
				writer.Write(value);
			}
			writer.WriteUInt((uint)changes.Count);
		}

		public void OnSerializeDelta(NetworkWriter writer)
		{
			writer.WriteUInt((uint)changes.Count);
			for (int i = 0; i < changes.Count; i++)
			{
				Change change = changes[i];
				writer.WriteByte((byte)change.operation);
				switch (change.operation)
				{
				case Operation.OP_ADD:
					writer.Write(change.item);
					break;
				case Operation.OP_REMOVEAT:
					writer.WriteUInt((uint)change.index);
					break;
				case Operation.OP_INSERT:
				case Operation.OP_SET:
					writer.WriteUInt((uint)change.index);
					writer.Write(change.item);
					break;
				}
			}
		}

		public void OnDeserializeAll(NetworkReader reader)
		{
			IsReadOnly = true;
			int num = (int)reader.ReadUInt();
			objects.Clear();
			changes.Clear();
			for (int i = 0; i < num; i++)
			{
				T item = reader.Read<T>();
				objects.Add(item);
			}
			changesAhead = (int)reader.ReadUInt();
		}

		public void OnDeserializeDelta(NetworkReader reader)
		{
			IsReadOnly = true;
			int num = (int)reader.ReadUInt();
			for (int i = 0; i < num; i++)
			{
				Operation operation = (Operation)reader.ReadByte();
				bool flag = changesAhead == 0;
				int num2 = 0;
				T oldItem = default(T);
				T val = default(T);
				switch (operation)
				{
				case Operation.OP_ADD:
					val = reader.Read<T>();
					if (flag)
					{
						num2 = objects.Count;
						objects.Add(val);
					}
					break;
				case Operation.OP_CLEAR:
					if (flag)
					{
						objects.Clear();
					}
					break;
				case Operation.OP_INSERT:
					num2 = (int)reader.ReadUInt();
					val = reader.Read<T>();
					if (flag)
					{
						objects.Insert(num2, val);
					}
					break;
				case Operation.OP_REMOVEAT:
					num2 = (int)reader.ReadUInt();
					if (flag)
					{
						oldItem = objects[num2];
						objects.RemoveAt(num2);
					}
					break;
				case Operation.OP_SET:
					num2 = (int)reader.ReadUInt();
					val = reader.Read<T>();
					if (flag)
					{
						oldItem = objects[num2];
						objects[num2] = val;
					}
					break;
				}
				if (flag)
				{
					SyncListChanged callback = this.Callback;
					if (callback != null)
					{
						callback(operation, num2, oldItem, val);
					}
				}
				else
				{
					changesAhead--;
				}
			}
		}

		public void Add(T item)
		{
			objects.Add(item);
			AddOperation(Operation.OP_ADD, objects.Count - 1, default(T), item);
		}

		public void AddRange(IEnumerable<T> range)
		{
			foreach (T item in range)
			{
				Add(item);
			}
		}

		public void Clear()
		{
			objects.Clear();
			AddOperation(Operation.OP_CLEAR, 0, default(T), default(T));
		}

		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		public void CopyTo(T[] array, int index)
		{
			objects.CopyTo(array, index);
		}

		public int IndexOf(T item)
		{
			for (int i = 0; i < objects.Count; i++)
			{
				if (comparer.Equals(item, objects[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public int FindIndex(Predicate<T> match)
		{
			for (int i = 0; i < objects.Count; i++)
			{
				if (match(objects[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public T Find(Predicate<T> match)
		{
			int num = FindIndex(match);
			if (num == -1)
			{
				return default(T);
			}
			return objects[num];
		}

		public List<T> FindAll(Predicate<T> match)
		{
			List<T> list = new List<T>();
			for (int i = 0; i < objects.Count; i++)
			{
				if (match(objects[i]))
				{
					list.Add(objects[i]);
				}
			}
			return list;
		}

		public void Insert(int index, T item)
		{
			objects.Insert(index, item);
			AddOperation(Operation.OP_INSERT, index, default(T), item);
		}

		public void InsertRange(int index, IEnumerable<T> range)
		{
			foreach (T item in range)
			{
				Insert(index, item);
				index++;
			}
		}

		public bool Remove(T item)
		{
			int num = IndexOf(item);
			bool num2 = num >= 0;
			if (num2)
			{
				RemoveAt(num);
			}
			return num2;
		}

		public void RemoveAt(int index)
		{
			T oldItem = objects[index];
			objects.RemoveAt(index);
			AddOperation(Operation.OP_REMOVEAT, index, oldItem, default(T));
		}

		public int RemoveAll(Predicate<T> match)
		{
			List<T> list = new List<T>();
			for (int i = 0; i < objects.Count; i++)
			{
				if (match(objects[i]))
				{
					list.Add(objects[i]);
				}
			}
			foreach (T item in list)
			{
				Remove(item);
			}
			return list.Count;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
}
