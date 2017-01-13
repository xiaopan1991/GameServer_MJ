using LitJson;
namespace CommonDLL
{
	public static class CommonFunction
	{
		public static string Class2Json(object t)
		{
			var json_bill = JsonMapper.ToJson(t);
			return json_bill;
		}

		public static T Json2Class<T>(string s)
		{
			var t = JsonMapper.ToObject<T>(s);
			return t;
		}
	}
}
