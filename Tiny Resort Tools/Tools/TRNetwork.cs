using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;

namespace TinyResort {

    public class TRNetwork : NetworkBehaviour {

        // Create a method to add characters to a TRNetwork type (might need a class)
        // Create a method to call the Target/Cmd methods using a list of connected players (to specify who sent to)
        
        internal static TRNetwork instance;
        internal List<TRNetwork> connectedPlayers = new List<TRNetwork>();

        /*public void addPlayerPostfix(CharMovement newChar) {
            if (!this.connectedPlayers.Contains(newChar) && !newChar.isLocalPlayer) { this.connectedPlayers.Add(newChar); }
        }*/


        #region Remote Commands

        private void Awake() {
            TRTools.QuickPatch(typeof(NetworkPlayersManager), "updateStatus", typeof(TRNetwork), null, "addPlayerPostfix");
            
            instance = this;
        }
        
        [Command]
        public void CmdSendMessageToHost(string message) {
	        PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
	        writer.WriteString(message);
	        SendCommandInternal(typeof(TRNetwork), "CmdSendMessageToClient", writer, 0, true);
	        NetworkWriterPool.Recycle(writer);
	        
        }

        internal void UserCode_CmdSendMessageToHost(string message) {
	        TRTools.LogError($"Sending message to host: {message}");
        }
        
        internal static void InvokeUserCode_CmdSendMessageToHost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) {
            if (!NetworkServer.active) {
                TRTools.LogError("Command CmdSendMessageToHost called on client.");
                return;
            }
            ((TRNetwork)obj).UserCode_CmdSendMessageToHost(reader.ReadString());
        }

        [TargetRpc]
        public void TargetSendMessageToClient(NetworkConnection con, string message) {
            PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
            writer.WriteString(message);
            SendCommandInternal(typeof(TRNetwork), "TargetSendMessageToClient", writer, 0, true);
            NetworkWriterPool.Recycle(writer);
        }

        internal void UserCode_TargetSendMessageToClient(NetworkConnection con, string message) { TRTools.LogError($"Sending message to Client: {message}"); }

        internal static void InvokeUserCode_TargetSendMessageToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) {
            if (!NetworkServer.active) {
                TRTools.LogError("Command CmdSendMessageToClient called on client.");
                return;
            }
            ((TRNetwork)obj).UserCode_TargetSendMessageToClient(NetworkClient.connection, reader.ReadString());
        }
        
        static TRNetwork() { 
            RemoteCallHelper.RegisterCommandDelegate(typeof(TRNetwork), "CmdSendMessageToHost", new CmdDelegate(TRNetwork.InvokeUserCode_CmdSendMessageToHost), true);
            RemoteCallHelper.RegisterCommandDelegate(typeof(TRNetwork), "TargetSendMessageToClient", new CmdDelegate(TRNetwork.InvokeUserCode_TargetSendMessageToClient), true); 
        }

        #endregion
    }
}
