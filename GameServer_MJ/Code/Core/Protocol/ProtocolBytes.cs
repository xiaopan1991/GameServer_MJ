using System;
using System.Linq;

namespace GameServer_MJ
{
	public class ProtocolBytes : ProtocolBase
	{
		public byte[] bytes;

		public override ProtocolBase Decode(byte[] readbuff, int start, int length)
		{
			ProtocolBytes protocol = new ProtocolBytes();
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
			return GetString(0);
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

		public void AddString(string str)
		{
			Int32 len = str.Length;
			byte[] lenBytes = BitConverter.GetBytes(len);
			byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
			bytes = (bytes == null) ? 
				lenBytes.Concat(strBytes).ToArray() : 
			    bytes.Concat(lenBytes).Concat(strBytes).ToArray();
		}
		public string GetString(int start, ref int end)
		{
			if (bytes == null) return string.Empty;
			if (bytes.Length < start + sizeof(Int32)) return string.Empty;
			Int32 strLen = BitConverter.ToInt32(bytes, start);
			if (bytes.Length < strLen + start + sizeof(Int32)) return string.Empty;

			string str = System.Text.Encoding.UTF8.GetString(bytes,start+sizeof(Int32),strLen);
			end = start + sizeof(Int32) + strLen;
			return str;
		}
		public string GetString(int start)
		{
			int end = 0;
			return GetString(start, ref end);
		}

		public void AddInt(int num)
		{
			byte[] numBytes = BitConverter.GetBytes(num);
			bytes = (bytes == null) ? numBytes : bytes.Concat(numBytes).ToArray();
		}
		public int GetInt(int start, ref int end)
		{
			if (bytes == null) return 0;
			if (bytes.Length < start + sizeof(Int32)) return 0;
			end = start + sizeof(Int32);
			return BitConverter.ToInt32(bytes, start);
		}
		public int GetInt(int start)
		{
			int end = 0;
			return GetInt(start, ref end);
		}

		public void AddFloat(float num)
		{
			byte[] numBytes = BitConverter.GetBytes(num);
			bytes = (bytes == null) ? numBytes : bytes.Concat(numBytes).ToArray();
		}
		public float GetFloat(int start, ref int end)
		{
			if (bytes == null) return 0;
			if (bytes.Length < start + sizeof(float)) return 0;
			end = start + sizeof(float);
			return BitConverter.ToSingle(bytes, start);
		}
		public float GetFloat(int start)
		{
			int end = 0;
			return GetInt(start, ref end);
		}
	}
}
