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

        public void CmdSendMessageToClient(string message) {
	        PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
	        writer.WriteString(message);
	        SendCommandInternal(typeof(TRNetwork), "CmdSendMessageToClient", writer, 0, true);
	        NetworkWriterPool.Recycle(writer);
	        
        }

        internal void UserCode_CmdSendMessageToClient(NetworkConnection con, string message) {
	        TRTools.LogError($"Sending message to client: {message}");
        }


        internal static void InvokeUserCode_CmdSendMessageToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection) {
            if (!NetworkServer.active) {
                TRTools.LogError("Command CmdSendMessageToClient called on client.");
                return;
            }
            ((TRNetwork)obj).UserCode_CmdSendMessageToClient(NetworkClient.connection, reader.ReadString());

        }

        static TRNetwork() { RemoteCallHelper.RegisterCommandDelegate(typeof(TRNetwork), "CmdSendMessageToClient", new CmdDelegate(TRNetwork.InvokeUserCode_CmdSendMessageToClient), true); }

        #endregion
    }

}
