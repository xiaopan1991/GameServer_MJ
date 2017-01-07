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
			ServerNet.GetInstance().InitAndStart("127.0.0.1", 1234, ProtocolBaseType.Bytes);
			Console.ReadLine();
		}
	}
}
