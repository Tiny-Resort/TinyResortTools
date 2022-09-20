using System.IO;

namespace kcp2k
{
	internal class Segment
	{
		internal uint conv;

		internal uint cmd;

		internal uint frg;

		internal uint wnd;

		internal uint ts;

		internal uint sn;

		internal uint una;

		internal uint resendts;

		internal int rto;

		internal uint fastack;

		internal uint xmit;

		internal MemoryStream data = new MemoryStream(1200);

		internal int Encode(byte[] ptr, int offset)
		{
			int num = offset;
			offset += Utils.Encode32U(ptr, offset, conv);
			offset += Utils.Encode8u(ptr, offset, (byte)cmd);
			offset += Utils.Encode8u(ptr, offset, (byte)frg);
			offset += Utils.Encode16U(ptr, offset, (ushort)wnd);
			offset += Utils.Encode32U(ptr, offset, ts);
			offset += Utils.Encode32U(ptr, offset, sn);
			offset += Utils.Encode32U(ptr, offset, una);
			offset += Utils.Encode32U(ptr, offset, (uint)data.Position);
			return offset - num;
		}

		internal void Reset()
		{
			conv = 0u;
			cmd = 0u;
			frg = 0u;
			wnd = 0u;
			ts = 0u;
			sn = 0u;
			una = 0u;
			rto = 0;
			xmit = 0u;
			resendts = 0u;
			fastack = 0u;
			data.SetLength(0L);
		}
	}
}
