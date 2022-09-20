using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	public abstract class NetworkConnection
	{
		public const int LocalConnectionId = 0;

		internal readonly HashSet<NetworkIdentity> observing = new HashSet<NetworkIdentity>();

		public readonly int connectionId;

		public bool isAuthenticated;

		public object authenticationData;

		public bool isReady;

		public float lastMessageTime;

		public readonly HashSet<NetworkIdentity> clientOwnedObjects = new HashSet<NetworkIdentity>();

		protected Dictionary<int, Batcher> batches = new Dictionary<int, Batcher>();

		public abstract string address { get; }

		public NetworkIdentity identity { get; internal set; }

		public double remoteTimeStamp { get; internal set; }

		internal NetworkConnection()
		{
			lastMessageTime = Time.time;
		}

		internal NetworkConnection(int networkConnectionId)
			: this()
		{
			connectionId = networkConnectionId;
		}

		protected Batcher GetBatchForChannelId(int channelId)
		{
			Batcher value;
			if (!batches.TryGetValue(channelId, out value))
			{
				value = new Batcher(Transport.activeTransport.GetBatchThreshold(channelId));
				batches[channelId] = value;
			}
			return value;
		}

		protected static bool ValidatePacketSize(ArraySegment<byte> segment, int channelId)
		{
			int maxPacketSize = Transport.activeTransport.GetMaxPacketSize(channelId);
			if (segment.Count > maxPacketSize)
			{
				Debug.LogError(string.Format("NetworkConnection.ValidatePacketSize: cannot send packet larger than {0} bytes, was {1} bytes", maxPacketSize, segment.Count));
				return false;
			}
			if (segment.Count == 0)
			{
				Debug.LogError("NetworkConnection.ValidatePacketSize: cannot send zero bytes");
				return false;
			}
			return true;
		}

		public void Send<T>(T message, int channelId = 0) where T : struct, NetworkMessage
		{
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				MessagePacking.Pack(message, pooledNetworkWriter);
				NetworkDiagnostics.OnSend(message, channelId, pooledNetworkWriter.Position, 1);
				Send(pooledNetworkWriter.ToArraySegment(), channelId);
			}
		}

		internal virtual void Send(ArraySegment<byte> segment, int channelId = 0)
		{
			GetBatchForChannelId(channelId).AddMessage(segment);
		}

		protected abstract void SendToTransport(ArraySegment<byte> segment, int channelId = 0);

		internal virtual void Update()
		{
			foreach (KeyValuePair<int, Batcher> batch in batches)
			{
				Batcher value = batch.Value;
				using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
				{
					while (value.MakeNextBatch(pooledNetworkWriter, NetworkTime.localTime))
					{
						ArraySegment<byte> segment = pooledNetworkWriter.ToArraySegment();
						if (ValidatePacketSize(segment, batch.Key))
						{
							SendToTransport(segment, batch.Key);
							pooledNetworkWriter.Position = 0;
						}
					}
				}
			}
		}

		public abstract void Disconnect();

		public override string ToString()
		{
			return string.Format("connection({0})", connectionId);
		}

		internal void AddToObserving(NetworkIdentity netIdentity)
		{
			observing.Add(netIdentity);
			NetworkServer.ShowForConnection(netIdentity, this);
		}

		internal void RemoveFromObserving(NetworkIdentity netIdentity, bool isDestroyed)
		{
			observing.Remove(netIdentity);
			if (!isDestroyed)
			{
				NetworkServer.HideForConnection(netIdentity, this);
			}
		}

		internal void RemoveFromObservingsObservers()
		{
			foreach (NetworkIdentity item in observing)
			{
				item.RemoveObserverInternal(this);
			}
			observing.Clear();
		}

		internal virtual bool IsAlive(float timeout)
		{
			return Time.time - lastMessageTime < timeout;
		}

		internal void AddOwnedObject(NetworkIdentity obj)
		{
			clientOwnedObjects.Add(obj);
		}

		internal void RemoveOwnedObject(NetworkIdentity obj)
		{
			clientOwnedObjects.Remove(obj);
		}

		internal void DestroyOwnedObjects()
		{
			foreach (NetworkIdentity item in new HashSet<NetworkIdentity>(clientOwnedObjects))
			{
				if (item != null)
				{
					NetworkServer.Destroy(item.gameObject);
				}
			}
			clientOwnedObjects.Clear();
		}
	}
}
