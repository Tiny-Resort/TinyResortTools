using UnityEngine;

namespace Mirror
{
	public class NetworkTransformChild : NetworkTransformBase
	{
		[Header("Target")]
		public Transform target;

		protected override Transform targetComponent
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
