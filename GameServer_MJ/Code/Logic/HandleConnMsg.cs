using System;
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
			int start = 0;
			ProtocolBytes protocol = protoBase as ProtocolBytes;
			string protoName = protocol.GetString(start, ref start);
			string id = protocol.GetString(start, ref start);
			string pw = protocol.GetString(start, ref start);
			string strFormat = "[收到注册协议]" + conn.GetAdress();
			Console.WriteLine(strFormat + " 用户名: " + id + "  密码: " + pw);
			protocol = new ProtocolBytes();
			protocol.AddString(protoName);
			if (DataManager.GetInstance().Register(id, pw))
			{
				protocol.AddInt(0);
			}
			else
			{
				protocol.AddInt(-1);
			}
			DataManager.GetInstance().CreatePlayer(id);
			conn.Send(protoBase);
		}

		public void MsgLogin(Conn conn, ProtocolBase protoBase)
		{
			int start = 0;
			ProtocolBytes protocol = protoBase as ProtocolBytes;
			string protoName = protocol.GetString(start, ref start);
			string id = protocol.GetString(start, ref start);
			string pw = protocol.GetString(start, ref start);

			string strFormat = "[收到登录协议]" + conn.GetAdress();
			Console.WriteLine(strFormat + "  用户名: " + id + "  密码: " + pw);

			ProtocolBytes protocolRet = new ProtocolBytes();
			protocolRet.AddString(protoName);

			if (!DataManager.GetInstance().CheckPassWord(id, pw))
			{
				protocolRet.AddInt(-1);
				conn.Send(protocolRet);
				return;
			}

			ProtocolBytes protocolLogout = new ProtocolBytes();
			protocolLogout.AddString("Logout");
			if (!Player.KickOff(id, protocolLogout))
			{
				protocolRet.AddInt(-1);
				conn.Send(protocolRet);
				return;
			}

			PlayerData playerData = DataManager.GetInstance().GetPlayerData(id);
			if (playerData == null)
			{
				protocolRet.AddInt(-1);
				conn.Send(protocolRet);
				return;
			}

			conn.player = new Player(id, conn);
			conn.player.data = playerData;

			ServerNet.GetInstance().handlePlayerEvent.OnLogin(conn.player);

			protocolRet.AddInt(0);
			conn.Send(protocolRet);
		}

		public void MsgLogout(Conn conn, ProtocolBase protoBase)
		{
			ProtocolBytes protocol = new ProtocolBytes();
			protocol.AddString("Logout");
			protocol.AddInt(0);
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
