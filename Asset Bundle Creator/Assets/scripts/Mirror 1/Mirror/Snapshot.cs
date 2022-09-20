namespace Mirror
{
	public interface Snapshot
	{
		double remoteTimestamp { get; set; }

		double localTimestamp { get; set; }
	}
}
