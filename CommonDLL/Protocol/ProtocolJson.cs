using System;
using LitJson;

namespace CommonDLL
{
	public class ProtocolJson : ProtocolBase
	{
		private string Json;
		private JsonData JsonData;

		public ProtocolJson()
		{
		}
		public ProtocolJson(string s)
		{
			Json = s;
			JsonData = JsonMapper.ToObject(Json);
		}
		public ProtocolJson(object o)
		{
			Json = CommonFunction.Class2Json(o);
			JsonData = JsonMapper.ToObject(Json);
		}

		public override ProtocolBase Decode(byte[] readbuff, int start, int length)
		{
			string s = System.Text.Encoding.UTF8.GetString(readbuff, start, length);
			ProtocolJson protocol = new ProtocolJson(s);
			return protocol;
		}

		public override byte[] Encode()
		{
			return System.Text.Encoding.UTF8.GetBytes(Json); ;
		}

		public T GetData<T>()
		{
			return CommonFunction.Json2Class<T>(Json);
		}

		public override string GetName()
		{
			return (string)JsonData["ServerProtoCol"];
		}
		public int GetState()
		{
			return (int)JsonData["State"];
		}
		public JsonData GetJsonData()
		{
			return JsonData;
		}

		public override string GetDesc()
		{
			return Json;
		}
	}
}
