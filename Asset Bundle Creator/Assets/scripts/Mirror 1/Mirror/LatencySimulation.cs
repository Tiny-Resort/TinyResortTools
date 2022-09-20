using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	[HelpURL("https://mirror-networking.gitbook.io/docs/transports/latency-simulaton-transport")]
	[DisallowMultipleComponent]
	public class LatencySimulation : Transport
	{
		public Transport wrap;

		[Header("Common")]
		[Tooltip("Spike latency via perlin(Time * speedMultiplier) * spikeMultiplier")]
		[Range(0f, 1f)]
		public float latencySpikeMultiplier;

		[Tooltip("Spike latency via perlin(Time * speedMultiplier) * spikeMultiplier")]
		public float latencySpikeSpeedMultiplier = 1f;

		[Header("Reliable Messages")]
		[Tooltip("Reliable latency in seconds")]
		public float reliableLatency;

		[Header("Unreliable Messages")]
		[Tooltip("Packet loss in %")]
		[Range(0f, 1f)]
		public float unreliableLoss;

		[Tooltip("Unreliable latency in seconds")]
		public float unreliableLatency;

		[Tooltip("Scramble % of unreliable messages, just like over the real network. Mirror unreliable is unordered.")]
		[Range(0f, 1f)]
		public float unreliableScramble;

		private List<QueuedMessage> reliableClientToServer = new List<QueuedMessage>();

		private List<QueuedMessage> reliableServerToClient = new List<QueuedMessage>();

		private List<QueuedMessage> unreliableClientToServer = new List<QueuedMessage>();

		private List<QueuedMessage> unreliableServerToClient = new List<QueuedMessage>();

		private System.Random random = new System.Random();

		public void Awake()
		{
			if (wrap == null)
			{
				throw new Exception("PressureDrop requires an underlying transport to wrap around.");
			}
		}

		private void OnEnable()
		{
			wrap.enabled = true;
		}

		private void OnDisable()
		{
			wrap.enabled = false;
		}

		protected virtual float Noise(float time)
		{
			return Mathf.PerlinNoise(time, time);
		}

		private float SimulateLatency(int channeldId)
		{
			float num = Noise(Time.time * latencySpikeSpeedMultiplier) * latencySpikeMultiplier;
			switch (channeldId)
			{
			case 0:
				return reliableLatency + num;
			case 1:
				return unreliableLatency + num;
			default:
				return 0f;
			}
		}

		private void SimulateSend(int connectionId, ArraySegment<byte> segment, int channelId, float latency, List<QueuedMessage> reliableQueue, List<QueuedMessage> unreliableQueue)
		{
			byte[] array = new byte[segment.Count];
			Buffer.BlockCopy(segment.Array, segment.Offset, array, 0, segment.Count);
			QueuedMessage queuedMessage = default(QueuedMessage);
			queuedMessage.connectionId = connectionId;
			queuedMessage.bytes = array;
			queuedMessage.time = Time.time + latency;
			QueuedMessage item = queuedMessage;
			switch (channelId)
			{
			case 0:
				reliableQueue.Add(item);
				break;
			case 1:
				if (!(random.NextDouble() < (double)unreliableLoss))
				{
					bool num = random.NextDouble() < (double)unreliableScramble;
					int count = unreliableQueue.Count;
					int index = (num ? random.Next(0, count + 1) : count);
					unreliableQueue.Insert(index, item);
				}
				break;
			default:
				Debug.LogError(string.Format("{0} unexpected channelId: {1}", "LatencySimulation", channelId));
				break;
			}
		}

		public override bool Available()
		{
			return wrap.Available();
		}

		public override void ClientConnect(string address)
		{
			wrap.OnClientConnected = OnClientConnected;
			wrap.OnClientDataReceived = OnClientDataReceived;
			wrap.OnClientError = OnClientError;
			wrap.OnClientDisconnected = OnClientDisconnected;
			wrap.ClientConnect(address);
		}

		public override void ClientConnect(Uri uri)
		{
			wrap.OnClientConnected = OnClientConnected;
			wrap.OnClientDataReceived = OnClientDataReceived;
			wrap.OnClientError = OnClientError;
			wrap.OnClientDisconnected = OnClientDisconnected;
			wrap.ClientConnect(uri);
		}

		public override bool ClientConnected()
		{
			return wrap.ClientConnected();
		}

		public override void ClientDisconnect()
		{
			wrap.ClientDisconnect();
			reliableClientToServer.Clear();
			unreliableClientToServer.Clear();
		}

		public override void ClientSend(ArraySegment<byte> segment, int channelId)
		{
			float latency = SimulateLatency(channelId);
			SimulateSend(0, segment, channelId, latency, reliableClientToServer, unreliableClientToServer);
		}

		public override Uri ServerUri()
		{
			return wrap.ServerUri();
		}

		public override bool ServerActive()
		{
			return wrap.ServerActive();
		}

		public override string ServerGetClientAddress(int connectionId)
		{
			return wrap.ServerGetClientAddress(connectionId);
		}

		public override void ServerDisconnect(int connectionId)
		{
			wrap.ServerDisconnect(connectionId);
		}

		public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
		{
			float latency = SimulateLatency(channelId);
			SimulateSend(connectionId, segment, channelId, latency, reliableServerToClient, unreliableServerToClient);
		}

		public override void ServerStart()
		{
			wrap.OnServerConnected = OnServerConnected;
			wrap.OnServerDataReceived = OnServerDataReceived;
			wrap.OnServerError = OnServerError;
			wrap.OnServerDisconnected = OnServerDisconnected;
			wrap.ServerStart();
		}

		public override void ServerStop()
		{
			wrap.ServerStop();
			reliableServerToClient.Clear();
			unreliableServerToClient.Clear();
		}

		public override void ClientEarlyUpdate()
		{
			wrap.ClientEarlyUpdate();
		}

		public override void ServerEarlyUpdate()
		{
			wrap.ServerEarlyUpdate();
		}

		public override void ClientLateUpdate()
		{
			if (reliableClientToServer.Count > 0)
			{
				QueuedMessage queuedMessage = reliableClientToServer[0];
				if (queuedMessage.time <= Time.time)
				{
					wrap.ClientSend(new ArraySegment<byte>(queuedMessage.bytes), 0);
					reliableClientToServer.RemoveAt(0);
				}
			}
			if (unreliableClientToServer.Count > 0)
			{
				QueuedMessage queuedMessage2 = unreliableClientToServer[0];
				if (queuedMessage2.time <= Time.time)
				{
					wrap.ClientSend(new ArraySegment<byte>(queuedMessage2.bytes), 1);
					unreliableClientToServer.RemoveAt(0);
				}
			}
			wrap.ClientLateUpdate();
		}

		public override void ServerLateUpdate()
		{
			if (reliableServerToClient.Count > 0)
			{
				QueuedMessage queuedMessage = reliableServerToClient[0];
				if (queuedMessage.time <= Time.time)
				{
					wrap.ServerSend(queuedMessage.connectionId, new ArraySegment<byte>(queuedMessage.bytes), 0);
					reliableServerToClient.RemoveAt(0);
				}
			}
			if (unreliableServerToClient.Count > 0)
			{
				QueuedMessage queuedMessage2 = unreliableServerToClient[0];
				if (queuedMessage2.time <= Time.time)
				{
					wrap.ServerSend(queuedMessage2.connectionId, new ArraySegment<byte>(queuedMessage2.bytes), 1);
					unreliableServerToClient.RemoveAt(0);
				}
			}
			wrap.ServerLateUpdate();
		}

		public override int GetBatchThreshold(int channelId)
		{
			return wrap.GetBatchThreshold(channelId);
		}

		public override int GetMaxPacketSize(int channelId = 0)
		{
			return wrap.GetMaxPacketSize(channelId);
		}

		public override void Shutdown()
		{
			wrap.Shutdown();
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", "LatencySimulation", wrap);
		}
	}
}
