using System;
namespace GameServer_MJ
{
	public class ProtocolProtoBuf : ProtocolBase
	{
		public byte[] bytes;

		public override ProtocolBase Decode(byte[] readbuff, int start, int length)
		{
			ProtocolProtoBuf protocol = new ProtocolProtoBuf();
			protocol.bytes = new byte[length];
			Array.Copy(readbuff, start, protocol.bytes, 0, length);
			return protocol;
		}

		public override byte[] Encode()
		{
			return bytes;
		}

		public override string GetName()
		{
			return string.Empty;
		}

		public override string GetDesc()
		{
			string str = string.Empty;

			if (bytes == null) return str;
			for (int i = 0; i < bytes.Length; i++)
			{
				int b = (int)bytes[i];
				str += b.ToString() + " ";
			}

			return str;
		}

		///////////辅助方法//////////
	}
}
