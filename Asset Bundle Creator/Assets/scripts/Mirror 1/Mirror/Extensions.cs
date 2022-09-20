using System;
using System.Collections.Generic;

namespace Mirror
{
	public static class Extensions
	{
		public static int GetStableHashCode(this string text)
		{
			int num = 23;
			foreach (char c in text)
			{
				num = num * 31 + c;
			}
			return num;
		}

		internal static string GetMethodName(this Delegate func)
		{
			return func.Method.Name;
		}

		public static void CopyTo<T>(this IEnumerable<T> source, List<T> destination)
		{
			destination.AddRange(source);
		}
	}
}
