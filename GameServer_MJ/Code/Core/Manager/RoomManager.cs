using System;
using System.Collections.Generic;
using LitJson;
using CommonDLL;

namespace GameServer_MJ
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

		public void CreateRoom(Player player, RoomOptions options=null)
		{
			Room room = new Room();
			if (options != null)
				room.Options = options;
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

		public ProtocolJson GetRoomList()
		{
			JsonData rData = new JsonData();
			rData["ServerProtoCol"] = "GetRoomList";
			rData["RoomList"] = new JsonData();
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Room room = list[i];
				JsonData jdata = new JsonData();
				jdata["RoomName"] = room.Options.RoomName;
				jdata["Count"] = room.list.Count;
				jdata["Status"] = (int)room.status;
				rData["RoomList"].Add(jdata);
			}

			ProtocolJson protocol = new ProtocolJson(rData.ToJson());
			return protocol;
		}
	}
}
