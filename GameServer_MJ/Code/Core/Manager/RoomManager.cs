using System;
using System.Collections.Generic;

namespace SocketTest
{
	public class RoomManager
	{
		private static RoomManager instance = null;

		public List<Room> list = null;

		public static RoomManager GetInstance()
		{
			if (instance == null)
				instance = new RoomManager();
			return instance;
		}

		public RoomManager()
		{
			
		}

		public void Init()
		{
			list = new List<Room>();
		}

		public void CreateRoom(Player player)
		{
			Room room = new Room();
			lock(list)
			{
				list.Add(room);
				room.AddPlayer(player);
			}
		}

		public void LeaveRoom(Player player)
		{
			PlayerTempData tempData = player.tempData;
			if (tempData.status == PlayerTempData.Status.None)
				return;
			Room room = tempData.room;
			lock(list)
			{
				room.DelPlayer(player.id);
				if (room.list.Count == 0)
					list.Remove(room);
			}
		}

		public ProtocolBytes GetRoomList()
		{
			ProtocolBytes protocol = new ProtocolBytes();
			protocol.AddString("GetRoomList");
			int count = list.Count;

			protocol.AddInt(count);
			for (int i = 0; i < count; i++)
			{
				Room room = list[i];
				protocol.AddInt(room.list.Count);
				protocol.AddInt((int)room.status);
			}
			return protocol;
		}
	}
}
