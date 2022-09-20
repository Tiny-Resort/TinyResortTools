using System;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

public abstract class NextCommon
{
	protected const int MAX_MESSAGES = 256;

	protected EResult SendSocket(HSteamNetConnection conn, byte[] data, int channelId)
	{
		Array.Resize(ref data, data.Length + 1);
		data[data.Length - 1] = (byte)channelId;
		GCHandle gCHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
		IntPtr pData = gCHandle.AddrOfPinnedObject();
		int nSendFlags = ((channelId != 1) ? 8 : 0);
		long pOutMessageNumber;
		EResult eResult = SteamNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, nSendFlags, out pOutMessageNumber);
		if (eResult != EResult.k_EResultOK)
		{
			Debug.LogWarning(string.Format("Send issue: {0}", eResult));
		}
		gCHandle.Free();
		return eResult;
	}

	protected ValueTuple<byte[], int> ProcessMessage(IntPtr ptrs)
	{
		SteamNetworkingMessage_t steamNetworkingMessage_t = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptrs);
		byte[] array = new byte[steamNetworkingMessage_t.m_cbSize];
		Marshal.Copy(steamNetworkingMessage_t.m_pData, array, 0, steamNetworkingMessage_t.m_cbSize);
		NativeMethods.SteamAPI_SteamNetworkingMessage_t_Release(ptrs);
		int item = array[array.Length - 1];
		Array.Resize(ref array, array.Length - 1);
		return new ValueTuple<byte[], int>(array, item);
	}
}
