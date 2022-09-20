namespace Mirror
{
	public interface SyncObject
	{
		bool IsDirty { get; }

		void Flush();

		void OnSerializeAll(NetworkWriter writer);

		void OnSerializeDelta(NetworkWriter writer);

		void OnDeserializeAll(NetworkReader reader);

		void OnDeserializeDelta(NetworkReader reader);

		void Reset();
	}
}
