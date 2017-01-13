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
			RegisterData registerData = protocol.GetData<RegisterData>();
			string strFormat = "[收到注册协议]" + conn.GetAdress();
			Console.WriteLine(strFormat + " 用户名: " + registerData.UserName + "  密码: " + registerData.PassWord);

			JsonData Jdata = new JsonData();
			Jdata["ServerProtoCol"] = registerData.ServerProtoCol;

			if (DataManager.GetInstance().Register(registerData.UserName, registerData.PassWord))
			{
				Jdata["State"] = 0;
				DataManager.GetInstance().CreatePlayer(registerData.UserName);
			}
			else
			{
				Jdata["State"] = -1;
			}
			protocol = new ProtocolJson(Jdata.ToJson());
			conn.Send(protocol);
		}

		public void MsgLogin(Conn conn, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			LoginData loginData = protocol.GetData<LoginData>();
			string strFormat = "[收到登录协议]" + conn.GetAdress();
			Console.WriteLine(strFormat + "  用户名: " + loginData.UserName + "  密码: " + loginData.PassWord);

			JsonData Jdata = new JsonData();
			Jdata["ServerProtoCol"] = loginData.ServerProtoCol;

			if (!DataManager.GetInstance().CheckPassWord(loginData.UserName, loginData.PassWord))
			{
				Jdata["State"] = -1;
				protocol = new ProtocolJson(Jdata.ToJson());
				conn.Send(protocol);
				return;
			}

			JsonData logoutJsonData = new JsonData();
			logoutJsonData["ServerProtoCol"] = "Logout";

			ProtocolJson protocolLogout = new ProtocolJson(logoutJsonData.ToJson());
			if (!Player.KickOff(loginData.UserName, protocolLogout))
			{
				Jdata["State"] = -1;
				protocol = new ProtocolJson(Jdata.ToJson());
				conn.Send(protocol);
				return;
			}

			PlayerData playerData = DataManager.GetInstance().GetPlayerData(loginData.UserName);
			if (playerData == null)
			{
				Jdata["State"] = -1;
				protocol = new ProtocolJson(Jdata.ToJson());
				conn.Send(protocol);
				return;
			}

			conn.player = new Player(loginData.UserName, conn);
			conn.player.data = playerData;

			ServerNet.GetInstance().handlePlayerEvent.OnLogin(conn.player);

			Jdata["State"] = 0;
			protocol = new ProtocolJson(Jdata.ToJson());
			conn.Send(protocol);
		}

		public void MsgLogout(Conn conn, ProtocolBase protoBase)
		{
			JsonData Jdata = new JsonData();
			Jdata["ServerProtoCol"] = "Logout";
			Jdata["State"] = 0;

			ProtocolJson protocol = new ProtocolJson(Jdata.ToJson());
			conn.Send(protocol);
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
