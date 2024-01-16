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

    /*[SyncVar]
    public List<Chest> allChests;*/

    public readonly SyncList<Chest> allChests = new();

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

    public override void OnStartServer() => allChests.AddRange(ContainerManager.manage.activeChests);

    // public override void OnStartClient() => RpcCustomRPC("TESTTESTTEST",ContainerManager.manage.activeChests);

    #region Send To Host

    /*
     * Sends a message from the Host -> Host and from Client -> Host
     */

    [TargetRpc]
    public void CmdSendMessageToHost() {
        var writer = NetworkWriterPool.GetWriter();
        writer.WriteList(ContainerManager.manage.activeChests);
        writer.WriteUInt(NetworkMapSharer.Instance.localChar.netId);
        SendCommandInternal(typeof(TRNetwork), "CmdSendMessageToHost", writer, 0, false);
        NetworkWriterPool.Recycle(writer);
    }

    protected static void InvokeUserCode_CmdSendMessageToHost(
        NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection
    ) =>
        ((TRNetwork)obj).UserCode_CmdSendMessageToHost(reader.ReadList<Chest>());

    protected void UserCode_CmdSendMessageToHost(List<Chest> message) {
        share.RpcCustomRPC(ContainerManager.manage.activeChests);
        TRTools.LogError("CmdSendMessageToHost");
    }

    #endregion

    #region Send to Single Client

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

    #endregion

    #region Send To All

    /*
         * Sends a message from the to both Host and Client, but only from Host.
         */
    [ClientRpc]
    public void RpcCustomRPC(List<Chest> chests) {
        var writer = NetworkWriterPool.GetWriter();
        writer.WriteList(chests);
        SendRPCInternal(typeof(TRNetwork), "CustomRPC", writer, 0, false);
        NetworkWriterPool.Recycle(writer);
    }

    protected static void InvokeUserCode_CustomRPC(
        NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection
    ) =>
        ((TRNetwork)obj).UserCode_CustomRPC(reader.ReadList<Chest>());

    protected void UserCode_CustomRPC(List<Chest> type) {
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

        RemoteCallHelper.RegisterRpcDelegate(typeof(TRNetwork), "CustomRPC", new CmdDelegate(InvokeUserCode_CustomRPC));
        RemoteCallHelper.RegisterCommandDelegate(
            typeof(TRNetwork), "CmdSendMessageToHost", new CmdDelegate(InvokeUserCode_CmdSendMessageToHost), false
        );
        RemoteCallHelper.RegisterCommandDelegate(
            typeof(TRNetwork), "TargetSendMessageToClient", new CmdDelegate(InvokeUserCode_TargetSendMessageToClient),
            false
        );
    }
}
