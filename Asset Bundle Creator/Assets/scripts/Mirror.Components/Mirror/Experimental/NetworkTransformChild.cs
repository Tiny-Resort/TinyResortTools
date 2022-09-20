using UnityEngine;

namespace Mirror.Experimental
{
	[AddComponentMenu("Network/Experimental/NetworkTransformChildExperimentalExperimental")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-transform-child")]
	public class NetworkTransformChild : NetworkTransformBase
	{
		[Header("Target")]
		public Transform target;

		protected override Transform targetTransform
		{
			get
			{
				return target;
			}
		}

		private void MirrorProcessed()
		{
		}
	}
}
