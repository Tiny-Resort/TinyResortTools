using System;
using System.Reflection;
using Mirror;
using Mirror.RemoteCalls;
using HarmonyLib;
using UnityEngine;

namespace TinyResort {
    public class TRNetwork : NetworkBehaviour {
        public static TRNetwork share;

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

        public override void OnStartServer() { }

        public override void OnStartClient() { }

        #region Send To Host

        /*
         * Sends a message from the Host -> Host and from Client -> Host
         */
        [Command]
        public void CmdSendMessageToHost(string message) {
            var writer = NetworkWriterPool.GetWriter();
            writer.WriteString(message);
            writer.WriteUInt(NetworkMapSharer.Instance.localChar.netId);
            SendCommandInternal(typeof(TRNetwork), "CmdSendMessageToHost", writer, 0, false);
            NetworkWriterPool.Recycle(writer);
        }

        protected static void InvokeUserCode_CmdSendMessageToHost(
            NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection
        ) =>
            ((TRNetwork)obj).UserCode_CmdSendMessageToHost(reader.ReadString());

        protected void UserCode_CmdSendMessageToHost(string message) {
            if (isServer) TRTools.LogError(message);

        }

        #endregion

        #region Send to Single Client

        [TargetRpc]
        public void TargetSendMessageToClient(NetworkConnection con, string message) {
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
        public void RpcCustomRPC(string type) {
            var writer = NetworkWriterPool.GetWriter();
            writer.WriteString(type);
            SendRPCInternal(typeof(TRNetwork), "CustomRPC", writer, 0, false);
            NetworkWriterPool.Recycle(writer);
        }

        protected static void InvokeUserCode_CustomRPC(
            NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection
        ) =>
            ((TRNetwork)obj).UserCode_CustomRPC(reader.ReadString());

        protected void UserCode_CustomRPC(string type) => TRTools.LogError($"{type}");

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
}
