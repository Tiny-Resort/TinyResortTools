namespace Mirror
{
	public struct SceneMessage : NetworkMessage
	{
		public string sceneName;

		public SceneOperation sceneOperation;

		public bool customHandling;
	}
}
