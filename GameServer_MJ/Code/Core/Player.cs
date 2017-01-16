using System;
using CommonDLL;
using LitJson;

namespace GameServer_MJ
{
	public class Player
	{
		public string id;
		public Conn conn;
		public PlayerData data;
		public PlayerTempData tempData;
		
		public Player(string id, Conn conn)
		{
			this.id = id;
			this.conn = conn;
			tempData = new PlayerTempData();
		}

		//public void Send(ProtocolBase proto)
		//{
		//	if (conn == null)
		//		return;
		//	ServerNet.GetInstance().Send(conn, proto);
		//}

		public void Send(string ServerName, JsonData data=null)
		{
			if (conn == null)
				return;
			conn.Send(ServerName, data);
		}

		public bool Logout()
		{
			//ServerNet.GetInstance().handlePlayerEvent.OnLogout(this);
			if (!DataManager.GetInstance().SavePlayer(this))
				return false;

			conn.player = null;
			conn.Close();

			return true;
		}

		public static bool KickOff(string id)
		{
			Conn[] conns = ServerNet.GetInstance().conns;
			for (int i = 0; i < conns.Length; i++)
			{
				if (conns[i] == null) continue;
				if (!conns[i].isUse) continue;
				if (conns[i].player == null) continue;

				if (conns[i].player.id == id)
				{
					lock(conns[i].player)
					{
						conns[i].player.Send("Logout", null);

						return conns[i].player.Logout();
					}
				}

			}
			return true;
		}
	}
}
