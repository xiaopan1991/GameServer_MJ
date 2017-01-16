using System;
using LitJson;
using CommonDLL;

namespace GameServer_MJ
{
	public partial class HandlePlayerMsg
	{
		public void MsgGetRoomList(Player player, ProtocolBase protoBase)
		{
			player.Send(StaticValue.Server_GetRoomList, RoomManager.GetInstance().GetRoomList());
		}

		public void MsgCreateRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string ServerName = protocol.GetName();
			string RoomName = (string)protocol.GetValue("RoomName");

			JsonData SendData = new JsonData();
			if (player.tempData.status != PlayerTempData.Status.None)
			{
				SendData["State"] = -1;
				player.Send(ServerName, SendData);
				Console.WriteLine(string.Format("MsgCreateRoom Fail; id:{0}", player.id));
				return;
			}

			RoomOptions options = new RoomOptions(RoomName);
			RoomManager.GetInstance().CreateRoom(player, options);

			SendData = RoomManager.GetInstance().GetRoomList();
			SendData["State"] = 0;
			player.Send(ServerName, SendData);
			Console.WriteLine(string.Format("MagCreateRoom Seccuss; id:{0}", player.id));
		}

		public void MsgEnterRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string ServerName = protocol.GetName();
			int Index = (int)protocol.GetValue("Index");
			Console.WriteLine(string.Format("[收到MsgEnterRoom] UserName:{0}; Index:{1}" , player.id, Index));

			JsonData SendData = new JsonData();

			if (Index < 0 || Index >= RoomManager.GetInstance().list.Count)
			{
				Console.WriteLine(string.Format("MsgEnterRoom index err; id:{0}", player.id));
				SendData["State"] = -1;

				player.Send(ServerName, SendData);
				return;
			}

			Room room = RoomManager.GetInstance().list[Index];
			if (room.status != Room.Status.Prepare)
			{
				Console.WriteLine(string.Format("MsgEnterRoom status err; id:{0}", player.id));
				SendData["State"] = -1;
				player.Send(ServerName, SendData);
				return;
			}

			if (room.AddPlayer(player))
			{
				room.Broadcast(StaticValue.Server_GetRoomList, RoomManager.GetInstance().GetRoomList());

				SendData["State"] = 0;
				player.Send(ServerName, SendData);
			}
			else
			{
				Console.WriteLine("MsgEnterRoom maxPlayer err; id:{0}", player.id);
				SendData["State"] = -1;
				player.Send(ServerName, SendData);
			}
		}

		public void MsgGetRoomInfo(Player player, ProtocolBase protoBase)
		{
			if (player.tempData.status != PlayerTempData.Status.Room)
			{
				Console.WriteLine(string.Format("MsgGetRoomInfo status err; id:{0}", player.id));
				return;
			}
			Room room = player.tempData.room;
			player.Send(StaticValue.Server_GetRoomInfo, room.GetRoomInfo());
		}

		public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolJson protocol = protoBase as ProtocolJson;
			string ServerName = protocol.GetName();

			JsonData SendData = new JsonData();
			if (player.tempData.status != PlayerTempData.Status.Room)
			{
				Console.WriteLine(string.Format("MsgLeaveRoom status err; id:{0}", player.id));
				SendData["State"] = -1;
				player.Send(ServerName, SendData);
				return;
			}

			SendData["State"] = 0;
			player.Send(ServerName, SendData);
			Room room = player.tempData.room;
			RoomManager.GetInstance().LeaveRoom(player);

			if (room != null)
				room.Broadcast(StaticValue.Server_GetRoomInfo, room.GetRoomInfo());
		}
	}
}
