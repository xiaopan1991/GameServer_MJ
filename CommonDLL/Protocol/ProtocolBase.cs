using LitJson;

namespace CommonDLL
{
	public enum ProtocolBaseType
	{
		Bytes,
		String,
		Json,
		Protobuf
	}
	
	public class ProtocolBase
	{
		//解码
		public virtual ProtocolBase Decode(byte[] readbuff, int start, int length)
		{
			return new ProtocolBase();
		}
		//编码
		public virtual byte[] Encode()
		{
			return new byte[] { };
		}
		public virtual byte[] Encode<T>(T data)
		{
			return new byte[] { };
		}
		//协议名称，用于消息分发
		public virtual string GetName()
		{
			return string.Empty;;
		}
		public virtual string GetDesc()
		{
			return string.Empty;
		}
	}
}
