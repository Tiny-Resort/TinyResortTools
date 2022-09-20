using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Mirror
{
	public static class Utils
	{
		public static uint GetTrueRandomUInt()
		{
			using (RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider())
			{
				byte[] array = new byte[4];
				rNGCryptoServiceProvider.GetBytes(array);
				return BitConverter.ToUInt32(array, 0);
			}
		}

		public static bool IsPrefab(GameObject obj)
		{
			return false;
		}

		public static bool IsSceneObjectWithPrefabParent(GameObject gameObject, out GameObject prefab)
		{
			prefab = null;
			if (prefab == null)
			{
				Debug.LogError("Failed to find prefab parent for scene object [name:" + gameObject.name + "]");
				return false;
			}
			return true;
		}

		public static bool IsPointInScreen(Vector2 point)
		{
			if (0f <= point.x && point.x < (float)Screen.width && 0f <= point.y)
			{
				return point.y < (float)Screen.height;
			}
			return false;
		}
	}
}
