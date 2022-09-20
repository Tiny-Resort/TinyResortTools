using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mirror
{
	public static class NetworkWriterExtensions
	{
		private static readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

		private static readonly byte[] stringBuffer = new byte[32768];

		public static void WriteByte(this NetworkWriter writer, byte value)
		{
			writer.WriteByte(value);
		}

		public static void WriteSByte(this NetworkWriter writer, sbyte value)
		{
			writer.WriteByte((byte)value);
		}

		public static void WriteChar(this NetworkWriter writer, char value)
		{
			writer.WriteUShort(value);
		}

		[Obsolete("We've cleaned up the API. Use WriteBool instead.")]
		public static void WriteBoolean(this NetworkWriter writer, bool value)
		{
			writer.WriteBool(value);
		}

		public static void WriteBool(this NetworkWriter writer, bool value)
		{
			writer.WriteByte((byte)(value ? 1u : 0u));
		}

		[Obsolete("We've cleaned up the API. Use WriteUShort instead.")]
		public static void WriteUInt16(this NetworkWriter writer, ushort value)
		{
			writer.WriteUShort(value);
		}

		public static void WriteUShort(this NetworkWriter writer, ushort value)
		{
			writer.WriteByte((byte)value);
			writer.WriteByte((byte)(value >> 8));
		}

		[Obsolete("We've cleaned up the API. Use WriteShort instead.")]
		public static void WriteInt16(this NetworkWriter writer, short value)
		{
			writer.WriteShort(value);
		}

		public static void WriteShort(this NetworkWriter writer, short value)
		{
			writer.WriteUShort((ushort)value);
		}

		[Obsolete("We've cleaned up the API. Use WriteUInt instead.")]
		public static void WriteUInt32(this NetworkWriter writer, uint value)
		{
			writer.WriteUInt(value);
		}

		public static void WriteUInt(this NetworkWriter writer, uint value)
		{
			writer.WriteByte((byte)value);
			writer.WriteByte((byte)(value >> 8));
			writer.WriteByte((byte)(value >> 16));
			writer.WriteByte((byte)(value >> 24));
		}

		[Obsolete("We've cleaned up the API. Use WriteInt instead.")]
		public static void WriteInt32(this NetworkWriter writer, int value)
		{
			writer.WriteInt(value);
		}

		public static void WriteInt(this NetworkWriter writer, int value)
		{
			writer.WriteUInt((uint)value);
		}

		[Obsolete("We've cleaned up the API. Use WriteULong instead.")]
		public static void WriteUInt64(this NetworkWriter writer, ulong value)
		{
			writer.WriteULong(value);
		}

		public static void WriteULong(this NetworkWriter writer, ulong value)
		{
			writer.WriteByte((byte)value);
			writer.WriteByte((byte)(value >> 8));
			writer.WriteByte((byte)(value >> 16));
			writer.WriteByte((byte)(value >> 24));
			writer.WriteByte((byte)(value >> 32));
			writer.WriteByte((byte)(value >> 40));
			writer.WriteByte((byte)(value >> 48));
			writer.WriteByte((byte)(value >> 56));
		}

		[Obsolete("We've cleaned up the API. Use WriteLong instead.")]
		public static void WriteInt64(this NetworkWriter writer, long value)
		{
			writer.WriteLong(value);
		}

		public static void WriteLong(this NetworkWriter writer, long value)
		{
			writer.WriteULong((ulong)value);
		}

		[Obsolete("We've cleaned up the API. Use WriteFloat instead.")]
		public static void WriteSingle(this NetworkWriter writer, float value)
		{
			writer.WriteFloat(value);
		}

		public static void WriteFloat(this NetworkWriter writer, float value)
		{
			UIntFloat uIntFloat = default(UIntFloat);
			uIntFloat.floatValue = value;
			UIntFloat uIntFloat2 = uIntFloat;
			writer.WriteUInt(uIntFloat2.intValue);
		}

		public static void WriteDouble(this NetworkWriter writer, double value)
		{
			UIntDouble uIntDouble = default(UIntDouble);
			uIntDouble.doubleValue = value;
			UIntDouble uIntDouble2 = uIntDouble;
			writer.WriteULong(uIntDouble2.longValue);
		}

		public static void WriteDecimal(this NetworkWriter writer, decimal value)
		{
			UIntDecimal uIntDecimal = default(UIntDecimal);
			uIntDecimal.decimalValue = value;
			UIntDecimal uIntDecimal2 = uIntDecimal;
			writer.WriteULong(uIntDecimal2.longValue1);
			writer.WriteULong(uIntDecimal2.longValue2);
		}

		public static void WriteString(this NetworkWriter writer, string value)
		{
			if (value == null)
			{
				writer.WriteUShort(0);
				return;
			}
			int bytes = encoding.GetBytes(value, 0, value.Length, stringBuffer, 0);
			if (bytes >= 32768)
			{
				throw new IndexOutOfRangeException("NetworkWriter.Write(string) too long: " + bytes + ". Limit: " + 32768);
			}
			writer.WriteUShort(checked((ushort)(bytes + 1)));
			writer.WriteBytes(stringBuffer, 0, bytes);
		}

		public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				writer.WriteUInt(0u);
				return;
			}
			writer.WriteUInt(checked((uint)count) + 1);
			writer.WriteBytes(buffer, offset, count);
		}

		public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer)
		{
			writer.WriteBytesAndSize(buffer, 0, (buffer != null) ? buffer.Length : 0);
		}

		public static void WriteBytesAndSizeSegment(this NetworkWriter writer, ArraySegment<byte> buffer)
		{
			writer.WriteBytesAndSize(buffer.Array, buffer.Offset, buffer.Count);
		}

		public static void WriteVector2(this NetworkWriter writer, Vector2 value)
		{
			writer.WriteFloat(value.x);
			writer.WriteFloat(value.y);
		}

		public static void WriteVector3(this NetworkWriter writer, Vector3 value)
		{
			writer.WriteFloat(value.x);
			writer.WriteFloat(value.y);
			writer.WriteFloat(value.z);
		}

		public static void WriteVector3Nullable(this NetworkWriter writer, Vector3? value)
		{
			writer.WriteBool(value.HasValue);
			if (value.HasValue)
			{
				writer.WriteVector3(value.Value);
			}
		}

		public static void WriteVector4(this NetworkWriter writer, Vector4 value)
		{
			writer.WriteFloat(value.x);
			writer.WriteFloat(value.y);
			writer.WriteFloat(value.z);
			writer.WriteFloat(value.w);
		}

		public static void WriteVector2Int(this NetworkWriter writer, Vector2Int value)
		{
			writer.WriteInt(value.x);
			writer.WriteInt(value.y);
		}

		public static void WriteVector3Int(this NetworkWriter writer, Vector3Int value)
		{
			writer.WriteInt(value.x);
			writer.WriteInt(value.y);
			writer.WriteInt(value.z);
		}

		public static void WriteColor(this NetworkWriter writer, Color value)
		{
			writer.WriteFloat(value.r);
			writer.WriteFloat(value.g);
			writer.WriteFloat(value.b);
			writer.WriteFloat(value.a);
		}

		public static void WriteColor32(this NetworkWriter writer, Color32 value)
		{
			writer.WriteByte(value.r);
			writer.WriteByte(value.g);
			writer.WriteByte(value.b);
			writer.WriteByte(value.a);
		}

		public static void WriteQuaternion(this NetworkWriter writer, Quaternion value)
		{
			writer.WriteFloat(value.x);
			writer.WriteFloat(value.y);
			writer.WriteFloat(value.z);
			writer.WriteFloat(value.w);
		}

		public static void WriteQuaternionNullable(this NetworkWriter writer, Quaternion? value)
		{
			writer.WriteBool(value.HasValue);
			if (value.HasValue)
			{
				writer.WriteQuaternion(value.Value);
			}
		}

		public static void WriteRect(this NetworkWriter writer, Rect value)
		{
			writer.WriteFloat(value.xMin);
			writer.WriteFloat(value.yMin);
			writer.WriteFloat(value.width);
			writer.WriteFloat(value.height);
		}

		public static void WritePlane(this NetworkWriter writer, Plane value)
		{
			writer.WriteVector3(value.normal);
			writer.WriteFloat(value.distance);
		}

		public static void WriteRay(this NetworkWriter writer, Ray value)
		{
			writer.WriteVector3(value.origin);
			writer.WriteVector3(value.direction);
		}

		public static void WriteMatrix4x4(this NetworkWriter writer, Matrix4x4 value)
		{
			writer.WriteFloat(value.m00);
			writer.WriteFloat(value.m01);
			writer.WriteFloat(value.m02);
			writer.WriteFloat(value.m03);
			writer.WriteFloat(value.m10);
			writer.WriteFloat(value.m11);
			writer.WriteFloat(value.m12);
			writer.WriteFloat(value.m13);
			writer.WriteFloat(value.m20);
			writer.WriteFloat(value.m21);
			writer.WriteFloat(value.m22);
			writer.WriteFloat(value.m23);
			writer.WriteFloat(value.m30);
			writer.WriteFloat(value.m31);
			writer.WriteFloat(value.m32);
			writer.WriteFloat(value.m33);
		}

		public static void WriteGuid(this NetworkWriter writer, Guid value)
		{
			byte[] array = value.ToByteArray();
			writer.WriteBytes(array, 0, array.Length);
		}

		public static void WriteNetworkIdentity(this NetworkWriter writer, NetworkIdentity value)
		{
			if (value == null)
			{
				writer.WriteUInt(0u);
			}
			else
			{
				writer.WriteUInt(value.netId);
			}
		}

		public static void WriteNetworkBehaviour(this NetworkWriter writer, NetworkBehaviour value)
		{
			if (value == null)
			{
				writer.WriteUInt(0u);
				return;
			}
			writer.WriteUInt(value.netId);
			writer.WriteByte((byte)value.ComponentIndex);
		}

		public static void WriteTransform(this NetworkWriter writer, Transform value)
		{
			if (value == null)
			{
				writer.WriteUInt(0u);
				return;
			}
			NetworkIdentity component = value.GetComponent<NetworkIdentity>();
			if (component != null)
			{
				writer.WriteUInt(component.netId);
				return;
			}
			Debug.LogWarning("NetworkWriter " + (((object)value != null) ? value.ToString() : null) + " has no NetworkIdentity");
			writer.WriteUInt(0u);
		}

		public static void WriteGameObject(this NetworkWriter writer, GameObject value)
		{
			if (value == null)
			{
				writer.WriteUInt(0u);
				return;
			}
			NetworkIdentity component = value.GetComponent<NetworkIdentity>();
			if (component != null)
			{
				writer.WriteUInt(component.netId);
				return;
			}
			Debug.LogWarning("NetworkWriter " + (((object)value != null) ? value.ToString() : null) + " has no NetworkIdentity");
			writer.WriteUInt(0u);
		}

		public static void WriteUri(this NetworkWriter writer, Uri uri)
		{
			writer.WriteString(((object)uri != null) ? uri.ToString() : null);
		}

		public static void WriteList<T>(this NetworkWriter writer, List<T> list)
		{
			if (list == null)
			{
				writer.WriteInt(-1);
				return;
			}
			writer.WriteInt(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				writer.Write(list[i]);
			}
		}

		public static void WriteArray<T>(this NetworkWriter writer, T[] array)
		{
			if (array == null)
			{
				writer.WriteInt(-1);
				return;
			}
			writer.WriteInt(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				writer.Write(array[i]);
			}
		}

		public static void WriteArraySegment<T>(this NetworkWriter writer, ArraySegment<T> segment)
		{
			int count = segment.Count;
			writer.WriteInt(count);
			for (int i = 0; i < count; i++)
			{
				writer.Write(segment.Array[segment.Offset + i]);
			}
		}
	}
}
