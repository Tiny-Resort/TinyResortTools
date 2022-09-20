using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Mirror
{
	public static class NetworkReaderExtensions
	{
		private static readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

		public static byte ReadByte(this NetworkReader reader)
		{
			return reader.ReadByte();
		}

		public static sbyte ReadSByte(this NetworkReader reader)
		{
			return (sbyte)reader.ReadByte();
		}

		public static char ReadChar(this NetworkReader reader)
		{
			return (char)reader.ReadUShort();
		}

		[Obsolete("We've cleaned up the API. Use ReadBool instead.")]
		public static bool ReadBoolean(this NetworkReader reader)
		{
			return reader.ReadBool();
		}

		public static bool ReadBool(this NetworkReader reader)
		{
			return reader.ReadByte() != 0;
		}

		[Obsolete("We've cleaned up the API. Use ReadShort instead.")]
		public static short ReadInt16(this NetworkReader reader)
		{
			return reader.ReadShort();
		}

		public static short ReadShort(this NetworkReader reader)
		{
			return (short)reader.ReadUShort();
		}

		[Obsolete("We've cleaned up the API. Use ReadUShort instead.")]
		public static ushort ReadUInt16(this NetworkReader reader)
		{
			return reader.ReadUShort();
		}

		public static ushort ReadUShort(this NetworkReader reader)
		{
			return (ushort)((ushort)(0u | reader.ReadByte()) | (ushort)(reader.ReadByte() << 8));
		}

		[Obsolete("We've cleaned up the API. Use ReadInt instead.")]
		public static int ReadInt32(this NetworkReader reader)
		{
			return reader.ReadInt();
		}

		public static int ReadInt(this NetworkReader reader)
		{
			return (int)reader.ReadUInt();
		}

		[Obsolete("We've cleaned up the API. Use ReadUInt instead.")]
		public static uint ReadUInt32(this NetworkReader reader)
		{
			return reader.ReadUInt();
		}

		public static uint ReadUInt(this NetworkReader reader)
		{
			return 0u | reader.ReadByte() | (uint)(reader.ReadByte() << 8) | (uint)(reader.ReadByte() << 16) | (uint)(reader.ReadByte() << 24);
		}

		[Obsolete("We've cleaned up the API. Use ReadLong instead.")]
		public static long ReadInt64(this NetworkReader reader)
		{
			return reader.ReadLong();
		}

		public static long ReadLong(this NetworkReader reader)
		{
			return (long)reader.ReadULong();
		}

		[Obsolete("We've cleaned up the API. Use ReadULong instead.")]
		public static ulong ReadUInt64(this NetworkReader reader)
		{
			return reader.ReadULong();
		}

		public static ulong ReadULong(this NetworkReader reader)
		{
			return 0uL | (ulong)reader.ReadByte() | ((ulong)reader.ReadByte() << 8) | ((ulong)reader.ReadByte() << 16) | ((ulong)reader.ReadByte() << 24) | ((ulong)reader.ReadByte() << 32) | ((ulong)reader.ReadByte() << 40) | ((ulong)reader.ReadByte() << 48) | ((ulong)reader.ReadByte() << 56);
		}

		[Obsolete("We've cleaned up the API. Use ReadFloat instead.")]
		public static float ReadSingle(this NetworkReader reader)
		{
			return reader.ReadFloat();
		}

		public static float ReadFloat(this NetworkReader reader)
		{
			UIntFloat uIntFloat = default(UIntFloat);
			uIntFloat.intValue = reader.ReadUInt();
			return uIntFloat.floatValue;
		}

		public static double ReadDouble(this NetworkReader reader)
		{
			UIntDouble uIntDouble = default(UIntDouble);
			uIntDouble.longValue = reader.ReadULong();
			return uIntDouble.doubleValue;
		}

		public static decimal ReadDecimal(this NetworkReader reader)
		{
			UIntDecimal uIntDecimal = default(UIntDecimal);
			uIntDecimal.longValue1 = reader.ReadULong();
			uIntDecimal.longValue2 = reader.ReadULong();
			return uIntDecimal.decimalValue;
		}

		public static string ReadString(this NetworkReader reader)
		{
			ushort num = reader.ReadUShort();
			if (num == 0)
			{
				return null;
			}
			int num2 = num - 1;
			if (num2 >= 32768)
			{
				throw new EndOfStreamException("ReadString too long: " + num2 + ". Limit is: " + 32768);
			}
			ArraySegment<byte> arraySegment = reader.ReadBytesSegment(num2);
			return encoding.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
		}

		public static byte[] ReadBytesAndSize(this NetworkReader reader)
		{
			uint num = reader.ReadUInt();
			if (num != 0)
			{
				return ReadBytes(reader, checked((int)(num - 1u)));
			}
			return null;
		}

		public static ArraySegment<byte> ReadBytesAndSizeSegment(this NetworkReader reader)
		{
			uint num = reader.ReadUInt();
			if (num != 0)
			{
				return reader.ReadBytesSegment(checked((int)(num - 1u)));
			}
			return default(ArraySegment<byte>);
		}

		public static Vector2 ReadVector2(this NetworkReader reader)
		{
			return new Vector2(reader.ReadFloat(), reader.ReadFloat());
		}

		public static Vector3 ReadVector3(this NetworkReader reader)
		{
			return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
		}

		public static Vector3? ReadVector3Nullable(this NetworkReader reader)
		{
			return reader.ReadBool() ? reader.ReadVector3() : default(Vector3);
		}

		public static Vector4 ReadVector4(this NetworkReader reader)
		{
			return new Vector4(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
		}

		public static Vector2Int ReadVector2Int(this NetworkReader reader)
		{
			return new Vector2Int(reader.ReadInt(), reader.ReadInt());
		}

		public static Vector3Int ReadVector3Int(this NetworkReader reader)
		{
			return new Vector3Int(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}

		public static Color ReadColor(this NetworkReader reader)
		{
			return new Color(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
		}

		public static Color32 ReadColor32(this NetworkReader reader)
		{
			return new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
		}

		public static Quaternion ReadQuaternion(this NetworkReader reader)
		{
			return new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
		}

		public static Quaternion? ReadQuaternionNullable(this NetworkReader reader)
		{
			return reader.ReadBool() ? reader.ReadQuaternion() : default(Quaternion);
		}

		public static Rect ReadRect(this NetworkReader reader)
		{
			return new Rect(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
		}

		public static Plane ReadPlane(this NetworkReader reader)
		{
			return new Plane(reader.ReadVector3(), reader.ReadFloat());
		}

		public static Ray ReadRay(this NetworkReader reader)
		{
			return new Ray(reader.ReadVector3(), reader.ReadVector3());
		}

		public static Matrix4x4 ReadMatrix4x4(this NetworkReader reader)
		{
			Matrix4x4 result = default(Matrix4x4);
			result.m00 = reader.ReadFloat();
			result.m01 = reader.ReadFloat();
			result.m02 = reader.ReadFloat();
			result.m03 = reader.ReadFloat();
			result.m10 = reader.ReadFloat();
			result.m11 = reader.ReadFloat();
			result.m12 = reader.ReadFloat();
			result.m13 = reader.ReadFloat();
			result.m20 = reader.ReadFloat();
			result.m21 = reader.ReadFloat();
			result.m22 = reader.ReadFloat();
			result.m23 = reader.ReadFloat();
			result.m30 = reader.ReadFloat();
			result.m31 = reader.ReadFloat();
			result.m32 = reader.ReadFloat();
			result.m33 = reader.ReadFloat();
			return result;
		}

		public static byte[] ReadBytes(this NetworkReader reader, int count)
		{
			byte[] array = new byte[count];
			reader.ReadBytes(array, count);
			return array;
		}

		public static Guid ReadGuid(this NetworkReader reader)
		{
			return new Guid(reader.ReadBytes(16));
		}

		public static Transform ReadTransform(this NetworkReader reader)
		{
			NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
			if (!(networkIdentity != null))
			{
				return null;
			}
			return networkIdentity.transform;
		}

		public static GameObject ReadGameObject(this NetworkReader reader)
		{
			NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
			if (!(networkIdentity != null))
			{
				return null;
			}
			return networkIdentity.gameObject;
		}

		public static NetworkIdentity ReadNetworkIdentity(this NetworkReader reader)
		{
			uint num = reader.ReadUInt();
			if (num == 0)
			{
				return null;
			}
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(num, out value))
			{
				return value;
			}
			return null;
		}

		public static NetworkBehaviour ReadNetworkBehaviour(this NetworkReader reader)
		{
			uint num = reader.ReadUInt();
			if (num == 0)
			{
				return null;
			}
			byte b = reader.ReadByte();
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(num, out value))
			{
				return value.NetworkBehaviours[b];
			}
			return null;
		}

		public static T ReadNetworkBehaviour<T>(this NetworkReader reader) where T : NetworkBehaviour
		{
			return reader.ReadNetworkBehaviour() as T;
		}

		public static NetworkBehaviour.NetworkBehaviourSyncVar ReadNetworkBehaviourSyncVar(this NetworkReader reader)
		{
			uint num = reader.ReadUInt();
			byte componentIndex = 0;
			if (num != 0)
			{
				componentIndex = reader.ReadByte();
			}
			return new NetworkBehaviour.NetworkBehaviourSyncVar(num, componentIndex);
		}

		public static List<T> ReadList<T>(this NetworkReader reader)
		{
			int num = reader.ReadInt();
			if (num < 0)
			{
				return null;
			}
			List<T> list = new List<T>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(reader.Read<T>());
			}
			return list;
		}

		public static T[] ReadArray<T>(this NetworkReader reader)
		{
			int num = reader.ReadInt();
			if (num < 0)
			{
				return null;
			}
			if (num > reader.Length - reader.Position)
			{
				throw new EndOfStreamException(string.Format("Received array that is too large: {0}", num));
			}
			T[] array = new T[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = reader.Read<T>();
			}
			return array;
		}

		public static Uri ReadUri(this NetworkReader reader)
		{
			string text = reader.ReadString();
			if (!string.IsNullOrEmpty(text))
			{
				return new Uri(text);
			}
			return null;
		}
	}
}
