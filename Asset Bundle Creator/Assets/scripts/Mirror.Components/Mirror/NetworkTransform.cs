using UnityEngine;

namespace Mirror
{
	[DisallowMultipleComponent]
	public class NetworkTransform : NetworkTransformBase
	{
		protected override Transform targetComponent
		{
			get
			{
				return base.transform;
			}
		}

		private void MirrorProcessed()
		{
		}
	}
}
