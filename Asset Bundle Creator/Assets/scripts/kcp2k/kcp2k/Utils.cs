using System.Runtime.CompilerServices;

namespace kcp2k
{
	public static class Utils
	{
		public static int Clamp(int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		public static int Encode8u(byte[] p, int offset, byte c)
		{
			p[offset] = c;
			return 1;
		}

		public static int Decode8u(byte[] p, int offset, ref byte c)
		{
			c = p[offset];
			return 1;
		}

		public static int Encode16U(byte[] p, int offset, ushort w)
		{
			p[offset] = (byte)w;
			p[1 + offset] = (byte)(w >> 8);
			return 2;
		}

		public static int Decode16U(byte[] p, int offset, ref ushort c)
		{
			ushort num = 0;
			num = (ushort)(num | p[offset]);
			num = (ushort)(num | (ushort)(p[1 + offset] << 8));
			c = num;
			return 2;
		}

		public static int Encode32U(byte[] p, int offset, uint l)
		{
			p[offset] = (byte)l;
			p[1 + offset] = (byte)(l >> 8);
			p[2 + offset] = (byte)(l >> 16);
			p[3 + offset] = (byte)(l >> 24);
			return 4;
		}

		public static int Decode32U(byte[] p, int offset, ref uint c)
		{
			uint num = 0u;
			num |= p[offset];
			num |= (uint)(p[1 + offset] << 8);
			num |= (uint)(p[2 + offset] << 16);
			num |= (uint)(p[3 + offset] << 24);
			c = num;
			return 4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TimeDiff(uint later, uint earlier)
		{
			return (int)(later - earlier);
		}
	}
}
