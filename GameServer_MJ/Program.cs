using System;

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
			ServerNet.GetInstance().InitAndStart("192.168.1.131", 1234, ProtocolBaseType.Bytes);
			Console.ReadLine();
		}
	}
}
