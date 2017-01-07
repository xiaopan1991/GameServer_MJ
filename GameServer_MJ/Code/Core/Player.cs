using System;
namespace SocketTest
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

		public void Send(ProtocolBase proto)
		{
			if (conn == null)
				return;
			ServerNet.GetInstance().Send(conn, proto);
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

		public static bool KickOff(string id, ProtocolBase proto)
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
						if (proto != null)
							conns[i].player.Send(proto);

						return conns[i].player.Logout();
					}
				}

			}
			return true;
		}
	}
}
