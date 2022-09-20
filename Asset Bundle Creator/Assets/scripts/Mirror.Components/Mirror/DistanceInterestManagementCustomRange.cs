using UnityEngine;

namespace Mirror
{
	[RequireComponent(typeof(NetworkIdentity))]
	[DisallowMultipleComponent]
	public class DistanceInterestManagementCustomRange : MonoBehaviour
	{
		[Tooltip("The maximum range that objects will be visible at.")]
		public int visRange = 20;
	}
}
