using System;
using System.Collections.Generic;

namespace Mirror
{
	public static class SnapshotInterpolation
	{
		public static void InsertIfNewEnough<T>(T snapshot, SortedList<double, T> buffer) where T : Snapshot
		{
			double remoteTimestamp = snapshot.remoteTimestamp;
			if ((buffer.Count != 1 || !(remoteTimestamp <= buffer.Values[0].remoteTimestamp)) && (buffer.Count < 2 || !(remoteTimestamp <= buffer.Values[1].remoteTimestamp)) && !buffer.ContainsKey(remoteTimestamp))
			{
				buffer.Add(remoteTimestamp, snapshot);
			}
		}

		public static bool HasAmountOlderThan<T>(SortedList<double, T> buffer, double threshold, int amount) where T : Snapshot
		{
			if (buffer.Count >= amount)
			{
				return buffer.Values[amount - 1].localTimestamp <= threshold;
			}
			return false;
		}

		public static double CalculateCatchup<T>(SortedList<double, T> buffer, int catchupThreshold, double catchupMultiplier) where T : Snapshot
		{
			int num = buffer.Count - catchupThreshold;
			if (num <= 0)
			{
				return 0.0;
			}
			return (double)num * catchupMultiplier;
		}

		public static void GetFirstSecondAndDelta<T>(SortedList<double, T> buffer, out T first, out T second, out double delta) where T : Snapshot
		{
			first = buffer.Values[0];
			second = buffer.Values[1];
			delta = second.remoteTimestamp - first.remoteTimestamp;
		}

		public static bool Compute<T>(double time, double deltaTime, ref double interpolationTime, double bufferTime, SortedList<double, T> buffer, int catchupThreshold, float catchupMultiplier, Func<T, T, double, T> Interpolate, out T computed) where T : Snapshot
		{
			computed = default(T);
			double threshold = time - bufferTime;
			if (!HasAmountOlderThan(buffer, threshold, 2))
			{
				return false;
			}
			double num = CalculateCatchup(buffer, catchupThreshold, catchupMultiplier);
			deltaTime *= 1.0 + num;
			interpolationTime += deltaTime;
			T first;
			T second;
			double delta;
			GetFirstSecondAndDelta(buffer, out first, out second, out delta);
			while (interpolationTime >= delta && HasAmountOlderThan(buffer, threshold, 3))
			{
				interpolationTime -= delta;
				buffer.RemoveAt(0);
				GetFirstSecondAndDelta(buffer, out first, out second, out delta);
			}
			double arg = Mathd.InverseLerp(first.remoteTimestamp, second.remoteTimestamp, first.remoteTimestamp + interpolationTime);
			computed = Interpolate(first, second, arg);
			if (!HasAmountOlderThan(buffer, threshold, 3))
			{
				interpolationTime = Math.Min(interpolationTime, delta);
			}
			return true;
		}
	}
}
