using UnityEngine;

namespace Aevus
{
	public class CreateInfiniteLoopDemo : MonoBehaviour
	{
		public bool infiniteLoopInUpdate;

		public bool infiniteLoopInOnDrawGizmos;

		private void Update()
		{
			while (infiniteLoopInUpdate)
			{
			}
		}

		private void FixedUpdate()
		{
			base.transform.Rotate(0.77f, 0.66f, 0.88f);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(base.transform.position, Vector3.right);
			while (infiniteLoopInOnDrawGizmos)
			{
			}
		}
	}
}
