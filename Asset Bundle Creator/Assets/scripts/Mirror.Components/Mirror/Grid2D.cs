using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	public class Grid2D<T>
	{
		private Dictionary<Vector2Int, HashSet<T>> grid = new Dictionary<Vector2Int, HashSet<T>>();

		private Vector2Int[] neighbourOffsets = new Vector2Int[9]
		{
			Vector2Int.up,
			Vector2Int.up + Vector2Int.left,
			Vector2Int.up + Vector2Int.right,
			Vector2Int.left,
			Vector2Int.zero,
			Vector2Int.right,
			Vector2Int.down,
			Vector2Int.down + Vector2Int.left,
			Vector2Int.down + Vector2Int.right
		};

		public void Add(Vector2Int position, T value)
		{
			HashSet<T> value2;
			if (!grid.TryGetValue(position, out value2))
			{
				value2 = new HashSet<T>();
				grid[position] = value2;
			}
			value2.Add(value);
		}

		private void GetAt(Vector2Int position, HashSet<T> result)
		{
			HashSet<T> value;
			if (!grid.TryGetValue(position, out value))
			{
				return;
			}
			foreach (T item in value)
			{
				result.Add(item);
			}
		}

		public void GetWithNeighbours(Vector2Int position, HashSet<T> result)
		{
			result.Clear();
			Vector2Int[] array = neighbourOffsets;
			foreach (Vector2Int vector2Int in array)
			{
				GetAt(position + vector2Int, result);
			}
		}

		public void ClearNonAlloc()
		{
			foreach (HashSet<T> value in grid.Values)
			{
				value.Clear();
			}
		}
	}
}
