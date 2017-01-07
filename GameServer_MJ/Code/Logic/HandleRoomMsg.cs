using System;
namespace SocketTest
{
	public partial class HandlePlayerMsg
	{
		public void MsgGetRoomList(Player player, ProtocolBase protoBase)
		{
			player.Send(RoomManager.GetInstance().GetRoomList());
		}

		public void MsgCreateRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolBytes protocol = new ProtocolBytes();
			protocol.AddString("CreateRoom");
			if (player.tempData.status != PlayerTempData.Status.None)
			{
				Console.WriteLine(string.Format("MsgCreateRoom Fail; id:{0}", player.id));
				protocol.AddInt(-1);
				player.Send(protocol);
				return;
			}

			RoomManager.GetInstance().CreateRoom(player);
			protocol.AddInt(0);
			player.Send(protocol);
			Console.WriteLine(string.Format("MagCreateRoom Seccuss; id:{0}", player.id));
		}

		public void MsgEnterRoom(Player player, ProtocolBase protoBase)
		{
			int start = 0;
			ProtocolBytes protocol = protoBase as ProtocolBytes;
			string protoName = protocol.GetString(start, ref start);
			int index = protocol.GetInt(start, ref start);
			Console.WriteLine("[收到MsgEnterRoom] " + player.id + " " + index);

			protocol = new ProtocolBytes();
			protocol.AddString(protoName);
			if (index < 0 || index >= RoomManager.GetInstance().list.Count)
			{
				Console.WriteLine(string.Format("MsgEnterRoom index err; id:{0}", player.id));
				protocol.AddInt(-1);
				player.Send(protocol);
				return;
			}

			Room room = RoomManager.GetInstance().list[index];
			if (room.status != Room.Status.Prepare)
			{
				Console.WriteLine(string.Format("MsgEnterRoom status err; id:{0}", player.id));
				protocol.AddInt(-1);
				player.Send(protocol);
				return;
			}

			if (room.AddPlayer(player))
			{
				room.Broadcast(room.GetRoomInfo());
				protocol.AddInt(0);
				player.Send(protocol);
			}
			else
			{
				Console.WriteLine("MsgEnterRoom maxPlayer err; id:{0}", player.id);
				protocol.AddInt(-1);
				player.Send(protocol);
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
			player.Send(room.GetRoomInfo());
		}

		public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
		{
			ProtocolBytes protocol = new ProtocolBytes();
			protocol.AddString("LeaveRoom");

			if (player.tempData.status != PlayerTempData.Status.Room)
			{
				Console.WriteLine(string.Format("MsgLeaveRoom status err; id:{0}", player.id));
				protocol.AddInt(-1);
				player.Send(protocol);
				return;
			}
			protocol.AddInt(0);
			player.Send(protocol);
			Room room = player.tempData.room;
			RoomManager.GetInstance().LeaveRoom(player);

			if (room != null)
				room.Broadcast(room.GetRoomInfo());
		}
	}
}
