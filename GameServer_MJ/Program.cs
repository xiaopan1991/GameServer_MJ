using System;
using System.Text;
using System.IO;
using CommonDLL;

namespace GameServer_MJ
{
	
	class MainClass
	{
		public static void Main(string[] args)
		{
			ConnectServer();
		}

		// 连接服务器
		private static void ConnectServer()
		{
			DataManager.GetInstance().Init();
			RoomManager.GetInstance().Init();
			ServerNet.GetInstance().InitAndStart(StaticValue.IP, StaticValue.PORT, ProtocolBaseType.Json);
			Console.ReadLine();
		}
	}
}
