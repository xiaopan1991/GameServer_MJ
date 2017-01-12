using System;
using System.IO;
using ProtoBuf;
using System.Linq;

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

		public override byte[] Encode<T>(T data)
		{
			try
			{
				MemoryStream ms = new MemoryStream();
				Serializer.Serialize<T>(ms, data);
				byte[] lenBytes = BitConverter.GetBytes(ms.Length);

				byte[] result = new byte[ms.Length];
				ms.Position = 0;
				ms.Read(result, 0, result.Length);
				ms.Close();

				bytes = lenBytes.Concat(result).ToArray();

				return bytes;
			}
			catch (Exception ex)
			{
				Console.WriteLine("序列化失败: " + ex.ToString());
				return null;
			}
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
