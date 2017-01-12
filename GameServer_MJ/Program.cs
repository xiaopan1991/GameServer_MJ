using System;
using ProtoBuf;
using System.Text;
using System.IO;

namespace GameServer_MJ
{
	
	
	class MainClass
	{
		public static void Main(string[] args)
		{
			//ConnectServer();
			//TestSerialize();
		}


		// 连接服务器
		private static void ConnectServer()
		{
			DataManager.GetInstance().Init();
			RoomManager.GetInstance().Init();
			ServerNet.GetInstance().InitAndStart("192.168.1.131", 1234, ProtocolBaseType.Bytes);
			Console.ReadLine();
		}

		//private static void TestSerialize()
		//{
		//	NetModel item = new NetModel() { ID=1,Commit="LanOu",Message="Unity",Type = "Register"};
		//	byte[] temp = Serialize<NetModel>(item);
		//	Console.WriteLine(temp.Length);

		//	NetModel result = DeSerialize<NetModel>(temp);
		//	Console.WriteLine(result.Type);
		//}

		private static byte[] Serialize<T>(T model)
		{
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					Serializer.Serialize<T>(ms, model);
					byte[] result = new byte[ms.Length];
					ms.Position = 0;
					ms.Read(result, 0, result.Length);
					ms.Close();
					return result;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Serialize Fail: " + ex.Message);
				return null;
			}
		}

		private static T DeSerialize<T>(byte[] msg)
		{
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					ms.Write(msg, 0, msg.Length);
					ms.Position = 0;
					T result = Serializer.Deserialize<T>(ms);
					ms.Close();
					return result;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Deserizlize Fail: " + ex.Message);
				return default(T);
			}
		}
	}
}
