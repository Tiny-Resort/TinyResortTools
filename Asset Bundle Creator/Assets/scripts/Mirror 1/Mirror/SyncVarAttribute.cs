using System;
using UnityEngine;

namespace Mirror
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SyncVarAttribute : PropertyAttribute
	{
		public string hook;
	}
}
