using System;
using System.Collections;
using System.Collections.Generic;

namespace Mirror
{
	public class SyncSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, SyncObject
	{
		public delegate void SyncSetChanged(Operation op, T item);

		public enum Operation : byte
		{
			OP_ADD = 0,
			OP_CLEAR = 1,
			OP_REMOVE = 2
		}

		private struct Change
		{
			internal Operation operation;

			internal T item;
		}

		protected readonly ISet<T> objects;

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

		public event SyncSetChanged Callback;

		public SyncSet(ISet<T> objects)
		{
			this.objects = objects;
		}

		public void Reset()
		{
			IsReadOnly = false;
			changes.Clear();
			changesAhead = 0;
			objects.Clear();
		}

		public void Flush()
		{
			changes.Clear();
		}

		private void AddOperation(Operation op, T item)
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException("SyncSets can only be modified at the server");
			}
			Change change = default(Change);
			change.operation = op;
			change.item = item;
			Change item2 = change;
			changes.Add(item2);
			SyncSetChanged callback = this.Callback;
			if (callback != null)
			{
				callback(op, item);
			}
		}

		private void AddOperation(Operation op)
		{
			AddOperation(op, default(T));
		}

		public void OnSerializeAll(NetworkWriter writer)
		{
			writer.WriteUInt((uint)objects.Count);
			foreach (T @object in objects)
			{
				writer.Write(@object);
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
				case Operation.OP_REMOVE:
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
				T item = default(T);
				switch (operation)
				{
				case Operation.OP_ADD:
					item = reader.Read<T>();
					if (flag)
					{
						objects.Add(item);
					}
					break;
				case Operation.OP_CLEAR:
					if (flag)
					{
						objects.Clear();
					}
					break;
				case Operation.OP_REMOVE:
					item = reader.Read<T>();
					if (flag)
					{
						objects.Remove(item);
					}
					break;
				}
				if (flag)
				{
					SyncSetChanged callback = this.Callback;
					if (callback != null)
					{
						callback(operation, item);
					}
				}
				else
				{
					changesAhead--;
				}
			}
		}

		public bool Add(T item)
		{
			if (objects.Add(item))
			{
				AddOperation(Operation.OP_ADD, item);
				return true;
			}
			return false;
		}

		void ICollection<T>.Add(T item)
		{
			if (objects.Add(item))
			{
				AddOperation(Operation.OP_ADD, item);
			}
		}

		public void Clear()
		{
			objects.Clear();
			AddOperation(Operation.OP_CLEAR);
		}

		public bool Contains(T item)
		{
			return objects.Contains(item);
		}

		public void CopyTo(T[] array, int index)
		{
			objects.CopyTo(array, index);
		}

		public bool Remove(T item)
		{
			if (objects.Remove(item))
			{
				AddOperation(Operation.OP_REMOVE, item);
				return true;
			}
			return false;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return objects.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			if (other == this)
			{
				Clear();
				return;
			}
			foreach (T item in other)
			{
				Remove(item);
			}
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			ISet<T> set = other as ISet<T>;
			if (set != null)
			{
				IntersectWithSet(set);
				return;
			}
			HashSet<T> otherSet = new HashSet<T>(other);
			IntersectWithSet(otherSet);
		}

		private void IntersectWithSet(ISet<T> otherSet)
		{
			foreach (T item in new List<T>(objects))
			{
				if (!otherSet.Contains(item))
				{
					Remove(item);
				}
			}
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			return objects.IsProperSubsetOf(other);
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			return objects.IsProperSupersetOf(other);
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			return objects.IsSubsetOf(other);
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			return objects.IsSupersetOf(other);
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			return objects.Overlaps(other);
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			return objects.SetEquals(other);
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			if (other == this)
			{
				Clear();
				return;
			}
			foreach (T item in other)
			{
				if (!Remove(item))
				{
					Add(item);
				}
			}
		}

		public void UnionWith(IEnumerable<T> other)
		{
			if (other == this)
			{
				return;
			}
			foreach (T item in other)
			{
				Add(item);
			}
		}
	}
}
