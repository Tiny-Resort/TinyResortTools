using System;
using UnityEngine;

namespace Mirror
{
	public struct SpawnMessage : NetworkMessage
	{
		public uint netId;

		public bool isLocalPlayer;

		public bool isOwner;

		public ulong sceneId;

		public Guid assetId;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public ArraySegment<byte> payload;
	}
}
