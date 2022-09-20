using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Mirror
{
	public class SyncIDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, SyncObject, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		public delegate void SyncDictionaryChanged(Operation op, TKey key, TValue item);

		public enum Operation : byte
		{
			OP_ADD = 0,
			OP_CLEAR = 1,
			OP_REMOVE = 2,
			OP_SET = 3
		}

		private struct Change
		{
			internal Operation operation;

			internal TKey key;

			internal TValue item;
		}

		protected readonly IDictionary<TKey, TValue> objects;

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

		public ICollection<TKey> Keys
		{
			get
			{
				return objects.Keys;
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				return objects.Values;
			}
		}

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get
			{
				return objects.Keys;
			}
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get
			{
				return objects.Values;
			}
		}

		public TValue this[TKey i]
		{
			get
			{
				return objects[i];
			}
			set
			{
				if (ContainsKey(i))
				{
					objects[i] = value;
					AddOperation(Operation.OP_SET, i, value);
				}
				else
				{
					objects[i] = value;
					AddOperation(Operation.OP_ADD, i, value);
				}
			}
		}

		public event SyncDictionaryChanged Callback;

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

		public SyncIDictionary(IDictionary<TKey, TValue> objects)
		{
			this.objects = objects;
		}

		private void AddOperation(Operation op, TKey key, TValue item)
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException("SyncDictionaries can only be modified by the server");
			}
			Change change = default(Change);
			change.operation = op;
			change.key = key;
			change.item = item;
			Change item2 = change;
			changes.Add(item2);
			SyncDictionaryChanged callback = this.Callback;
			if (callback != null)
			{
				callback(op, key, item);
			}
		}

		public void OnSerializeAll(NetworkWriter writer)
		{
			writer.WriteUInt((uint)objects.Count);
			foreach (KeyValuePair<TKey, TValue> @object in objects)
			{
				writer.Write(@object.Key);
				writer.Write(@object.Value);
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
				case Operation.OP_REMOVE:
				case Operation.OP_SET:
					writer.Write(change.key);
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
				TKey key = reader.Read<TKey>();
				TValue value = reader.Read<TValue>();
				objects.Add(key, value);
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
				TKey key = default(TKey);
				TValue val = default(TValue);
				switch (operation)
				{
				case Operation.OP_ADD:
				case Operation.OP_SET:
					key = reader.Read<TKey>();
					val = reader.Read<TValue>();
					if (flag)
					{
						objects[key] = val;
					}
					break;
				case Operation.OP_CLEAR:
					if (flag)
					{
						objects.Clear();
					}
					break;
				case Operation.OP_REMOVE:
					key = reader.Read<TKey>();
					val = reader.Read<TValue>();
					if (flag)
					{
						objects.Remove(key);
					}
					break;
				}
				if (flag)
				{
					SyncDictionaryChanged callback = this.Callback;
					if (callback != null)
					{
						callback(operation, key, val);
					}
				}
				else
				{
					changesAhead--;
				}
			}
		}

		public void Clear()
		{
			objects.Clear();
			AddOperation(Operation.OP_CLEAR, default(TKey), default(TValue));
		}

		public bool ContainsKey(TKey key)
		{
			return objects.ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			TValue value;
			if (objects.TryGetValue(key, out value) && objects.Remove(key))
			{
				AddOperation(Operation.OP_REMOVE, key, value);
				return true;
			}
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return objects.TryGetValue(key, out value);
		}

		public void Add(TKey key, TValue value)
		{
			objects.Add(key, value);
			AddOperation(Operation.OP_ADD, key, value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			TValue value;
			if (TryGetValue(item.Key, out value))
			{
				return EqualityComparer<TValue>.Default.Equals(value, item.Value);
			}
			return false;
		}

		public void CopyTo([NotNull] KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "Array Index Out of Range");
			}
			if (array.Length - arrayIndex < Count)
			{
				throw new ArgumentException("The number of items in the SyncDictionary is greater than the available space from arrayIndex to the end of the destination array");
			}
			int num = arrayIndex;
			foreach (KeyValuePair<TKey, TValue> @object in objects)
			{
				array[num] = @object;
				num++;
			}
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			bool num = objects.Remove(item.Key);
			if (num)
			{
				AddOperation(Operation.OP_REMOVE, item.Key, item.Value);
			}
			return num;
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return objects.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return objects.GetEnumerator();
		}
	}
}
