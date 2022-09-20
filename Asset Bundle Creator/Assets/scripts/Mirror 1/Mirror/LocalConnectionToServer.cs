using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	public class LocalConnectionToServer : NetworkConnectionToServer
	{
		internal LocalConnectionToClient connectionToClient;

		internal readonly Queue<PooledNetworkWriter> queue = new Queue<PooledNetworkWriter>();

		private bool connectedEventPending;

		private bool disconnectedEventPending;

		public override string address
		{
			get
			{
				return "localhost";
			}
		}

		internal void QueueConnectedEvent()
		{
			connectedEventPending = true;
		}

		internal void QueueDisconnectedEvent()
		{
			disconnectedEventPending = true;
		}

		internal override void Send(ArraySegment<byte> segment, int channelId = 0)
		{
			if (segment.Count == 0)
			{
				Debug.LogError("LocalConnection.SendBytes cannot send zero bytes");
				return;
			}
			Batcher batchForChannelId = GetBatchForChannelId(channelId);
			batchForChannelId.AddMessage(segment);
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				if (batchForChannelId.MakeNextBatch(pooledNetworkWriter, NetworkTime.localTime))
				{
					NetworkServer.OnTransportData(connectionId, pooledNetworkWriter.ToArraySegment(), channelId);
				}
				else
				{
					Debug.LogError("Local connection failed to make batch. This should never happen.");
				}
			}
		}

		internal override void Update()
		{
			base.Update();
			if (connectedEventPending)
			{
				connectedEventPending = false;
				Action onConnectedEvent = NetworkClient.OnConnectedEvent;
				if (onConnectedEvent != null)
				{
					onConnectedEvent();
				}
			}
			while (queue.Count > 0)
			{
				PooledNetworkWriter pooledNetworkWriter = queue.Dequeue();
				ArraySegment<byte> message = pooledNetworkWriter.ToArraySegment();
				Batcher batchForChannelId = GetBatchForChannelId(0);
				batchForChannelId.AddMessage(message);
				using (PooledNetworkWriter pooledNetworkWriter2 = NetworkWriterPool.GetWriter())
				{
					if (batchForChannelId.MakeNextBatch(pooledNetworkWriter2, NetworkTime.localTime))
					{
						NetworkClient.OnTransportData(pooledNetworkWriter2.ToArraySegment(), 0);
					}
				}
				NetworkWriterPool.Recycle(pooledNetworkWriter);
			}
			if (disconnectedEventPending)
			{
				disconnectedEventPending = false;
				Action onDisconnectedEvent = NetworkClient.OnDisconnectedEvent;
				if (onDisconnectedEvent != null)
				{
					onDisconnectedEvent();
				}
			}
		}

		internal void DisconnectInternal()
		{
			isReady = false;
			NetworkClient.ready = false;
		}

		public override void Disconnect()
		{
			connectionToClient.DisconnectInternal();
			DisconnectInternal();
			NetworkServer.RemoveLocalConnection();
			NetworkClient.OnTransportDisconnected();
		}

		internal override bool IsAlive(float timeout)
		{
			return true;
		}
	}
}
