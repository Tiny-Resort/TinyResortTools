using Mirror;
using Mirror.RemoteCalls;

namespace TinyResort {

    public class TRNetwork : NetworkBehaviour {

        #region Remote Commands

        /*
         
         [Command]
	public void CmdActivateTrap(uint animalToTrapId, int xPos, int yPos)
	{
		
	}
	

		
         */

        [Command]
        public void CmdSendMessageToHost(string message) {
	        PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
	        writer.WriteString(message);
	        SendCommandInternal(typeof(TRNetwork), "CmdSendMessageToClient", writer, 0, true);
	        NetworkWriterPool.Recycle(writer);
	        
        }

        internal void UserCode_CmdSendMessageToHost(NetworkConnection con, string message) {
	        TRTools.LogError($"Sending message to client: {message}");
        }


        internal static void InvokeUserCode_CmdSendMessageToHost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) {
            if (!NetworkServer.active) {
                TRTools.LogError("Command CmdSendMessageToHost called on client.");
                return;
            }
            ((TRNetwork)obj).UserCode_CmdSendMessageToHost(NetworkClient.connection, reader.ReadString());

        }

        static TRNetwork() { RemoteCallHelper.RegisterCommandDelegate(typeof(TRNetwork), "CmdSendMessageToHost", new CmdDelegate(TRNetwork.InvokeUserCode_CmdSendMessageToHost), true); }

        #endregion
    }

}
