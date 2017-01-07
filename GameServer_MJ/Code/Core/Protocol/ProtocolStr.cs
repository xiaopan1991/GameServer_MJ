using System;
namespace GameServer_MJ
{
	public class ProtocolStr : ProtocolBase
	{
		public string str;

		public override ProtocolBase Decode(byte[] readbuff, int start, int length)
		{
			ProtocolStr protocol = new ProtocolStr();
			protocol.str = System.Text.Encoding.UTF8.GetString(readbuff, start, length);
			return protocol;
		}

		public override byte[] Encode()
		{
			byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
			return b;
		}

		public override string GetName()
		{
			return (str.Length == 0) ? string.Empty : str.Split(',')[0];
		}

		public override string GetDesc()
		{
			return this.str;
		}
	}
}
