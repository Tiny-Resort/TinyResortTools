using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using Mirror;
using Mirror.RemoteCalls;
using HarmonyLib;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

// TODO: It is sending Chest data now, but can't write/read it due to it being a unique type.
// see: https://mirror-networking.gitbook.io/docs/manual/guides/serialization

namespace TinyResort;

public class TRNetwork : NetworkBehaviour {

    public static TRNetwork share;

    [SyncVar]
    public List<Chest> allChests = new();

    internal static Guid assetId = Guid.Parse("5452546f-6f6c-7343-7573-746f6d525043");

    internal static GameObject SpawnTRNetworkManager(SpawnMessage msg) {
        var TRNetworkManager = new GameObject("TRNetwork");
        DontDestroyOnLoad(TRNetworkManager);
        TRNetworkManager.SetActive(false);
        TRNetworkManager.AddComponent<TRNetwork>();
        TRNetworkManager.SetActive(true);
        return TRNetworkManager;
    }

    internal static void UnSpawnTRNetworkManager(GameObject spawned) => Destroy(spawned);

    internal static void Initialize() =>
        NetworkClient.RegisterSpawnHandler(assetId, SpawnTRNetworkManager, UnSpawnTRNetworkManager);

    internal static void Update() {
        if (share == null)
            if (NetworkServer.active)
                NetworkServer.Spawn(SpawnTRNetworkManager(new SpawnMessage()), assetId);
    }

    private void Awake() => share = this;

    //public override void OnStartServer() => allChests.AddRange(ContainerManager.manage.activeChests);

    #region Send To Host

    /*
     * Sends a message from the Host -> Host and from Client -> Host
     */
    [TargetRpc]
    public void CmdRequestActiveChests() {
        var writer = NetworkWriterPool.GetWriter();
        writer.WriteChestList(ContainerManager.manage.activeChests);

        //writer.WriteUInt(NetworkMapSharer.Instance.localChar.netId);
        SendCommandInternal(typeof(TRNetwork), "CmdRequestActiveChests", writer, 0, false);
        NetworkWriterPool.Recycle(writer);
    }

    protected static void InvokeUserCode_CmdRequestActiveChests(
        NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection
    ) =>
        ((TRNetwork)obj).UserCode_CmdRequestActiveChests(reader.ReadChestList());

    protected void UserCode_CmdRequestActiveChests(List<Chest> message) {
        share.RpcSendActiveChests(message);
        TRTools.LogError("CmdSendMessageToHost");
    }

    #endregion

    #region Send To All

    /*
         * Sends a message from the to both Host and Client, but only from Host.
         */
    [ClientRpc]
    public void RpcSendActiveChests(List<Chest> chests) {
        var writer = NetworkWriterPool.GetWriter();
        writer.WriteChestList(chests);
        SendRPCInternal(typeof(TRNetwork), "RpcSendActiveChests", writer, 0, false);
        NetworkWriterPool.Recycle(writer);
    }

    protected static void InvokeUserCode_RpcSendActiveChests(
        NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection
    ) =>
        ((TRNetwork)obj).UserCode_RpcSendActiveChests(reader.ReadChestList());

    protected void UserCode_RpcSendActiveChests(List<Chest> type) {
        if (!NetworkMapSharer.Instance.localChar) TRTools.LogError($"No Local Char");
        TRTools.LogError($"{type.Count}");
        if (NetworkMapSharer.Instance.localChar.isServer) TRTools.LogError($"Attempting to Send to Clients.");
        if (NetworkMapSharer.Instance.localChar.isClient && !NetworkMapSharer.Instance.localChar.isServer) {
            TRTools.LogError($"Attempting to Add....");
            share.allChests.Clear();
            share.allChests.AddRange(type);
        }
    }

    #endregion

    static TRNetwork() {
        RemoteCallHelper.RegisterRpcDelegate(
            typeof(TRNetwork), "RpcSendActiveChests", new CmdDelegate(InvokeUserCode_RpcSendActiveChests)
        );
        RemoteCallHelper.RegisterCommandDelegate(
            typeof(TRNetwork), "CmdRequestActiveChests", new CmdDelegate(InvokeUserCode_CmdRequestActiveChests), false
        );
        /*RemoteCallHelper.RegisterCommandDelegate(
            typeof(TRNetwork), "TargetSendMessageToClient", new CmdDelegate(InvokeUserCode_TargetSendMessageToClient),
            false
        );*/
    }

    /*#region Send to Single Client

    [TargetRpc]
    public void TargetSendMessageToClient(NetworkConnectionToClient con, string message) {
        var writer = NetworkWriterPool.GetWriter();
        writer.WriteString(message);
        SendCommandInternal(typeof(TRNetwork), "TargetSendMessageToClient", writer, 0, false);
        NetworkWriterPool.Recycle(writer);
    }

    internal void UserCode_TargetSendMessageToClient(string message) =>
        TRTools.LogError($"Sending message to Client: {message}");

    internal static void InvokeUserCode_TargetSendMessageToClient(
        NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection
    ) {
        if (!NetworkServer.active) {
            TRTools.LogError("Command CmdSendMessageToClient called on client.");
            return;
        }
        ((TRNetwork)obj).UserCode_TargetSendMessageToClient(reader.ReadString());
    }

    #endregion*/
}

public static class ChestReaderWriter {
    public static void WriteChestList(this NetworkWriter writer, List<Chest> dataList) {
        // Write the count of items in the list
        writer.WriteInt(dataList.Count);

        // Iterate through the list and write each custom data
        foreach (var chest in dataList) {
            writer.WriteBool(chest.inside);
            writer.WriteInt(chest.insideX);
            writer.WriteInt(chest.insideY);
            writer.WriteInt(chest.xPos);
            writer.WriteInt(chest.yPos);
            writer.WriteArray(chest.itemIds);
            writer.WriteArray(chest.itemStacks);
            writer.WriteInt(chest.playingLookingInside);
            writer.WriteInt(chest.placedInWorldLevel);
        }
    }

    public static List<Chest> ReadChestList(this NetworkReader reader) {
        // Read the count of items in the list
        var count = reader.ReadInt();

        // Create a new list to store CustomData objects
        var dataList = new List<Chest>();

        // Iterate through the count and read each CustomData object
        for (var i = 0; i < count; i++) {
            var data = new Chest(0, 0);
            data.inside = reader.ReadBool();
            data.insideX = reader.ReadInt();
            data.insideY = reader.ReadInt();
            data.xPos = reader.ReadInt();
            data.yPos = reader.ReadInt();
            data.itemIds = reader.ReadArray<int>();
            data.itemStacks = reader.ReadArray<int>();
            data.playingLookingInside = reader.ReadInt();
            data.placedInWorldLevel = reader.ReadInt();

            // Read other properties as needed
            dataList.Add(data);
        }

        return dataList;
    }
}
