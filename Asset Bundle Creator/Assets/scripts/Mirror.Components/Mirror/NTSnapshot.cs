using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirror
{
	public struct NTSnapshot : Snapshot
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public double remoteTimestamp
		{
			get;
			set; }

		public double localTimestamp
		{
			get;
			set; }

		public NTSnapshot(double remoteTimestamp, double localTimestamp, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			this.remoteTimestamp = remoteTimestamp;
			this.localTimestamp = localTimestamp;
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}

		public static NTSnapshot Interpolate(NTSnapshot from, NTSnapshot to, double t)
		{
			return new NTSnapshot(0.0, 0.0, Vector3.LerpUnclamped(from.position, to.position, (float)t), Quaternion.SlerpUnclamped(from.rotation, to.rotation, (float)t), Vector3.LerpUnclamped(from.scale, to.scale, (float)t));
		}
	}
}
