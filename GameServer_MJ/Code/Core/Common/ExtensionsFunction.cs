using System;
using LitJson;
namespace GameServer_MJ
{
	public static class ExtensionsFunction
	{
		public static void Add(this JsonData data, string key, JsonData vvalue)
		{
			data[key] = vvalue;
		}
	}
}
