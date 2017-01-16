using System;
using CommonDLL;
using System.Text;
using LitJson;

namespace GameServer_MJ
{
	public partial class HandleConnMsg
	{
		public void MsgHeartBeat(Conn conn, ProtocolBase protoBase)
		{
			conn.lastTickTime = Sys.GetTimeStamp();
			Console.WriteLine("[更新心跳时间] " + conn.GetAdress());
		}

		public void MsgRegister(Conn conn, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string UserName = (string)protocol.GetValue("UserName");
			string Password = (string)protocol.GetValue("Password");
			Console.WriteLine(string.Format("[收到注册协议]:{0};  用户名:{1};  密码:   {2}", conn.GetAdress(), UserName, Password));

			JsonData SendData = new JsonData();
			if (DataManager.GetInstance().Register(UserName, Password))
			{
				SendData["State"] = 0;
				DataManager.GetInstance().CreatePlayer(UserName);
			}
			else
			{
				SendData["State"] = -1;
			}
			conn.Send(protocol.GetName(), SendData);
		}

		public void MsgLogin(Conn conn, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string UserName = (string)protocol.GetValue("UserName");
			string Password = (string)protocol.GetValue("Password");
			string ServerName = protocol.GetName();
			Console.WriteLine(string.Format("[收到登录协议]:{0};  用户名:{1};  密码:   {2}", conn.GetAdress(), UserName, Password));

			JsonData SendData = new JsonData();

			if (!DataManager.GetInstance().CheckPassWord(UserName, Password))
			{
				SendData["State"] = -1;
				conn.Send(ServerName, SendData);
				return;
			}


			if (!Player.KickOff(UserName))
			{
				SendData["State"] = -1;
				conn.Send(ServerName, SendData);
				return;
			}

			PlayerData playerData = DataManager.GetInstance().GetPlayerData(UserName);
			if (playerData == null)
			{
				SendData["State"] = -1;
				conn.Send(ServerName, SendData);
				return;
			}

			conn.player = new Player(UserName, conn);
			conn.player.data = playerData;

			ServerNet.GetInstance().handlePlayerEvent.OnLogin(conn.player);

			SendData["State"] = 0;
			conn.Send(ServerName, SendData);
		}

		public void MsgLogout(Conn conn, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string ServerName = protocol.GetName();

			JsonData SendData = new JsonData();
			SendData["State"] = 0;

			conn.Send(ServerName, SendData);
			if (conn.player == null)
			{
				conn.Close();
			}
			else
			{
				conn.player.Logout();
			}
		}
	}
}
